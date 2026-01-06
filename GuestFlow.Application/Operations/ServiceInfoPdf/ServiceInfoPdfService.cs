using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.ServiceInfoPdf
{
    /// <summary>
    /// Service Information PDF Service Implementation.
    /// Generates NON-FINANCIAL service information PDFs.
    /// 
    /// These PDFs contain ONLY operational details:
    /// - Date, time, pickup location
    /// - Notes and reminders
    /// - Guest information
    /// 
    /// These PDFs do NOT contain:
    /// - Prices
    /// - Payment information
    /// - Financial totals
    /// 
    /// This is NOT an invoice - do not confuse with Invoice PDF.
    /// </summary>
    public class ServiceInfoPdfService : IServiceInfoPdfService
    {
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly ILogger<ServiceInfoPdfService> _logger;

        public ServiceInfoPdfService(
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            ILogger<ServiceInfoPdfService> logger)
        {
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _logger = logger;
        }

        /// <summary>
        /// Generate a Service Information PDF for a Transfer.
        /// Contains operational details only - NO prices, NO payment info.
        /// </summary>
        public async Task<ServiceMessage<ServiceInfoPdfResult>> GenerateTransferInfoPdfAsync(int transferId)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Include(t => t.PickupCity)
                    .Include(t => t.DropoffCity)
                    .FirstOrDefaultAsync(t => t.Id == transferId);

                if (transfer == null)
                {
                    return new ServiceMessage<ServiceInfoPdfResult>
                    {
                        IsSuccess = false,
                        Message = "Transfer bulunamadı."
                    };
                }

                // Build PDF content (non-financial)
                var pdfContent = BuildTransferInfoContent(transfer);
                
                // Generate PDF (placeholder - actual PDF generation would use a PDF library)
                var pdfUrl = await GeneratePdfFile(pdfContent, "Transfer", transferId);

                _logger.LogInformation($"Service Info PDF generated for Transfer #{transferId}");

                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = true,
                    Message = "Servis bilgi PDF'i başarıyla oluşturuldu.",
                    Data = new ServiceInfoPdfResult
                    {
                        PdfUrl = pdfUrl,
                        GeneratedDate = DateTime.UtcNow,
                        ServiceType = "Transfer",
                        ServiceId = transferId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Service Info PDF for Transfer #{transferId}");
                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = false,
                    Message = $"PDF oluşturulurken hata: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate a Service Information PDF for a City Tour.
        /// </summary>
        public async Task<ServiceMessage<ServiceInfoPdfResult>> GenerateCityTourInfoPdfAsync(int cityTourId)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetAll()
                    .Include(t => t.OwnerGuest)
                    .Include(t => t.Personnel)
                    .Include(t => t.City)
                    .Include(t => t.Vehicle)
                    .FirstOrDefaultAsync(t => t.Id == cityTourId);

                if (cityTour == null)
                {
                    return new ServiceMessage<ServiceInfoPdfResult>
                    {
                        IsSuccess = false,
                        Message = "Şehir turu bulunamadı."
                    };
                }

                var pdfContent = BuildCityTourInfoContent(cityTour);
                var pdfUrl = await GeneratePdfFile(pdfContent, "CityTour", cityTourId);

                _logger.LogInformation($"Service Info PDF generated for CityTour #{cityTourId}");

                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = true,
                    Message = "Servis bilgi PDF'i başarıyla oluşturuldu.",
                    Data = new ServiceInfoPdfResult
                    {
                        PdfUrl = pdfUrl,
                        GeneratedDate = DateTime.UtcNow,
                        ServiceType = "CityTour",
                        ServiceId = cityTourId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Service Info PDF for CityTour #{cityTourId}");
                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = false,
                    Message = $"PDF oluşturulurken hata: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Generate a Service Information PDF for a Yacht Tour.
        /// </summary>
        public async Task<ServiceMessage<ServiceInfoPdfResult>> GenerateYachtTourInfoPdfAsync(int yachtTourId)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetAll()
                    .Include(t => t.OwnerGuest)
                    .Include(t => t.Personnel)
                    .Include(t => t.City)
                    .FirstOrDefaultAsync(t => t.Id == yachtTourId);

                if (yachtTour == null)
                {
                    return new ServiceMessage<ServiceInfoPdfResult>
                    {
                        IsSuccess = false,
                        Message = "Yat turu bulunamadı."
                    };
                }

                var pdfContent = BuildYachtTourInfoContent(yachtTour);
                var pdfUrl = await GeneratePdfFile(pdfContent, "YachtTour", yachtTourId);

                _logger.LogInformation($"Service Info PDF generated for YachtTour #{yachtTourId}");

                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = true,
                    Message = "Servis bilgi PDF'i başarıyla oluşturuldu.",
                    Data = new ServiceInfoPdfResult
                    {
                        PdfUrl = pdfUrl,
                        GeneratedDate = DateTime.UtcNow,
                        ServiceType = "YachtTour",
                        ServiceId = yachtTourId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating Service Info PDF for YachtTour #{yachtTourId}");
                return new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = false,
                    Message = $"PDF oluşturulurken hata: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Regenerate Service Information PDF when service details change.
        /// </summary>
        public async Task<ServiceMessage<ServiceInfoPdfResult>> RegenerateServiceInfoPdfAsync(string serviceType, int serviceId)
        {
            return serviceType.ToLower() switch
            {
                "transfer" => await GenerateTransferInfoPdfAsync(serviceId),
                "citytour" => await GenerateCityTourInfoPdfAsync(serviceId),
                "yachttour" => await GenerateYachtTourInfoPdfAsync(serviceId),
                _ => new ServiceMessage<ServiceInfoPdfResult>
                {
                    IsSuccess = false,
                    Message = $"Bilinmeyen servis tipi: {serviceType}"
                }
            };
        }

        #region Private Helper Methods

        /// <summary>
        /// Build transfer info content - NO PRICES, NO PAYMENT INFO
        /// </summary>
        private ServiceInfoContent BuildTransferInfoContent(TransferEntity transfer)
        {
            return new ServiceInfoContent
            {
                Title = "TRANSFER BİLGİLERİ",
                // NOTE: This is NOT an invoice - no financial information
                Sections = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Title = "MİSAFİR BİLGİLERİ",
                        Items = new Dictionary<string, string>
                        {
                            { "Misafir", transfer.Guest?.FullName ?? "N/A" },
                            { "Telefon", transfer.Guest?.PhoneNumber ?? "N/A" },
                            { "Oda", transfer.Guest?.RoomNumber ?? "N/A" }
                        }
                    },
                    new ContentSection
                    {
                        Title = "TRANSFER DETAYLARI",
                        Items = new Dictionary<string, string>
                        {
                            { "Tarih", transfer.TransferDate.ToString("dd.MM.yyyy HH:mm") },
                            { "Alış Adresi", transfer.PickupAddress ?? "N/A" },
                            { "Alış Şehri", transfer.PickupCity?.CityName ?? "N/A" },
                            { "Bırakış Adresi", transfer.DropoffAddress ?? "N/A" },
                            { "Bırakış Şehri", transfer.DropoffCity?.CityName ?? "N/A" }
                        }
                    },
                    new ContentSection
                    {
                        Title = "ARAÇ & ŞOFÖR",
                        Items = new Dictionary<string, string>
                        {
                            { "Araç", transfer.Vehicle?.PlateNumber ?? transfer.ExternalVehiclePlate ?? "N/A" },
                            { "Şoför", transfer.Personnel?.FullName ?? transfer.ExternalDriverName ?? "N/A" },
                            { "Şoför Tel", transfer.ExternalDriverPhone ?? "N/A" }
                        }
                    }
                },
                Notes = transfer.Note,
                Footer = "Bu belge sadece bilgilendirme amaçlıdır. Fatura değildir."
            };
        }

        /// <summary>
        /// Build city tour info content - NO PRICES, NO PAYMENT INFO
        /// </summary>
        private ServiceInfoContent BuildCityTourInfoContent(CityTourEntity cityTour)
        {
            return new ServiceInfoContent
            {
                Title = "ŞEHİR TURU BİLGİLERİ",
                Sections = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Title = "MİSAFİR BİLGİLERİ",
                        Items = new Dictionary<string, string>
                        {
                            { "Misafir", cityTour.OwnerGuest?.FullName ?? "N/A" },
                            { "Telefon", cityTour.OwnerGuest?.PhoneNumber ?? "N/A" }
                        }
                    },
                    new ContentSection
                    {
                        Title = "TUR DETAYLARI",
                        Items = new Dictionary<string, string>
                        {
                            { "Tarih", cityTour.TourDate.ToString("dd.MM.yyyy") },
                            { "Başlangıç Saati", cityTour.StartTime?.ToString(@"hh\:mm") ?? "N/A" },
                            { "Bitiş Saati", cityTour.EndTime?.ToString(@"hh\:mm") ?? "N/A" },
                            { "Süre", $"{cityTour.DurationHours} saat" },
                            { "Şehir", cityTour.City?.CityName ?? "N/A" },
                            { "Dil", cityTour.Language ?? "N/A" }
                        }
                    },
                    new ContentSection
                    {
                        Title = "REHBER & ARAÇ",
                        Items = new Dictionary<string, string>
                        {
                            { "Rehber", cityTour.GuideName ?? "N/A" },
                            { "Rehber Tel", cityTour.GuidePhone ?? "N/A" },
                            { "Araç", cityTour.Vehicle?.PlateNumber ?? cityTour.ExternalVehiclePlate ?? "N/A" },
                            { "Şoför", cityTour.DriverName ?? cityTour.ExternalDriverName ?? "N/A" }
                        }
                    }
                },
                Footer = "Bu belge sadece bilgilendirme amaçlıdır. Fatura değildir."
            };
        }

        /// <summary>
        /// Build yacht tour info content - NO PRICES, NO PAYMENT INFO
        /// </summary>
        private ServiceInfoContent BuildYachtTourInfoContent(YachtTourEntity yachtTour)
        {
            return new ServiceInfoContent
            {
                Title = "YAT TURU BİLGİLERİ",
                Sections = new List<ContentSection>
                {
                    new ContentSection
                    {
                        Title = "MİSAFİR BİLGİLERİ",
                        Items = new Dictionary<string, string>
                        {
                            { "Misafir", yachtTour.OwnerGuest?.FullName ?? "N/A" },
                            { "Telefon", yachtTour.OwnerGuest?.PhoneNumber ?? "N/A" },
                            { "Kişi Sayısı", yachtTour.NumberOfPeople.ToString() }
                        }
                    },
                    new ContentSection
                    {
                        Title = "TUR DETAYLARI",
                        Items = new Dictionary<string, string>
                        {
                            { "Tarih", yachtTour.TourDate.ToString("dd.MM.yyyy") },
                            { "Başlangıç Saati", yachtTour.StartTime?.ToString(@"hh\:mm") ?? "N/A" },
                            { "Bitiş Saati", yachtTour.EndTime?.ToString(@"hh\:mm") ?? "N/A" },
                            { "Yat Adı", yachtTour.YachtName ?? "N/A" },
                            { "Şehir", yachtTour.City?.CityName ?? "N/A" }
                        }
                    },
                    new ContentSection
                    {
                        Title = "İSKELE BİLGİLERİ",
                        Items = new Dictionary<string, string>
                        {
                            { "Alış İskelesi", yachtTour.PickupPier ?? "N/A" },
                            { "Bırakış İskelesi", yachtTour.DropoffPier ?? "N/A" },
                            { "Kaptan Tel", yachtTour.CaptainPhone ?? "N/A" }
                        }
                    }
                },
                Notes = yachtTour.SpecialRequest,
                Footer = "Bu belge sadece bilgilendirme amaçlıdır. Fatura değildir."
            };
        }

        /// <summary>
        /// Generate PDF file from content (placeholder - actual implementation would use PDF library)
        /// </summary>
        private async Task<string> GeneratePdfFile(ServiceInfoContent content, string serviceType, int serviceId)
        {
            // TODO: Implement actual PDF generation using a library like QuestPDF, iTextSharp, etc.
            // For now, return a placeholder URL
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var pdfUrl = $"/pdfs/service-info/{serviceType.ToLower()}/{serviceId}/{timestamp}.pdf";
            
            await Task.CompletedTask; // Placeholder for async PDF generation
            
            return pdfUrl;
        }

        #endregion
    }

    #region Helper DTOs

    /// <summary>
    /// Service info PDF content structure
    /// </summary>
    internal class ServiceInfoContent
    {
        public string Title { get; set; } = string.Empty;
        public List<ContentSection> Sections { get; set; } = new();
        public string? Notes { get; set; }
        public string Footer { get; set; } = string.Empty;
    }

    /// <summary>
    /// Content section for service info PDF
    /// </summary>
    internal class ContentSection
    {
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, string> Items { get; set; } = new();
    }

    #endregion
}

