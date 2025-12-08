using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Transfer
{
    public class TransferManager : ITransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly IRepository<AirportEntity> _airportRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransferManager> _logger;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IMapper _mapper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateValidationService _dateValidationService;
        private readonly IInvoiceCreationService _invoiceCreationService;

        public TransferManager(
            IUnitOfWork unitOfWork,
            IRepository<TransferEntity> transferRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<AirportEntity> airportRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            DailyRevenueJob dailyRevenueJob,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<TransferManager> logger,
            IForeignKeyValidationService foreignKeyValidationService,
            ICurrencyService currencyService,
            IPdfUrlService pdfUrlService,
            IMapper mapper,
            IPriceCalculationService priceCalculationService,
            IDateValidationService dateValidationService,
            IInvoiceCreationService invoiceCreationService)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _guestRepository = guestRepository;
            _vehicleRepository = vehicleRepository;
            _airportRepository = airportRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _dailyRevenueJob = dailyRevenueJob;
            _pdfService = pdfService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
            _foreignKeyValidationService = foreignKeyValidationService;
            _currencyService = currencyService;
            _pdfUrlService = pdfUrlService;
            _mapper = mapper;
            _priceCalculationService = priceCalculationService;
            _dateValidationService = dateValidationService;
            _invoiceCreationService = invoiceCreationService;
        }

        public async Task<ServiceMessage> AddTransfer(AddTransferDto transfer)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    VehicleId = transfer.VehicleId,
                    AirportId = transfer.AirportId,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // İş Kuralı Validasyonları
                // 1. Transfer tarihi geçmişte olamaz
                var dateValidation = _dateValidationService.ValidateNotPastDate(transfer.TransferDate, "Transfer tarihi");
                if (!dateValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = dateValidation.ErrorMessage };
                }

                // 2. Araç müsaitlik kontrolü - Aynı araç aynı tarihte başka transferde kullanılıyor mu?
                var isVehicleAvailable = await _transferRepository.GetAll()
                    .Where(t => t.VehicleId == transfer.VehicleId &&
                               t.TransferDate.Date == transfer.TransferDate.Date &&
                               t.Status != "Cancelled")
                    .AnyAsync();

                if (isVehicleAvailable)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Seçilen araç bu tarihte başka bir transfer için rezerve edilmiş." };
                }

                // Fiyat hesaplama
                decimal finalPrice = _priceCalculationService.CalculateFinalPrice(transfer.Price, transfer.DiscountPercentage);

                // Para birimi belirleme
                var currency = _priceCalculationService.ValidateAndGetCurrency(transfer.Currency);

                // Transfer oluşturma
                var transferEntity = new TransferEntity
                {
                    TransferDate = transfer.TransferDate,
                    PickupAddress = transfer.PickupAddress,
                    DropoffAddress = transfer.DropoffAddress,
                    Price = transfer.Price,
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    AirportId = transfer.AirportId,
                    VehicleId = transfer.VehicleId,
                    Note = transfer.Note,
                    Status = transfer.Status,
                    IsFromAirport = transfer.IsFromAirport,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId,
                    DiscountPercentage = transfer.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency
                };

                await _transferRepository.AddAsync(transferEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(transfer.GuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // Fatura oluşturma
                if (transfer.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (transfer.PersonnelId > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(transfer.PersonnelId);
                    }

                    // Para birimi transferEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = transferEntity.Currency;

                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = transfer.InvoiceDescription ?? "Transfer faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = transfer.GuestId,
                        PersonnelId = transfer.PersonnelId > 0 ? transfer.PersonnelId : null,
                        TransferId = transferEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();

                    // PDF oluştur ve URL'i güncelle
                    try
                    {
                        var pdfUrl = await _pdfService.GenerateInvoicePdfAsync(invoice, guest, personnel);
                        invoice.PdfUrl = pdfUrl;
                        await _invoiceRepository.UpdateAsync(invoice);
                        await _unitOfWork.SaveChangesAsync();

                        // Fatura e-postası gönder
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
                            }
                            catch (Exception emailEx)
                            {
                                _logger.LogError(emailEx, $"Transfer fatura e-postası gönderilirken hata: {emailEx.Message}");
                            }
                        }
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, $"Transfer fatura PDF'i oluşturulurken hata: {pdfEx.Message}");
                        // PDF oluşturma hatası fatura oluşturmayı engellemez, sadece loglanır
                    }
                }

                // Rezervasyon onay e-postası gönder
                if (!string.IsNullOrEmpty(guest.Email) && !guest.IsSpecialGuest)
                {
                    try
                    {
                        var vehicle = await _vehicleRepository.GetByIdAsync(transfer.VehicleId);
                        var vehicleInfo = vehicle != null ? $"{vehicle.Type} - {vehicle.PlateNumber}" : "Bilinmiyor";
                        var details = $"Transfer Tarihi: {transfer.TransferDate:dd.MM.yyyy HH:mm}\n" +
                                     $"Alış Adresi: {transfer.PickupAddress}\n" +
                                     $"Bırakış Adresi: {transfer.DropoffAddress}\n" +
                                     $"Araç: {vehicleInfo}\n" +
                                     $"Tutar: {transferEntity.FinalPrice:N2} TRY";

                        await _emailService.SendBookingConfirmationAsync(
                            guest.Email,
                            guest.FullName,
                            "Transfer",
                            transfer.TransferDate,
                            details
                        );
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Transfer rezervasyon onay e-postası gönderilirken hata: {emailEx.Message}");
                    }
                }

                // Günlük gelir hesaplama
                await _dailyRevenueJob.CalculateDailyRevenue(transfer.TransferDate.Date);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer eklendi: {transferEntity.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> UpdateTransfer(UpdateTransferDto transfer)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyonlar
                var existing = await _transferRepository.GetAsync(x => x.Id == transfer.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    VehicleId = transfer.VehicleId,
                    AirportId = transfer.AirportId,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }
                    return new ServiceMessage { IsSuccess = false, Message = "Bırakış şehri bulunamadı." };

                if (!await _personnelRepository.GetAll(x => x.Id == transfer.PersonnelId).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // Güncelleme
                existing.TransferDate = transfer.TransferDate;
                existing.PickupAddress = transfer.PickupAddress;
                existing.DropoffAddress = transfer.DropoffAddress;
                existing.Price = transfer.Price;
                existing.GuestId = transfer.GuestId;
                existing.PersonnelId = transfer.PersonnelId;
                existing.AirportId = transfer.AirportId;
                existing.VehicleId = transfer.VehicleId;
                existing.Note = transfer.Note;
                existing.Status = transfer.Status;
                existing.IsFromAirport = transfer.IsFromAirport;
                existing.PickupCityId = transfer.PickupCityId;
                existing.DropoffCityId = transfer.DropoffCityId;

                await _transferRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer güncellendi: {transfer.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> DeleteTransfer(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _transferRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<GetTransferDto> GetTransferById(int id)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null)
                    throw new Exception("Transfer bulunamadı.");

                return _mapper.Map<GetTransferDto>(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<List<GetTransferDto>> GetTransfers()
        {
            try
            {
                var transfers = await _transferRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetTransferDto>>(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transferler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<PagedResult<GetTransferDto>> GetTransfersPaged(int pageNumber, int pageSize)
        {
            return await GetTransfersPaged(pageNumber, pageSize, null);
        }

        public async Task<PagedResult<GetTransferDto>> GetTransfersPaged(int pageNumber, int pageSize, TransferFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _transferRepository.GetAll()
                    .ApplyTransferFilters(filters)
                    .ApplyTransferSorting(sorting);

                var totalCount = await query.CountAsync();
                var transfers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetTransferDto>>(transfers);
                return new PagedResult<GetTransferDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış transferler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<TransferDetailDto> GetTransferDetailAsync(int id)
        {
            try
            {
                var transfer = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Include(t => t.Airport)
                    .Include(t => t.PickupCity)
                    .Include(t => t.DropoffCity)
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (transfer == null)
                    throw new Exception("Transfer bulunamadı.");

                var detail = _mapper.Map<TransferDetailDto>(transfer);
                
                // AutoMapper ile map edilemeyen özel alanları manuel olarak set et
                if (transfer.Personnel != null && detail.Personnel != null)
                {
                    detail.Personnel.PhoneNumber = null; // PersonnelEntity'de PhoneNumber yok
                }
                
                if (transfer.Vehicle != null && detail.Vehicle != null)
                {
                    detail.Vehicle.VehicleName = transfer.Vehicle.Type; // VehicleEntity'de Name yok, Type kullanıyoruz
                }
                
                if (transfer.Airport != null)
                {
                    detail.Airport = new TransferAirportDto
                    {
                        Id = transfer.Airport.Id,
                        AirportName = transfer.Airport.Name, // AirportEntity'de AirportName yok, Name kullanıyoruz
                        CityName = transfer.Airport.City?.CityName,
                        Country = transfer.Airport.City?.Country
                    };
                }
                
                if (transfer.PickupCity != null)
                {
                    detail.PickupCity = new TransferCityDto
                    {
                        Id = transfer.PickupCity.Id,
                        CityName = transfer.PickupCity.CityName,
                        Country = transfer.PickupCity.Country
                    };
                }
                
                if (transfer.DropoffCity != null)
                {
                    detail.DropoffCity = new TransferCityDto
                    {
                        Id = transfer.DropoffCity.Id,
                        CityName = transfer.DropoffCity.CityName,
                        Country = transfer.DropoffCity.Country
                    };
                }

                return detail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<TransferCalendarDto> GetTransferCalendarAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today;
                var end = endDate?.Date ?? today.AddDays(30);
                var weekEnd = today.AddDays(7);
                var monthEnd = today.AddMonths(1);

                // Bugünkü transferler
                var todayTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Where(t => t.TransferDate.Date == today && !t.IsDeleted)
                    .Select(t => new TransferCalendarItemDto
                    {
                        Id = t.Id,
                        TransferDate = t.TransferDate,
                        PickupAddress = t.PickupAddress,
                        DropoffAddress = t.DropoffAddress,
                        Status = t.Status.ToString(),
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : null,
                        VehicleName = t.Vehicle != null ? t.Vehicle.Type : null,
                        FinalPrice = t.FinalPrice,
                        IsFromAirport = t.IsFromAirport
                    })
                    .OrderBy(t => t.TransferDate)
                    .ToListAsync();

                // Bu haftanın transferleri
                var weekTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Where(t => t.TransferDate.Date > today && t.TransferDate.Date <= weekEnd && !t.IsDeleted)
                    .Select(t => new TransferCalendarItemDto
                    {
                        Id = t.Id,
                        TransferDate = t.TransferDate,
                        PickupAddress = t.PickupAddress,
                        DropoffAddress = t.DropoffAddress,
                        Status = t.Status.ToString(),
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : null,
                        VehicleName = t.Vehicle != null ? t.Vehicle.Type : null,
                        FinalPrice = t.FinalPrice,
                        IsFromAirport = t.IsFromAirport
                    })
                    .OrderBy(t => t.TransferDate)
                    .ToListAsync();

                // Bu ayın transferleri
                var monthTransfers = await _transferRepository.GetAll()
                    .Include(t => t.Guest)
                    .Include(t => t.Personnel)
                    .Include(t => t.Vehicle)
                    .Where(t => t.TransferDate.Date > weekEnd && t.TransferDate.Date <= monthEnd && !t.IsDeleted)
                    .Select(t => new TransferCalendarItemDto
                    {
                        Id = t.Id,
                        TransferDate = t.TransferDate,
                        PickupAddress = t.PickupAddress,
                        DropoffAddress = t.DropoffAddress,
                        Status = t.Status.ToString(),
                        GuestName = t.Guest != null ? t.Guest.FullName : "Bilinmiyor",
                        PersonnelName = t.Personnel != null ? t.Personnel.FullName : null,
                        VehicleName = t.Vehicle != null ? t.Vehicle.Type : null,
                        FinalPrice = t.FinalPrice,
                        IsFromAirport = t.IsFromAirport
                    })
                    .OrderBy(t => t.TransferDate)
                    .ToListAsync();

                var totalUpcoming = todayTransfers.Count + weekTransfers.Count + monthTransfers.Count;

                return new TransferCalendarDto
                {
                    Today = todayTransfers,
                    ThisWeek = weekTransfers,
                    ThisMonth = monthTransfers,
                    TotalUpcoming = totalUpcoming
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer takvimi getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<TransferStatisticsDto> GetTransferStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today.AddMonths(-1);
                var end = endDate?.Date ?? today;

                var transfers = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date >= start && t.TransferDate.Date <= end && !t.IsDeleted)
                    .ToListAsync();

                var statistics = new TransferStatisticsDto
                {
                    TotalTransfers = transfers.Count,
                    CompletedTransfers = transfers.Count(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                    PendingTransfers = transfers.Count(t => t.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
                    InProgressTransfers = transfers.Count(t => t.Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase)),
                    TotalRevenue = transfers.Sum(t => t.FinalPrice),
                    AveragePrice = transfers.Count > 0 ? transfers.Average(t => t.FinalPrice) : 0
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Transfer istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<ServiceMessage> UpdateTransferStatusAsync(int id, string status)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null || transfer.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                // Durum geçerliliğini kontrol et
                var validStatuses = new[] { "Pending", "InProgress", "Completed", "Cancelled" };
                if (!validStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
                    return new ServiceMessage { IsSuccess = false, Message = "Geçersiz durum." };

                // TransferEntity'de Status string olarak tutuluyor
                transfer.Status = status;
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer durumu güncellendi: {id} -> {status}");
                return new ServiceMessage { IsSuccess = true, Message = "Transfer durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer durumu güncellenirken hata: {ex.Message}. Id: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Transfer durumu güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> AssignVehicleAsync(int id, int vehicleId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null || transfer.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Araç bulunamadı." };

                transfer.VehicleId = vehicleId;
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer'e araç atandı: {id} -> {vehicleId}");
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla atandı." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer'e araç atanırken hata: {ex.Message}. Id: {id}, VehicleId: {vehicleId}");
                return new ServiceMessage { IsSuccess = false, Message = $"Araç atanırken hata: {ex.Message}" };
            }
        }

        private async Task<int> GenerateInvoiceNumber()
        {
            // Fatura numarası benzersizlik kontrolü ile oluştur
            int maxAttempts = 10;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var lastInvoice = await _invoiceRepository.GetAll()
                    .OrderByDescending(x => x.InvoiceNumber)
                    .FirstOrDefaultAsync();
                
                int newInvoiceNumber = lastInvoice != null ? lastInvoice.InvoiceNumber + 1 : 1000;
                
                // Benzersizlik kontrolü
                var exists = await _invoiceRepository.GetAll(x => x.InvoiceNumber == newInvoiceNumber).AnyAsync();
                if (!exists)
                {
                    return newInvoiceNumber;
                }
                
                // Eğer numara mevcutsa, bir sonraki numarayı dene
                newInvoiceNumber++;
            }
            
            // Tüm denemeler başarısız olursa, timestamp bazlı bir numara oluştur
            _logger.LogWarning("Fatura numarası oluşturulurken benzersizlik kontrolü başarısız oldu, timestamp bazlı numara kullanılıyor.");
            return int.Parse(DateTime.UtcNow.ToString("yyyyMMddHHmmss")) % 10000000; // Son 7 haneyi al
        }
    }
}