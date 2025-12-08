using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Invoice
{
    public class InvoiceManager : IInvoiceService
    {
        // Bu değişkenleri sınıfın içinde kullanıyoruz.
        // _invoiceRepository: Faturalarla ilgili veritabanı işlemlerini yapmak için kullanıyoruz.
        // _guestRepository: Misafir bilgilerini almak için kullanıyoruz.
        // _personnelRepository: Personel bilgilerini almak için kullanıyoruz.
        // _pdfService: PDF oluşturmak için kullanıyoruz.
        // _emailService: E-posta göndermek için kullanıyoruz.
        // _configuration: Konfigürasyon ayarlarına erişmek için kullanıyoruz.
        // _logger: Hataları veya bilgileri loglamak (kaydetmek) için kullanıyoruz.
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoiceManager> _logger;
        private readonly IPdfUrlService _pdfUrlService;

        // Constructor (yapıcı metod): Bu sınıf oluşturulurken bağımlılıkları (dependency) buradan alıyoruz.
        public InvoiceManager(
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<InvoiceManager> logger,
            IPdfUrlService pdfUrlService)
        {
            _invoiceRepository = invoiceRepository;
            _guestRepository = guestRepository;
            _personnelRepository = personnelRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _pdfService = pdfService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _pdfUrlService = pdfUrlService;
        }

        // Bu metod, belirli bir faturayı ID'sine göre getiriyor.
        public async Task<GetInvoiceDto> GetInvoiceById(int id)
        {
            try
            {
                // Veritabanından faturayı ID'sine göre çekiyoruz.
                // GetByIdAsync metodu, verilen ID ile eşleşen faturayı bulur.
                var invoice = await _invoiceRepository.GetByIdAsync(id);

                // Eğer fatura bulunamazsa, bir hata fırlatıyoruz (exception throw ediyoruz).
                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                // Fatura bulunduysa, onu bir DTO (Data Transfer Object) nesnesine çevirip geri döndürüyoruz.
                // DTO, veriyi taşımak için kullandığımız bir yapı. Burada fatura bilgilerini GetInvoiceDto'ya aktarıyoruz.
                return new GetInvoiceDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalAmount = invoice.TotalAmount,
                    IssueDate = invoice.IssueDate,
                    Currency = invoice.Currency,
                    Notes = invoice.Notes,
                    PdfUrl = invoice.PdfUrl,
                    GuestId = invoice.GuestId,
                    PersonnelId = invoice.PersonnelId,
                    TransferId = invoice.TransferId,
                    CityTourId = invoice.CityTourId,
                    YachtTourId = invoice.YachtTourId,
                    CreatedDate = invoice.CreatedDate
                };
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz (kaydediyoruz).
                // LogError metodu, hatayı ve detaylarını kaydeder. Burada fatura ID'sini de ekledik ki hangi faturada sorun olduğunu bilelim.
                _logger.LogError(ex, $"Fatura getirilirken hata: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw). Böylece bu metodu çağıran yer hatayı yakalayabilir.
                throw;
            }
        }

        // Bu metod, tüm faturaları getiriyor.
        public async Task<List<GetInvoiceDto>> GetInvoices()
        {
            try
            {
                // Veritabanından tüm faturaları çekiyoruz.
                // GetAll metodu tüm faturaları alır, Select ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                return await _invoiceRepository.GetAll()
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl,
                        GuestId = i.GuestId,
                        PersonnelId = i.PersonnelId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz.
                // Burada tüm faturaları çekerken bir sorun olduysa, hatayı ve varsa iç hatayı (InnerException) kaydediyoruz.
                _logger.LogError(ex, $"Faturalar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw).
                throw;
            }
        }

        // Bu metod, belirli bir misafire ait faturaları getiriyor.
        public async Task<List<GetInvoiceDto>> GetInvoicesByGuestId(int guestId)
        {
            try
            {
                // Veritabanından sadece belirli bir misafire (guestId) ait faturaları çekiyoruz.
                // GetAll metodu ile filtreleme yapıyoruz (x => x.GuestId == guestId), sonra Select ile her bir faturayı GetInvoiceDto'ya çeviriyoruz.
                // ToListAsync ile bu verileri bir liste haline getirip döndürüyoruz.
                return await _invoiceRepository.GetAll(x => x.GuestId == guestId)
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl,
                        GuestId = i.GuestId,
                        PersonnelId = i.PersonnelId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Eğer bir hata olursa, bunu logluyoruz.
                // Burada misafire ait faturaları çekerken bir sorun olduysa, hatayı, misafir ID'sini ve varsa iç hatayı (InnerException) kaydediyoruz.
                _logger.LogError(ex, $"Misafire ait faturalar listelenirken hata: {ex.Message}. GuestId: {guestId}. InnerException: {ex.InnerException?.Message}");

                // Hata olduğu için bu hatayı yukarıya fırlatıyoruz (throw).
                throw;
            }
        }

        /// <summary>
        /// Fatura için PDF oluşturur ve PdfUrl'i günceller
        /// </summary>
        public async Task<string> GeneratePdfForInvoiceAsync(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .Include(i => i.Personnel)
                    .Include(i => i.Transfer)
                    .Include(i => i.CityTour)
                    .Include(i => i.YachtTour)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                var guest = invoice.Guest ?? await _guestRepository.GetByIdAsync(invoice.GuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                PersonnelEntity? personnel = null;
                if (invoice.PersonnelId.HasValue)
                {
                    personnel = invoice.Personnel ?? await _personnelRepository.GetByIdAsync(invoice.PersonnelId.Value);
                }

                var pdfUrl = await _pdfService.GenerateInvoicePdfAsync(invoice, guest, personnel);

                // PdfUrl'i güncelle
                invoice.PdfUrl = pdfUrl;
                await _invoiceRepository.UpdateAsync(invoice);

                _logger.LogInformation($"Fatura PDF'i oluşturuldu ve güncellendi. InvoiceId: {invoiceId}, PdfUrl: {pdfUrl}");

                // E-posta gönder (misafir e-postası varsa)
                if (!string.IsNullOrEmpty(guest.Email) && !guest.IsSpecialGuest)
                {
                    try
                    {
                        var fullPdfPath = _pdfUrlService.GetFullFilePathFromUrl(pdfUrl);

                        await _emailService.SendInvoiceEmailAsync(
                            guest.Email,
                            guest.FullName,
                            invoice.InvoiceNumber,
                            fullPdfPath
                        );

                        _logger.LogInformation($"Fatura e-postası gönderildi. InvoiceId: {invoiceId}, Email: {guest.Email}");
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Fatura e-postası gönderilirken hata: {emailEx.Message}. InvoiceId: {invoiceId}");
                        // E-posta hatası fatura oluşturmayı engellemez
                    }
                }

                return pdfUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura PDF'i oluşturulurken hata: {ex.Message}. InvoiceId: {invoiceId}");
                throw;
            }
        }

        public async Task<InvoiceDetailDto> GetInvoiceDetailAsync(int id)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .Include(i => i.Personnel)
                    .Include(i => i.Transfer)
                    .Include(i => i.CityTour)
                    .Include(i => i.YachtTour)
                    .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

                if (invoice == null)
                    throw new Exception("Fatura bulunamadı.");

                // Hizmet bilgisini belirle
                InvoiceServiceDto? service = null;
                if (invoice.TransferId.HasValue && invoice.Transfer != null)
                {
                    service = new InvoiceServiceDto
                    {
                        ServiceType = "Transfer",
                        ServiceId = invoice.Transfer.Id,
                        ServiceName = $"{invoice.Transfer.PickupAddress} → {invoice.Transfer.DropoffAddress}",
                        ServiceDate = invoice.Transfer.TransferDate,
                        ServiceAmount = invoice.Transfer.FinalPrice,
                        AdditionalInfo = invoice.Transfer.Note
                    };
                }
                else if (invoice.CityTourId.HasValue && invoice.CityTour != null)
                {
                    service = new InvoiceServiceDto
                    {
                        ServiceType = "CityTour",
                        ServiceId = invoice.CityTour.Id,
                        ServiceName = $"Şehir Turu - {invoice.CityTour.Language}",
                        ServiceDate = invoice.CityTour.TourDate,
                        ServiceAmount = invoice.CityTour.FinalPrice,
                        AdditionalInfo = $"{invoice.CityTour.DurationHours} saat"
                    };
                }
                else if (invoice.YachtTourId.HasValue && invoice.YachtTour != null)
                {
                    service = new InvoiceServiceDto
                    {
                        ServiceType = "YachtTour",
                        ServiceId = invoice.YachtTour.Id,
                        ServiceName = $"Yat Turu - {invoice.YachtTour.YachtName}",
                        ServiceDate = invoice.YachtTour.TourDate,
                        ServiceAmount = invoice.YachtTour.FinalPrice,
                        AdditionalInfo = $"{invoice.YachtTour.NumberOfPeople} kişi"
                    };
                }

                var detail = new InvoiceDetailDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    IssueDate = invoice.IssueDate,
                    TotalAmount = invoice.TotalAmount,
                    Currency = invoice.Currency,
                    Notes = invoice.Notes,
                    PdfUrl = invoice.PdfUrl ?? string.Empty,
                    CreatedDate = invoice.CreatedDate,
                    Guest = invoice.Guest != null ? new InvoiceGuestDto
                    {
                        Id = invoice.Guest.Id,
                        FullName = invoice.Guest.FullName,
                        GuestCode = invoice.Guest.GuestCode,
                        Email = invoice.Guest.Email,
                        PhoneNumber = invoice.Guest.PhoneNumber,
                        Nationality = invoice.Guest.Nationality,
                        IsSpecialGuest = invoice.Guest.IsSpecialGuest
                    } : null,
                    Personnel = invoice.Personnel != null ? new InvoicePersonnelDto
                    {
                        Id = invoice.Personnel.Id,
                        FullName = invoice.Personnel.FullName,
                        Email = invoice.Personnel.Email,
                        UserType = invoice.Personnel.UserType.ToString()
                    } : null,
                    Service = service
                };

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<PagedResult<GetInvoiceDto>> GetInvoicesPagedAsync(int pageNumber, int pageSize, InvoiceFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .ApplyInvoiceFilters(filters)
                    .ApplyInvoiceSorting(sorting)
                    .Select(i => new GetInvoiceDto
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        TotalAmount = i.TotalAmount,
                        IssueDate = i.IssueDate,
                        Currency = i.Currency,
                        Notes = i.Notes,
                        PdfUrl = i.PdfUrl ?? string.Empty,
                        GuestId = i.GuestId,
                        PersonnelId = i.PersonnelId,
                        TransferId = i.TransferId,
                        CityTourId = i.CityTourId,
                        YachtTourId = i.YachtTourId,
                        CreatedDate = i.CreatedDate
                    });

                return await query.ToPagedResultAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış faturalar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today.AddMonths(-1);
                var end = endDate?.Date ?? today;

                var invoices = await _invoiceRepository.GetAll()
                    .Where(i => i.IssueDate.Date >= start && i.IssueDate.Date <= end && !i.IsDeleted)
                    .ToListAsync();

                var statistics = new InvoiceStatisticsDto
                {
                    TotalInvoices = invoices.Count,
                    InvoicesWithPdf = invoices.Count(i => !string.IsNullOrEmpty(i.PdfUrl)),
                    InvoicesWithoutPdf = invoices.Count(i => string.IsNullOrEmpty(i.PdfUrl)),
                    TotalRevenue = invoices.Sum(i => i.TotalAmount),
                    AverageInvoiceAmount = invoices.Count > 0 ? invoices.Average(i => i.TotalAmount) : 0,
                    TotalGuests = invoices.Select(i => i.GuestId).Distinct().Count()
                };

                // Para birimine göre grupla
                var invoicesByCurrency = invoices
                    .GroupBy(i => i.Currency)
                    .ToDictionary(g => g.Key, g => g.Count());

                var revenueByCurrency = invoices
                    .GroupBy(i => i.Currency)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.TotalAmount));

                statistics.InvoicesByCurrency = invoicesByCurrency;
                statistics.RevenueByCurrency = revenueByCurrency;

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<ServiceMessage> SendInvoiceByEmailAsync(int invoiceId, string? recipientEmail = null)
        {
            try
            {
                var invoice = await _invoiceRepository.GetAll()
                    .Include(i => i.Guest)
                    .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

                if (invoice == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Fatura bulunamadı." };

                // PDF yoksa oluştur
                if (string.IsNullOrEmpty(invoice.PdfUrl))
                {
                    try
                    {
                        var pdfUrl = await GeneratePdfForInvoiceAsync(invoiceId);
                        invoice = await _invoiceRepository.GetAll()
                            .Include(i => i.Guest)
                            .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, $"Fatura PDF'i oluşturulurken hata: {pdfEx.Message}");
                        return new ServiceMessage { IsSuccess = false, Message = $"PDF oluşturulamadı: {pdfEx.Message}" };
                    }
                }

                // E-posta adresini belirle
                var email = recipientEmail ?? invoice.Guest?.Email;
                if (string.IsNullOrWhiteSpace(email))
                    return new ServiceMessage { IsSuccess = false, Message = "E-posta adresi bulunamadı." };

                // PDF dosya yolunu al
                var pdfPath = invoice.PdfUrl;
                if (string.IsNullOrEmpty(pdfPath))
                    return new ServiceMessage { IsSuccess = false, Message = "PDF dosyası bulunamadı." };

                // PDF dosyasının fiziksel yolunu al (IPdfUrlService kullanarak)
                var pdfPhysicalPath = _pdfUrlService.GetFullFilePathFromUrl(pdfPath);
                if (string.IsNullOrEmpty(pdfPhysicalPath) || !System.IO.File.Exists(pdfPhysicalPath))
                {
                    // Fallback: Eski yöntem (geriye dönük uyumluluk için)
                    var fileName = _pdfUrlService.GetFileNameFromUrl(pdfPath);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = Path.GetFileName(pdfPath);
                    }
                    
                    // OutputPath kullan (e-posta için farklı bir path olabilir)
                    pdfPhysicalPath = Path.Combine(_configuration["PdfSettings:OutputPath"] ?? "wwwroot/pdfs", fileName);
                }

                // E-posta gönder
                try
                {
                    var guestName = invoice.Guest?.FullName ?? "Değerli Müşteri";
                    var subject = $"Fatura #{invoice.InvoiceNumber} - GuestFlow";
                    var body = $@"
Merhaba {guestName},

Faturanız ektedir.

Fatura Detayları:
- Fatura No: {invoice.InvoiceNumber}
- Tarih: {invoice.IssueDate:dd.MM.yyyy}
- Tutar: {invoice.TotalAmount:N2} {invoice.Currency}

İyi günler dileriz.
GuestFlow Ekibi";

                    var attachments = new List<string>();
                    if (System.IO.File.Exists(pdfPhysicalPath))
                    {
                        attachments.Add(pdfPhysicalPath);
                    }

                    var emailSent = await _emailService.SendEmailAsync(
                        email,
                        subject,
                        body,
                        false, // HTML değil, plain text
                        attachments
                    );

                    if (emailSent)
                    {
                        _logger.LogInformation($"Fatura e-postası gönderildi: {invoiceId} -> {email}");
                        return new ServiceMessage { IsSuccess = true, Message = "Fatura başarıyla e-posta ile gönderildi." };
                    }
                    else
                    {
                        return new ServiceMessage { IsSuccess = false, Message = "E-posta gönderilemedi." };
                    }
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, $"Fatura e-postası gönderilirken hata: {emailEx.Message}");
                    return new ServiceMessage { IsSuccess = false, Message = $"E-posta gönderilemedi: {emailEx.Message}" };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fatura e-postası gönderilirken hata: {ex.Message}. InvoiceId: {invoiceId}");
                return new ServiceMessage { IsSuccess = false, Message = $"E-posta gönderilemedi: {ex.Message}" };
            }
        }
    }
}