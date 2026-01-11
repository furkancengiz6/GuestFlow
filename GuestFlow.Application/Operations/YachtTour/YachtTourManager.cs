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
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Operations.Validation;
using GuestFlow.Application.Operations.Currency;
using GuestFlow.Application.Operations.Common;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace GuestFlow.Application.Operations.YachtTour
{
    public class YachtTourManager : IYachtTourService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<YachtTourManager> _logger;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IMapper _mapper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateValidationService _dateValidationService;
        private readonly IInvoiceCreationService _invoiceCreationService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly INotificationHubService _hubService;

        public YachtTourManager(
            IUnitOfWork unitOfWork,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            DailyRevenueJob dailyRevenueJob,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<YachtTourManager> logger,
            IForeignKeyValidationService foreignKeyValidationService,
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
            _yachtTourRepository = yachtTourRepository;
            _guestRepository = guestRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
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
        }

        private decimal GetVatRateForServiceType(string? serviceType)
        {
            var key = (serviceType ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key)) key = "default";

            var byType = _configuration[$"Accounting:Journal:VatRateByServiceType:{key}"]
                         ?? _configuration[$"Accounting:Journal:VatRateByServiceType:{key.ToLowerInvariant()}"];

            if (!string.IsNullOrWhiteSpace(byType) &&
                decimal.TryParse(byType, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedByType) &&
                parsedByType >= 0m)
                return parsedByType;

            var def = _configuration["Accounting:Journal:DefaultVatRate"];
            if (!string.IsNullOrWhiteSpace(def) &&
                decimal.TryParse(def, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedDef) &&
                parsedDef >= 0m)
                return parsedDef;

            return 0m;
        }

        private static decimal CalculateVatAmountFromVatInclusiveGross(decimal gross, decimal vatRate)
        {
            if (gross <= 0m) return 0m;
            if (vatRate <= 0m) return 0m;
            var vat = gross * vatRate / (1m + vatRate);
            return Math.Round(vat, 2, MidpointRounding.AwayFromZero);
        }

        public async Task<ServiceMessage<AddYachtTourResponseDto>> AddYachtTour(AddYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId,
                    CityId = yachtTour.CityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage<AddYachtTourResponseDto> { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // DATE REALITY: Past-dated entries ARE allowed
                // Service date represents when the operation actually occurred, not when entered
                // Tur tarihi validasyonu kaldırıldı - geçmiş tarihli girişler operasyonel olarak normaldir

                // Kişi sayısı kontrolü (1-100 arası)
                if (yachtTour.NumberOfPeople < 1 || yachtTour.NumberOfPeople > 100)
                {
                    return new ServiceMessage<AddYachtTourResponseDto> { IsSuccess = false, Message = "Kişi sayısı 1 ile 100 arasında olmalıdır." };
                }

                // Fiyat hesaplama
                decimal finalPrice = _priceCalculationService.CalculateFinalPrice(yachtTour.Price, yachtTour.DiscountPercentage);

                // Para birimi belirleme
                var currency = _priceCalculationService.ValidateAndGetCurrency(yachtTour.Currency);

                // Yat turu oluşturma
                var yachtTourEntity = new YachtTourEntity
                {
                    TourDate = yachtTour.TourDate,
                    NumberOfPeople = yachtTour.NumberOfPeople,
                    Price = yachtTour.Price,
                    SpecialRequest = yachtTour.SpecialRequest,
                    YachtName = yachtTour.YachtName,
                    OwnerGuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId,
                    CityId = yachtTour.CityId,
                    DiscountPercentage = yachtTour.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency,
                    PickupPier = yachtTour.PickupPier,
                    DropoffPier = yachtTour.DropoffPier,
                    PierAddress = yachtTour.PierAddress,
                    StartTime = yachtTour.StartTime,
                    EndTime = yachtTour.EndTime,
                    SafetyBriefingTime = yachtTour.SafetyBriefingTime,
                    MarinaPickupTime = yachtTour.MarinaPickupTime,
                    WeatherCheckTime = yachtTour.WeatherCheckTime,
                    FuelLevelCheck = yachtTour.FuelLevelCheck,
                    TourCategory = yachtTour.TourCategory,
                    YachtId = yachtTour.YachtId,
                    CaptainId = yachtTour.CaptainId,
                    LifeGuardCertified = yachtTour.LifeGuardCertified,
                    CoastGuardInspectionDate = yachtTour.CoastGuardInspectionDate,
                    GroupLeaderName = yachtTour.GroupLeaderName,
                    GroupLeaderPhone = yachtTour.GroupLeaderPhone,
                    EmergencyContactName = yachtTour.EmergencyContactName,
                    EmergencyContactPhone = yachtTour.EmergencyContactPhone,
                    EmergencyContactRelation = yachtTour.EmergencyContactRelation,
                    LifeJacketsProvided = yachtTour.LifeJacketsProvided,
                    LifeJacketCount = yachtTour.LifeJacketCount,
                    SafetyEquipmentCheck = yachtTour.SafetyEquipmentCheck,
                    EmergencyEquipment = yachtTour.EmergencyEquipment,
                    YachtCapacity = yachtTour.YachtCapacity,
                    YachtType = yachtTour.YachtType,
                    YachtLicenceRequired = yachtTour.YachtLicenceRequired,
                    CoastGuardApproved = yachtTour.CoastGuardApproved,
                    CrewSize = yachtTour.CrewSize,
                    CaptainExperience = yachtTour.CaptainExperience,
                    FuelRange = yachtTour.FuelRange,
                    WeatherBackupPlan = yachtTour.WeatherBackupPlan,
                    SwimmingProficiency = yachtTour.SwimmingProficiency,
                    MedicalConditions = yachtTour.MedicalConditions,
                    AlcoholPolicy = yachtTour.AlcoholPolicy,
                    FoodBeverageIncluded = yachtTour.FoodBeverageIncluded,
                    BeverageType = yachtTour.BeverageType,
                    MusicSystem = yachtTour.MusicSystem,
                    WaterSportsEquipment = yachtTour.WaterSportsEquipment,
                    MarinaContactName = yachtTour.MarinaContactName,
                    MarinaContactPhone = yachtTour.MarinaContactPhone,
                    SupplierName = yachtTour.SupplierName,
                    SupplierCost = yachtTour.SupplierCost,
                    SupplierCurrency = yachtTour.SupplierCurrency,
                    SupplierInvoiceNumber = yachtTour.SupplierInvoiceNumber,
                    ConciergeInternalNotes = yachtTour.ConciergeInternalNotes
                };

                await _yachtTourRepository.AddAsync(yachtTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(yachtTour.OwnerGuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                InvoicesEntity? invoice = null;
                string? pdfUrl = null;

                // Fatura oluşturma
                if (yachtTour.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (yachtTour.PersonnelId.HasValue && yachtTour.PersonnelId.Value > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(yachtTour.PersonnelId.Value);
                    }

                    // Para birimi yachtTourEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = yachtTourEntity.Currency;

                    invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = yachtTour.InvoiceDescription ?? "Yat turu faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = yachtTour.OwnerGuestId,
                        PersonnelId = yachtTour.PersonnelId,
                        // YachtTourId removed - invoices are now multi-service
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
                                _logger.LogError(emailEx, $"Yat turu fatura e-postası gönderilirken hata: {emailEx.Message}");
                            }
                        }
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, $"Yat turu fatura PDF'i oluşturulurken hata: {pdfEx.Message}");
                        // PDF oluşturma hatası fatura oluşturmayı engellemez, sadece loglanır
                    }
                }

                // Rezervasyon onay e-postası gönder
                if (!string.IsNullOrEmpty(guest.Email) && !guest.IsSpecialGuest)
                {
                    try
                    {
                        var city = await _cityRepository.GetByIdAsync(yachtTour.CityId);
                        var cityName = city != null ? city.CityName : "Bilinmiyor";
                        var details = $"Tur Tarihi: {yachtTour.TourDate:dd.MM.yyyy HH:mm}\n" +
                                     $"Şehir: {cityName}\n" +
                                     $"Yat Adı: {yachtTour.YachtName}\n" +
                                     $"Kişi Sayısı: {yachtTour.NumberOfPeople}\n" +
                                     $"Tutar: {yachtTourEntity.FinalPrice:N2} TRY";

                        await _emailService.SendBookingConfirmationAsync(
                            guest.Email,
                            guest.FullName,
                            "Yat Turu",
                            yachtTour.TourDate,
                            details
                        );
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Yat turu rezervasyon onay e-postası gönderilirken hata: {emailEx.Message}");
                    }
                }

                // Günlük gelir hesaplama
                await _dailyRevenueJob.CalculateDailyRevenue(yachtTour.TourDate.Date);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu eklendi: {yachtTourEntity.Id}");
                return new ServiceMessage<AddYachtTourResponseDto>
                {
                    IsSuccess = true,
                    Message = "Yat turu başarıyla eklendi.",
                    Data = new AddYachtTourResponseDto
                    {
                        YachtTourId = yachtTourEntity.Id,
                        InvoiceId = invoice?.Id,
                        InvoicePdfUrl = pdfUrl
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage<AddYachtTourResponseDto> { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> UpdateYachtTour(UpdateYachtTourDto yachtTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _yachtTourRepository.GetAsync(x => x.Id == yachtTour.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.PersonnelId,
                    CityId = yachtTour.CityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // İş Kuralı Validasyonları
                // DATE REALITY: Past-dated entries ARE allowed
                // Service date represents when the operation actually occurred, not when entered

                // Kişi sayısı kontrolü (1-100 arası)
                if (yachtTour.NumberOfPeople < 1 || yachtTour.NumberOfPeople > 100)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Kişi sayısı 1 ile 100 arasında olmalıdır." };
                }

                // Güncelleme
                existing.TourDate = yachtTour.TourDate;
                existing.NumberOfPeople = yachtTour.NumberOfPeople;
                existing.Price = yachtTour.Price;
                existing.SpecialRequest = yachtTour.SpecialRequest;
                existing.YachtName = yachtTour.YachtName;
                existing.OwnerGuestId = yachtTour.OwnerGuestId;
                existing.PersonnelId = yachtTour.PersonnelId;
                existing.CityId = yachtTour.CityId;
                existing.PickupPier = yachtTour.PickupPier;
                existing.DropoffPier = yachtTour.DropoffPier;
                existing.PierAddress = yachtTour.PierAddress;
                existing.StartTime = yachtTour.StartTime;
                existing.EndTime = yachtTour.EndTime;
                existing.SafetyBriefingTime = yachtTour.SafetyBriefingTime;
                existing.MarinaPickupTime = yachtTour.MarinaPickupTime;
                existing.WeatherCheckTime = yachtTour.WeatherCheckTime;
                existing.FuelLevelCheck = yachtTour.FuelLevelCheck;
                existing.TourCategory = yachtTour.TourCategory;
                existing.YachtId = yachtTour.YachtId;
                existing.CaptainId = yachtTour.CaptainId;
                existing.LifeGuardCertified = yachtTour.LifeGuardCertified;
                existing.CoastGuardInspectionDate = yachtTour.CoastGuardInspectionDate;
                existing.GroupLeaderName = yachtTour.GroupLeaderName;
                existing.GroupLeaderPhone = yachtTour.GroupLeaderPhone;
                existing.EmergencyContactName = yachtTour.EmergencyContactName;
                existing.EmergencyContactPhone = yachtTour.EmergencyContactPhone;
                existing.EmergencyContactRelation = yachtTour.EmergencyContactRelation;
                existing.LifeJacketsProvided = yachtTour.LifeJacketsProvided;
                existing.LifeJacketCount = yachtTour.LifeJacketCount;
                existing.SafetyEquipmentCheck = yachtTour.SafetyEquipmentCheck;
                existing.EmergencyEquipment = yachtTour.EmergencyEquipment;
                existing.YachtCapacity = yachtTour.YachtCapacity;
                existing.YachtType = yachtTour.YachtType;
                existing.YachtLicenceRequired = yachtTour.YachtLicenceRequired;
                existing.CoastGuardApproved = yachtTour.CoastGuardApproved;
                existing.CrewSize = yachtTour.CrewSize;
                existing.CaptainExperience = yachtTour.CaptainExperience;
                existing.FuelRange = yachtTour.FuelRange;
                existing.WeatherBackupPlan = yachtTour.WeatherBackupPlan;
                existing.SwimmingProficiency = yachtTour.SwimmingProficiency;
                existing.MedicalConditions = yachtTour.MedicalConditions;
                existing.AlcoholPolicy = yachtTour.AlcoholPolicy;
                existing.FoodBeverageIncluded = yachtTour.FoodBeverageIncluded;
                existing.BeverageType = yachtTour.BeverageType;
                existing.MusicSystem = yachtTour.MusicSystem;
                existing.WaterSportsEquipment = yachtTour.WaterSportsEquipment;
                existing.MarinaContactName = yachtTour.MarinaContactName;
                existing.MarinaContactPhone = yachtTour.MarinaContactPhone;
                existing.SupplierName = yachtTour.SupplierName;
                existing.SupplierCost = yachtTour.SupplierCost;
                existing.SupplierCurrency = yachtTour.SupplierCurrency;
                existing.SupplierInvoiceNumber = yachtTour.SupplierInvoiceNumber;
                existing.ConciergeInternalNotes = yachtTour.ConciergeInternalNotes;

                // AUDIT TRACEABILITY: Mark as updated with personnel trace
                existing.MarkAsUpdated(yachtTour.PersonnelId);

                await _yachtTourRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu güncellendi: {yachtTour.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> DeleteYachtTour(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _yachtTourRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<GetYachtTourDto> GetYachtTourById(int id)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetByIdAsync(id);
                if (yachtTour == null)
                    throw new Exception("Yat turu bulunamadı.");

                var yachtTourDto = _mapper.Map<GetYachtTourDto>(yachtTour);

                // Calculate payment status using PaymentStatusService
                var paymentStatus = await _paymentStatusService.GetServicePaymentStatusAsync(id, "YachtTour");
                if (paymentStatus != null)
                {
                    yachtTourDto.PaymentStatus = paymentStatus.PaymentStatus;
                    yachtTourDto.PaidAmount = paymentStatus.PaidAmount;
                    yachtTourDto.RemainingAmount = paymentStatus.RemainingAmount;
                    yachtTourDto.PaidAmountByCurrency = paymentStatus.PaidAmountByCurrency;
                    yachtTourDto.RemainingAmountByCurrency = paymentStatus.RemainingAmountByCurrency;
                }

                return yachtTourDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<List<GetYachtTourDto>> GetYachtTours()
        {
            try
            {
                var yachtTours = await _yachtTourRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetYachtTourDto>>(yachtTours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turları listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sayfalanmış yat turlarını getirir
        /// </summary>
        public async Task<PagedResult<GetYachtTourDto>> GetYachtToursPaged(int pageNumber, int pageSize, YachtTourFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _yachtTourRepository.GetAll()
                    .ApplyYachtTourFilters(filters)
                    .ApplyYachtTourSorting(sorting);

                var totalCount = await query.CountAsync();
                var yachtTours = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetYachtTourDto>>(yachtTours);
                return new PagedResult<GetYachtTourDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış yat turları listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<YachtTourDetailDto> GetYachtTourDetailAsync(int id)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .FirstOrDefaultAsync(yt => yt.Id == id && !yt.IsDeleted);

                if (yachtTour == null)
                    throw new Exception("Yat turu bulunamadı.");

                return _mapper.Map<YachtTourDetailDto>(yachtTour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
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

        public async Task<ServiceMessage> CreateYachtTourInvoiceAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var yachtTour = await _yachtTourRepository.GetByIdAsync(id);
                if (yachtTour == null || yachtTour.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };

                // Check if invoice already exists for this yacht tour
                var existingInvoice = await _invoiceRepository.GetAll()
                    .FirstOrDefaultAsync(i => i.InvoiceItems.Any(item => item.ServiceType == "YachtTour" && item.ServiceId == id) && !i.IsDeleted);
                if (existingInvoice != null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu yat turu için zaten fatura oluşturulmuş." };

                // Create invoice
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = await GenerateInvoiceNumber(),
                    IssueDate = DateTime.Now,
                    TotalAmount = yachtTour.FinalPrice,
                    Currency = yachtTour.Currency ?? "TRY",
                    Notes = $"Yat Turu #{id} - {yachtTour.YachtName ?? "Yat"}",
                    GuestId = yachtTour.OwnerGuestId,
                    PersonnelId = yachtTour.CreatedByPersonnelId,
                    Status = InvoiceStatus.Draft,
                    IsPdfGenerated = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync(); // Save to get invoice.Id

                // Add invoice item for this yacht tour
                var vatRate = GetVatRateForServiceType("YachtTour");
                var vatAmount = CalculateVatAmountFromVatInclusiveGross(yachtTour.FinalPrice, vatRate);
                var invoiceItem = new InvoiceItemEntity
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "YachtTour",
                    ServiceId = id,
                    Amount = yachtTour.FinalPrice,
                    VatRate = vatRate,
                    VatAmount = vatAmount,
                    Currency = yachtTour.Currency ?? "TRY",
                    Notes = $"Yat Turu: {yachtTour.YachtName ?? "Yat"}"
                };

                await _invoiceItemRepository.AddAsync(invoiceItem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Yat turu faturası oluşturuldu: {id} -> Invoice #{invoice.Id}");
                return new ServiceMessage { IsSuccess = true, Message = $"Fatura başarıyla oluşturuldu. Fatura No: {invoice.InvoiceNumber}" };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura oluşturulurken hata: {ex.Message}. YachtTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> SendYachtTourConfirmationAsync(int id)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetByIdAsync(id);
                if (yachtTour == null || yachtTour.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };

                var guest = await _guestRepository.GetByIdAsync(yachtTour.OwnerGuestId);
                if (guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bilgileri bulunamadı." };

                // Send confirmation email
                var subject = $"Yat Turu Onayı - #{id}";
                var body = $@"
Merhaba {guest.FullName},

Yat turunuz onaylanmıştır.

Tur Detayları:
- Tarih: {yachtTour.TourDate:dd.MM.yyyy HH:mm}
- Yat: {yachtTour.YachtName ?? "Yat"}
- Fiyat: {yachtTour.FinalPrice} {yachtTour.Currency ?? "TRY"}

Saygılarımla,
Hotel Concierge Team
";

                await _emailService.SendEmailAsync(guest.Email, subject, body);

                _logger.LogInformation($"Yat turu onay maili gönderildi: {id} -> {guest.Email}");
                return new ServiceMessage { IsSuccess = true, Message = "Onay maili başarıyla gönderildi." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Onay maili gönderilirken hata: {ex.Message}. YachtTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Onay maili gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Update yacht tour status (mark completed/cancelled)
        /// </summary>
        public async Task<ServiceMessage> UpdateYachtTourStatusAsync(int id, string status)
        {
            try
            {
                var yachtTour = await _yachtTourRepository.GetByIdAsync(id);
                if (yachtTour == null || yachtTour.IsDeleted)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Yat turu bulunamadı." };
                }

                // Validate status transitions
                if (!IsValidStatusTransition(yachtTour.Status, status))
                {
                    return new ServiceMessage { IsSuccess = false, Message = $"Geçersiz durum geçişi: {yachtTour.Status} -> {status}" };
                }

                yachtTour.Status = status;
                yachtTour.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                // Send live update
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("YachtTour", id, "status_updated");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                _logger.LogInformation($"Yat turu durumu güncellendi: {id} -> {status}");
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Yat turu durumu güncellenirken hata: {ex.Message}. YachtTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Yat turu durumu güncellenirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Validate status transitions for yacht tours
        /// </summary>
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            // Allow transitions from any status to Completed or Cancelled
            if (newStatus == "Completed" || newStatus == "Cancelled")
            {
                return true;
            }

            // Allow transition from Pending to Confirmed
            if (currentStatus == "Pending" && newStatus == "Confirmed")
            {
                return true;
            }

            // Allow transition from Confirmed to InProgress
            if (currentStatus == "Confirmed" && newStatus == "InProgress")
            {
                return true;
            }

            return false;
        }
    }
}