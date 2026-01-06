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
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Cache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.Entities.Enum;
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
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TransferManager> _logger;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly IBusinessRuleValidator _businessRuleValidator;
        private readonly ICacheService _cacheService;
        private readonly ICurrencyService _currencyService;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IMapper _mapper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateValidationService _dateValidationService;
        private readonly IInvoiceCreationService _invoiceCreationService;
        private readonly INotificationHubService _hubService;
        private readonly IPaymentStatusService _paymentStatusService;

        public TransferManager(
            IUnitOfWork unitOfWork,
            IRepository<TransferEntity> transferRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<VehicleEntity> vehicleRepository,
            IRepository<AirportEntity> airportRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            IRepository<PaymentEntity> paymentRepository,
            DailyRevenueJob dailyRevenueJob,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<TransferManager> logger,
            IForeignKeyValidationService foreignKeyValidationService,
            IBusinessRuleValidator businessRuleValidator,
            ICacheService cacheService,
            ICurrencyService currencyService,
            IPdfUrlService pdfUrlService,
            IMapper mapper,
            IPriceCalculationService priceCalculationService,
            IDateValidationService dateValidationService,
            IInvoiceCreationService invoiceCreationService,
            IPaymentStatusService paymentStatusService,
            INotificationHubService? hubService = null)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _guestRepository = guestRepository;
            _vehicleRepository = vehicleRepository;
            _airportRepository = airportRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _paymentRepository = paymentRepository;
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
            _paymentStatusService = paymentStatusService;
            _hubService = hubService;
            _businessRuleValidator = businessRuleValidator;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Clear transfer-related cache
        /// Transfer ile ilgili cache'i temizle
        /// </summary>
        private async Task ClearTransferCacheAsync()
        {
            try
            {
                // For now, we'll clear all transfer cache by removing keys with "transfers_" prefix
                // In a production system, you might want to use Redis SCAN or maintain a registry
                _logger.LogDebug("Transfer cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error clearing transfer cache");
            }
        }

        public async Task<ServiceMessage<AddTransferResponseDto>> AddTransfer(AddTransferDto transfer)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign Key Validasyonları (nullable alanlar için kontrol)
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    DriverId = transfer.DriverId,
                    VehicleId = transfer.VehicleId,
                    AirportId = transfer.AirportId,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage<AddTransferResponseDto> { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // BUSINESS VALIDATION RULES
                var businessValidation = await ValidateTransferBusinessRulesAsync(transfer);
                if (!businessValidation.IsValid)
                {
                    return new ServiceMessage<AddTransferResponseDto> { IsSuccess = false, Message = businessValidation.ErrorMessage };
                }

                // DATE REALITY: Past-dated entries ARE allowed
                // Service date represents when the operation actually occurred, not when entered
                // Transfer tarihi validasyonu kaldırıldı - geçmiş tarihli girişler operasyonel olarak normaldir

                // Araç müsaitlik kontrolü - Sadece VehicleId varsa kontrol et
                if (transfer.VehicleId.HasValue)
                {
                    var isVehicleAvailable = await _transferRepository.GetAll()
                        .Where(t => t.VehicleId == transfer.VehicleId &&
                                   t.TransferDate.Date == transfer.TransferDate.Date &&
                                   t.Status != "Cancelled")
                        .AnyAsync();

                    if (isVehicleAvailable)
                    {
                        return new ServiceMessage<AddTransferResponseDto> { IsSuccess = false, Message = "Seçilen araç bu tarihte başka bir transfer için rezerve edilmiş." };
                    }
                }

                // Fiyat hesaplama
                decimal finalPrice = _priceCalculationService.CalculateFinalPrice(transfer.Price, transfer.DiscountPercentage);

                // Para birimi belirleme
                var currency = _priceCalculationService.ValidateAndGetCurrency(transfer.Currency);

                // Transfer oluşturma
                var transferEntity = new TransferEntity
                {
                    TransferDate = transfer.TransferDate,
                    PickupTime = transfer.PickupTime,
                    ServiceStartTime = transfer.ServiceStartTime,
                    PickupAddress = transfer.PickupAddress,
                    DropoffAddress = transfer.DropoffAddress,
                    Price = transfer.Price,
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    DriverId = transfer.DriverId,
                    AirportId = transfer.AirportId,
                    VehicleId = transfer.VehicleId,
                    Note = transfer.Note,
                    Status = transfer.Status ?? "Pending",
                    TransferType = transfer.TransferType,
                    PickupCityId = transfer.PickupCityId,
                    DropoffCityId = transfer.DropoffCityId,
                    DiscountPercentage = transfer.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency,
                    DriverName = transfer.DriverName,
                    ExternalVehiclePlate = transfer.ExternalVehiclePlate,
                    ExternalDriverName = transfer.ExternalDriverName,
                    ExternalDriverPhone = transfer.ExternalDriverPhone,
                    SupplierName = transfer.SupplierName,
                    SupplierCost = transfer.SupplierCost,
                    SupplierCurrency = transfer.SupplierCurrency,
                    SupplierInvoiceNumber = transfer.SupplierInvoiceNumber,
                    SupplierContactPhone = transfer.SupplierContactPhone,
                    SupplierEmergencyContact = transfer.SupplierEmergencyContact,
                    AccessibilityRequirements = transfer.AccessibilityRequirements,
                    SpecialHandlingNotes = transfer.SpecialHandlingNotes,
                    ConciergeInternalNotes = transfer.ConciergeInternalNotes,
                    GuestVisibleNotes = transfer.GuestVisibleNotes,
                    ContactPersonName = transfer.ContactPersonName,
                    MeetingPointDetails = transfer.MeetingPointDetails,
                    GroupSize = transfer.GroupSize,
                    ChildCount = transfer.ChildCount,
                    InfantCount = transfer.InfantCount,
                    GuestLanguage = transfer.GuestLanguage,
                    EmergencyContactPhone = transfer.EmergencyContactPhone,
                    PrimaryContactPhone = transfer.PrimaryContactPhone,
                    SecondaryContactPhone = transfer.SecondaryContactPhone,
                    PickupConfirmationTime = transfer.PickupConfirmationTime,
                    DropoffConfirmationTime = transfer.DropoffConfirmationTime
                };

                await _transferRepository.AddAsync(transferEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(transfer.GuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                InvoicesEntity? invoice = null;
                string? pdfUrl = null;

                // Fatura oluşturma
                if (transfer.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (transfer.PersonnelId.HasValue && transfer.PersonnelId.Value > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(transfer.PersonnelId.Value);
                    }

                    // Para birimi transferEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = transferEntity.Currency;

                    invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = transfer.InvoiceDescription ?? "Transfer faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = transfer.GuestId,
                        PersonnelId = transfer.PersonnelId,
                        // TransferId removed - invoices are now multi-service
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();

                    // PDF oluştur ve URL'i güncelle
                    try
                    {
                        pdfUrl = await _pdfService.GenerateInvoicePdfAsync(invoice, guest, personnel);
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
                        var vehicleInfo = "Bilinmiyor";
                        if (transfer.VehicleId.HasValue)
                        {
                            var vehicle = await _vehicleRepository.GetByIdAsync(transfer.VehicleId.Value);
                            vehicleInfo = vehicle != null ? $"{vehicle.Type} - {vehicle.PlateNumber}" : "Bilinmiyor";
                        }
                        else if (!string.IsNullOrEmpty(transfer.ExternalVehiclePlate))
                        {
                            vehicleInfo = $"Dışarıdan - {transfer.ExternalVehiclePlate}";
                        }
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

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Transfer", transferEntity.Id, "created");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                // Clear cache after successful creation
                await ClearTransferCacheAsync();

                return new ServiceMessage<AddTransferResponseDto>
                {
                    IsSuccess = true,
                    Message = "Transfer başarıyla eklendi.",
                    Data = new AddTransferResponseDto
                    {
                        TransferId = transferEntity.Id,
                        InvoiceId = invoice?.Id,
                        InvoicePdfUrl = pdfUrl
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Transfer eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Transfer eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage<AddTransferResponseDto> { IsSuccess = false, Message = errorMessage };
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
                existing.PickupCityId = transfer.PickupCityId;
                existing.DropoffCityId = transfer.DropoffCityId;
                existing.SupplierName = transfer.SupplierName;
                existing.SupplierCost = transfer.SupplierCost;
                existing.SupplierCurrency = transfer.SupplierCurrency;
                existing.SupplierInvoiceNumber = transfer.SupplierInvoiceNumber;

                // AUDIT TRACEABILITY: Mark as updated with personnel trace
                existing.MarkAsUpdated(transfer.PersonnelId);

                await _transferRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer güncellendi: {transfer.Id}");

                // Send live update for real-time UI updates
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("Transfer", existing.Id, "updated");
                }

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

                var transferDto = _mapper.Map<GetTransferDto>(transfer);

                // Calculate payment status using PaymentStatusService
                var paymentStatus = await _paymentStatusService.GetServicePaymentStatusAsync(id, "Transfer");
                if (paymentStatus != null)
                {
                    transferDto.PaymentStatus = paymentStatus.PaymentStatus;
                    transferDto.PaidAmount = paymentStatus.PaidAmount;
                    transferDto.RemainingAmount = paymentStatus.RemainingAmount;
                    transferDto.PaidAmountByCurrency = paymentStatus.PaidAmountByCurrency;
                    transferDto.RemainingAmountByCurrency = paymentStatus.RemainingAmountByCurrency;
                }

                return transferDto;
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
                // Create cache key based on parameters
                var cacheKey = $"transfers_paged_{pageNumber}_{pageSize}_{filters?.GetHashCode() ?? 0}_{sorting?.GetHashCode() ?? 0}";

                // Try to get from cache first (5 minutes cache for list data)
                var cachedResult = await _cacheService.GetAsync<PagedResult<GetTransferDto>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Returning cached transfer list for key: {CacheKey}", cacheKey);
                    return cachedResult;
                }

                var query = _transferRepository.GetAll()
                    .ApplyTransferFilters(filters)
                    .ApplyTransferSorting(sorting);

                var totalCount = await query.CountAsync();
                var transfers = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetTransferDto>>(transfers);
                var result = new PagedResult<GetTransferDto>(dtos, totalCount, pageNumber, pageSize);

                // Cache the result for 5 minutes
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

                _logger.LogDebug("Cached transfer list for key: {CacheKey}", cacheKey);
                return result;
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

                // REVENUE REALITY: Revenue = collected money only (from PaymentEntity)
                var transferIds = transfers.Select(t => t.Id).ToList();
                var totalRevenue = await _paymentRepository.GetAll()
                    .Where(p => p.TransferId.HasValue && transferIds.Contains(p.TransferId.Value) && p.Status == PaymentStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

                var statistics = new TransferStatisticsDto
                {
                    TotalTransfers = transfers.Count,
                    CompletedTransfers = transfers.Count(t => t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
                    PendingTransfers = transfers.Count(t => t.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase)),
                    InProgressTransfers = transfers.Count(t => t.Status.Equals("InProgress", StringComparison.OrdinalIgnoreCase)),
                    TotalRevenue = totalRevenue,
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

        /// <summary>
        /// Transfer için business validation kuralları
        /// </summary>
        private async Task<ValidationResult> ValidateTransferBusinessRulesAsync(AddTransferDto transfer)
        {
            // Eski validasyon kuralları - hala gerekli olanlar
            // 1. Group size validation
            if (transfer.GroupSize.HasValue && transfer.GroupSize.Value <= 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Grup boyutu 1 veya daha büyük olmalıdır." } };
            }

            // 2. Child count validation
            if (transfer.ChildCount.HasValue && transfer.ChildCount.Value < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Çocuk sayısı 0 veya daha büyük olmalıdır." } };
            }

            // 3. Infant count validation
            if (transfer.InfantCount.HasValue && transfer.InfantCount.Value < 0)
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Bebek sayısı 0 veya daha büyük olmalıdır." } };
            }

            // 4. Group composition validation
            var totalPeople = 1 + (transfer.ChildCount ?? 0) + (transfer.InfantCount ?? 0); // 1 for main guest
            if (transfer.GroupSize.HasValue && totalPeople != transfer.GroupSize.Value)
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { $"Grup boyutu ({transfer.GroupSize}) ile kişi sayısı ({totalPeople}) eşleşmiyor." } };
            }

            // 5. Emergency contact validation for groups
            if ((transfer.GroupSize ?? 1) > 1 && string.IsNullOrWhiteSpace(transfer.EmergencyContactPhone))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Grup transferlerinde acil iletişim telefonu zorunludur." } };
            }

            // 5b. Primary contact phone validation
            if (string.IsNullOrWhiteSpace(transfer.PrimaryContactPhone))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Ana iletişim telefonu zorunludur." } };
            }

            // 5c. Secondary contact phone validation for groups
            if ((transfer.GroupSize ?? 1) > 3 && string.IsNullOrWhiteSpace(transfer.SecondaryContactPhone))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "4+ kişilik gruplarda yedek iletişim telefonu zorunludur." } };
            }

            // 6. Contact person validation for airport transfers
            if (transfer.AirportId.HasValue && string.IsNullOrWhiteSpace(transfer.ContactPersonName))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Havaalanı transferlerinde iletişim kişisi adı zorunludur." } };
            }

            // 7. Meeting point validation
            if (transfer.AirportId.HasValue && string.IsNullOrWhiteSpace(transfer.MeetingPointDetails))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Havaalanı transferlerinde buluşma noktası detayları zorunludur." } };
            }

            // 8. Time validation
            if (transfer.PickupTime.HasValue && transfer.ServiceStartTime.HasValue &&
                transfer.PickupTime.Value >= transfer.ServiceStartTime.Value)
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Alış saati, hizmet başlangıç saatinden önce olmalıdır." } };
            }

            // 9. Supplier contact validation
            if (!string.IsNullOrWhiteSpace(transfer.SupplierName) && string.IsNullOrWhiteSpace(transfer.SupplierContactPhone))
            {
                return new ValidationResult { IsValid = false, ErrorMessages = { "Tedarikçi adı girildiğinde iletişim telefonu zorunludur." } };
            }

            // Şimdi BusinessRuleValidator kullanarak gelişmiş validasyonları çalıştır
            // Geçici TransferEntity oluştur
            var tempTransfer = new TransferEntity
            {
                TransferDate = transfer.TransferDate,
                PickupAddress = transfer.PickupAddress,
                DropoffAddress = transfer.DropoffAddress,
                Price = transfer.Price,
                FinalPrice = transfer.Price, // AddTransferDto'da FinalPrice yok, Price kullan
                GuestId = transfer.GuestId,
                DriverId = transfer.DriverId,
                AirportId = transfer.AirportId,
                VehicleId = transfer.VehicleId,
                Status = transfer.Status ?? "Pending",
                GroupSize = transfer.GroupSize,
                IsVip = transfer.IsVip,
                ServiceStartTime = transfer.ServiceStartTime,
                SpecialHandlingNotes = transfer.SpecialHandlingNotes,
                TransportMode = transfer.TransportMode ?? TransportMode.Sedan
            };

            var businessValidation = await _businessRuleValidator.ValidateTransferAsync(tempTransfer, transfer);
            if (!businessValidation.IsValid)
            {
                return businessValidation;
            }

            // 10. Vehicle capacity vs group size validation (eski yöntemle uyumluluk için)
            if (transfer.VehicleId.HasValue && transfer.GroupSize.HasValue)
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(transfer.VehicleId.Value);
                if (vehicle != null && transfer.GroupSize.Value > vehicle.Capacity)
                {
                    return new ValidationResult { IsValid = false, ErrorMessages = { $"Grup boyutu ({transfer.GroupSize}) seçilen aracın kapasitesini ({vehicle.Capacity}) aşıyor." } };
                }
            }

            // 11. Transport mode validation based on group size (eski yöntemle uyumluluk için)
            if (transfer.TransportMode.HasValue && transfer.GroupSize.HasValue)
            {
                var groupSize = transfer.GroupSize.Value;
                var transportMode = transfer.TransportMode.Value;

                // Sedan can only handle small groups
                if (transportMode == TransportMode.Sedan && groupSize > 4)
                {
                    return new ValidationResult { IsValid = false, ErrorMessages = { "Sedan araç tipi en fazla 4 kişilik gruplar için uygundur." } };
                }

                // Minibus required for larger groups
                if (transportMode == TransportMode.Van && groupSize > 8)
                {
                    return new ValidationResult { IsValid = false, ErrorMessages = { "8+ kişilik gruplar için minibüs gerekli." } };
                }
            }

            return new ValidationResult { IsValid = true, ErrorMessages = new List<string>() };
        }

        public async Task<ServiceMessage> CreateTransferInvoiceAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null || transfer.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                // Check if invoice already exists for this transfer
                var existingInvoice = await _invoiceRepository.GetAll()
                    .FirstOrDefaultAsync(i => i.InvoiceItems.Any(item => item.ServiceType == "Transfer" && item.ServiceId == id) && !i.IsDeleted);
                if (existingInvoice != null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu transfer için zaten fatura oluşturulmuş." };

                // Create invoice
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = await GenerateInvoiceNumber(),
                    IssueDate = DateTime.Now,
                    TotalAmount = transfer.FinalPrice,
                    Currency = transfer.Currency ?? "TRY",
                    Notes = $"Transfer #{id} - {transfer.PickupAddress} → {transfer.DropoffAddress}",
                    GuestId = transfer.GuestId,
                    PersonnelId = transfer.PersonnelId,
                    Status = InvoiceStatus.Draft,
                    IsPdfGenerated = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync(); // Save to get invoice.Id

                // Add invoice item for this transfer
                var invoiceItem = new InvoiceItemEntity
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "Transfer",
                    ServiceId = id,
                    Amount = transfer.FinalPrice,
                    Currency = transfer.Currency ?? "TRY",
                    Notes = $"Transfer: {transfer.PickupAddress} → {transfer.DropoffAddress}"
                };

                await _invoiceItemRepository.AddAsync(invoiceItem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Transfer faturası oluşturuldu: {id} -> Invoice #{invoice.Id}");
                return new ServiceMessage { IsSuccess = true, Message = $"Fatura başarıyla oluşturuldu. Fatura No: {invoice.InvoiceNumber}" };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura oluşturulurken hata: {ex.Message}. TransferId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> SendTransferConfirmationAsync(int id)
        {
            try
            {
                var transfer = await _transferRepository.GetByIdAsync(id);
                if (transfer == null || transfer.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                var guest = await _guestRepository.GetByIdAsync(transfer.GuestId);
                if (guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bilgileri bulunamadı." };

                // Send confirmation email
                var subject = $"Transfer Onayı - #{id}";
                var body = $@"
Merhaba {guest.FullName},

Transferiniz onaylanmıştır.

Transfer Detayları:
- Tarih: {transfer.TransferDate:dd.MM.yyyy HH:mm}
- Rota: {transfer.PickupAddress} → {transfer.DropoffAddress}
- Fiyat: {transfer.FinalPrice} {transfer.Currency ?? "TRY"}

Saygılarımla,
Hotel Concierge Team
";

                await _emailService.SendEmailAsync(guest.Email, subject, body);

                _logger.LogInformation($"Transfer onay maili gönderildi: {id} -> {guest.Email}");
                return new ServiceMessage { IsSuccess = true, Message = "Onay maili başarıyla gönderildi." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Onay maili gönderilirken hata: {ex.Message}. TransferId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Onay maili gönderilirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> CreateRoundTripTransferAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var originalTransfer = await _transferRepository.GetByIdAsync(id);
                if (originalTransfer == null || originalTransfer.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Transfer bulunamadı." };

                // Check if round trip already exists
                var existingRoundTrip = await _transferRepository.GetAll()
                    .FirstOrDefaultAsync(t => t.ReturnTransferId == id && !t.IsDeleted);
                if (existingRoundTrip != null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu transfer için zaten gidiş-dönüş oluşturulmuş." };

                // Create round trip transfer (swap pickup and dropoff, add some time)
                var roundTripTransfer = new TransferEntity
                {
                    TransferDate = originalTransfer.TransferDate.AddHours(4), // Assume 4 hours later
                    PickupAddress = originalTransfer.DropoffAddress,
                    DropoffAddress = originalTransfer.PickupAddress,
                    Price = originalTransfer.Price,
                    FinalPrice = originalTransfer.FinalPrice,
                    GuestId = originalTransfer.GuestId,
                    PersonnelId = originalTransfer.PersonnelId,
                    DriverId = originalTransfer.DriverId,
                    AirportId = originalTransfer.AirportId,
                    VehicleId = originalTransfer.VehicleId,
                    Status = "Pending",
                    TransferType = originalTransfer.TransferType,
                    PickupCityId = originalTransfer.DropoffCityId,
                    DropoffCityId = originalTransfer.PickupCityId,
                    Currency = originalTransfer.Currency,
                    Note = $"Gidiş-dönüş - Transfer #{id}",
                    ReturnTransferId = id, // Link back to original
                    CreatedDate = DateTime.Now,
                    IsDeleted = false
                };

                await _transferRepository.AddAsync(roundTripTransfer);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Gidiş-dönüş transfer oluşturuldu: {id} -> {roundTripTransfer.Id}");
                return new ServiceMessage { IsSuccess = true, Message = $"Gidiş-dönüş transfer başarıyla oluşturuldu. Transfer ID: {roundTripTransfer.Id}" };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Gidiş-dönüş transfer oluşturulurken hata: {ex.Message}. OriginalTransferId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Gidiş-dönüş transfer oluşturulurken hata: {ex.Message}" };
            }
        }
    }
}