using AutoMapper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Operations.Payment;
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

namespace GuestFlow.Application.Operations.CityTour
{
    public class CityTourManager : ICityTourService
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyorum.
        // _cityTourRepository: Şehir turlarıyla ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _guestRepository: Misafirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _cityRepository: Şehirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _personnelRepository: Personellerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _invoiceRepository: Faturalarla ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _dailyRevenueJob: Günlük geliri hesaplamak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<InvoiceItemEntity> _invoiceItemRepository;
        private readonly DailyRevenueJob _dailyRevenueJob;
        private readonly IPdfService _pdfService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CityTourManager> _logger;
        private readonly IForeignKeyValidationService _foreignKeyValidationService;
        private readonly ICurrencyService _currencyService;
        private readonly IPdfUrlService _pdfUrlService;
        private readonly IMapper _mapper;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IDateValidationService _dateValidationService;
        private readonly IInvoiceCreationService _invoiceCreationService;
        private readonly IPaymentStatusService _paymentStatusService;
        private readonly INotificationHubService _hubService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityTourManager(
            IUnitOfWork unitOfWork,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<InvoiceItemEntity> invoiceItemRepository,
            DailyRevenueJob dailyRevenueJob,
            IPdfService pdfService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<CityTourManager> logger,
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
            _cityTourRepository = cityTourRepository;
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

        // Bu metodumla yeni bir şehir turu ekliyorum.
        public async Task<ServiceMessage<AddCityTourResponseDto>> AddCityTour(AddCityTourDto cityTour)
        {
            try
            {
                // Veritabanında bir işlem başlatıyorum.
                await _unitOfWork.BeginTransactionAsync();

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId,
                    VehicleId = cityTour.VehicleId,
                    TourId = cityTour.TourId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage<AddCityTourResponseDto> { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // DATE REALITY: Past-dated entries ARE allowed
                // Service date represents when the operation actually occurred, not when entered
                // Tur tarihi validasyonu kaldırıldı - geçmiş tarihli girişler operasyonel olarak normaldir

                // Süre kontrolü (1-24 saat arası)
                if (cityTour.DurationHours < 1 || cityTour.DurationHours > 24)
                {
                    return new ServiceMessage<AddCityTourResponseDto> { IsSuccess = false, Message = "Tur süresi 1 ile 24 saat arasında olmalıdır." };
                }

                // Fiyat hesaplama
                decimal finalPrice = _priceCalculationService.CalculateFinalPrice(cityTour.Price, cityTour.DiscountPercentage);

                // Para birimi belirleme
                var currency = _priceCalculationService.ValidateAndGetCurrency(cityTour.Currency);

                // Yeni bir şehir turu nesnesi oluşturuyorum ve DTO'dan gelen bilgileri buraya aktarıyorum.
                var cityTourEntity = new CityTourEntity
                {
                    TourDate = cityTour.TourDate,
                    Language = cityTour.Language,
                    DurationHours = cityTour.DurationHours,
                    Price = cityTour.Price,
                    OwnerGuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId,
                    TourId = cityTour.TourId,
                    DiscountPercentage = cityTour.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency,
                    StartTime = cityTour.StartTime,
                    EndTime = cityTour.EndTime,
                    PickupTime = cityTour.PickupTime,
                    TourConfirmationTime = cityTour.TourConfirmationTime,
                    VehicleId = cityTour.VehicleId,
                    TourGuideId = cityTour.TourGuideId,
                    AssistantGuideId = cityTour.AssistantGuideId,
                    DriverName = cityTour.DriverName,
                    GuideName = cityTour.GuideName,
                    GuidePhone = cityTour.GuidePhone,
                    GuideLanguages = cityTour.GuideLanguages,
                    BackupGuideName = cityTour.BackupGuideName,
                    BackupGuidePhone = cityTour.BackupGuidePhone,
                    ExternalVehiclePlate = cityTour.ExternalVehiclePlate,
                    ExternalDriverName = cityTour.ExternalDriverName,
                    ExternalDriverPhone = cityTour.ExternalDriverPhone,
                    GroupLeaderName = cityTour.GroupLeaderName,
                    GroupLeaderPhone = cityTour.GroupLeaderPhone,
                    EmergencyContactName = cityTour.EmergencyContactName,
                    EmergencyContactPhone = cityTour.EmergencyContactPhone,
                    EmergencyContactRelation = cityTour.EmergencyContactRelation,
                    MeetingPersonName = cityTour.MeetingPersonName,
                    MeetingPointDetails = cityTour.MeetingPointDetails,
                    TourDifficultyLevel = cityTour.TourDifficultyLevel,
                    WeatherDependent = cityTour.WeatherDependent,
                    MinimumParticipantCount = cityTour.MinimumParticipantCount,
                    MaximumParticipantCount = cityTour.MaximumParticipantCount,
                    DietaryRequirements = cityTour.DietaryRequirements,
                    AccessibilityNeeds = cityTour.AccessibilityNeeds,
                    PhotographyAllowed = cityTour.PhotographyAllowed,
                    SpecialEquipment = cityTour.SpecialEquipment,
                    SupplierName = cityTour.SupplierName,
                    SupplierCost = cityTour.SupplierCost,
                    SupplierCurrency = cityTour.SupplierCurrency,
                    SupplierInvoiceNumber = cityTour.SupplierInvoiceNumber,
                    ConciergeInternalNotes = cityTour.ConciergeInternalNotes
                };

                // Yeni şehir turunu veritabanına ekliyorum.
                await _cityTourRepository.AddAsync(cityTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(cityTour.OwnerGuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                InvoicesEntity? invoice = null;
                string? pdfUrl = null;

                // Eğer fatura oluşturulması isteniyorsa, bir fatura oluşturuyorum.
                if (cityTour.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (cityTour.PersonnelId.HasValue && cityTour.PersonnelId.Value > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(cityTour.PersonnelId.Value);
                    }

                    // Para birimi cityTourEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = cityTourEntity.Currency;

                    invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = cityTour.InvoiceDescription ?? "Şehir turu faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = cityTour.OwnerGuestId,
                        PersonnelId = cityTour.PersonnelId,
                        // CityTourId removed - invoices are now multi-service
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
                                _logger.LogError(emailEx, $"Şehir turu fatura e-postası gönderilirken hata: {emailEx.Message}");
                            }
                        }
                    }
                    catch (Exception pdfEx)
                    {
                        _logger.LogError(pdfEx, $"Şehir turu fatura PDF'i oluşturulurken hata: {pdfEx.Message}");
                        // PDF oluşturma hatası fatura oluşturmayı engellemez, sadece loglanır
                    }
                }

                // Rezervasyon onay e-postası gönder
                if (!string.IsNullOrEmpty(guest.Email) && !guest.IsSpecialGuest)
                {
                    try
                    {
                        var city = await _cityRepository.GetByIdAsync(cityTour.CityId);
                        var cityName = city != null ? city.CityName : "Bilinmiyor";
                        var details = $"Tur Tarihi: {cityTour.TourDate:dd.MM.yyyy HH:mm}\n" +
                                     $"Şehir: {cityName}\n" +
                                     $"Dil: {cityTour.Language}\n" +
                                     $"Süre: {cityTour.DurationHours} saat\n" +
                                     $"Tutar: {cityTourEntity.FinalPrice:N2} TRY";

                        await _emailService.SendBookingConfirmationAsync(
                            guest.Email,
                            guest.FullName,
                            "Şehir Turu",
                            cityTour.TourDate,
                            details
                        );
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, $"Şehir turu rezervasyon onay e-postası gönderilirken hata: {emailEx.Message}");
                    }
                }

                // O gün için günlük geliri hesaplıyorum.
                await _dailyRevenueJob.CalculateDailyRevenue(cityTour.TourDate.Date);

                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir turu eklendi: {cityTourEntity.Id}");
                return new ServiceMessage<AddCityTourResponseDto>
                {
                    IsSuccess = true,
                    Message = "Şehir turu başarıyla eklendi.",
                    Data = new AddCityTourResponseDto
                    {
                        CityTourId = cityTourEntity.Id,
                        InvoiceId = invoice?.Id,
                        InvoicePdfUrl = pdfUrl
                    }
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir turu eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage<AddCityTourResponseDto> { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla fatura numarası üretiyorum.
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

        // Bu metodumla mevcut bir şehir turunu güncelliyorum.
        public async Task<ServiceMessage> UpdateCityTour(UpdateCityTourDto cityTour)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek şehir turunu ID'sine göre veritabanından çekiyorum.
                var existing = await _cityTourRepository.GetAsync(x => x.Id == cityTour.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu bulunamadı." };

                // Foreign Key Validasyonları
                var fkValidation = await _foreignKeyValidationService.ValidateMultipleAsync(new ForeignKeyValidationRequest
                {
                    GuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // İş Kuralı Validasyonları
                // DATE REALITY: Past-dated entries ARE allowed
                // Service date represents when the operation actually occurred, not when entered

                // Süre kontrolü (1-24 saat arası)
                if (cityTour.DurationHours < 1 || cityTour.DurationHours > 24)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur süresi 1 ile 24 saat arasında olmalıdır." };
                }

                // Güncel bilgileri mevcut kayda aktarıyorum.
                existing.TourDate = cityTour.TourDate;
                existing.Language = cityTour.Language;
                existing.DurationHours = cityTour.DurationHours;
                existing.Price = cityTour.Price;
                existing.OwnerGuestId = cityTour.OwnerGuestId;
                existing.PersonnelId = cityTour.PersonnelId;
                existing.CityId = cityTour.CityId;
                existing.TourId = cityTour.TourId == 0 ? existing.TourId : cityTour.TourId;
                existing.DiscountPercentage = cityTour.DiscountPercentage;
                existing.Currency = cityTour.Currency ?? existing.Currency;
                existing.StartTime = cityTour.StartTime;
                existing.EndTime = cityTour.EndTime;
                existing.PickupTime = cityTour.PickupTime;
                existing.TourConfirmationTime = cityTour.TourConfirmationTime;
                existing.VehicleId = cityTour.VehicleId;
                existing.TourGuideId = cityTour.TourGuideId;
                existing.AssistantGuideId = cityTour.AssistantGuideId;
                existing.DriverName = cityTour.DriverName;
                existing.GuideName = cityTour.GuideName;
                existing.GuidePhone = cityTour.GuidePhone;
                existing.GuideLanguages = cityTour.GuideLanguages;
                existing.BackupGuideName = cityTour.BackupGuideName;
                existing.BackupGuidePhone = cityTour.BackupGuidePhone;
                existing.ExternalVehiclePlate = cityTour.ExternalVehiclePlate;
                existing.ExternalDriverName = cityTour.ExternalDriverName;
                existing.ExternalDriverPhone = cityTour.ExternalDriverPhone;
                existing.GroupLeaderName = cityTour.GroupLeaderName;
                existing.GroupLeaderPhone = cityTour.GroupLeaderPhone;
                existing.EmergencyContactName = cityTour.EmergencyContactName;
                existing.EmergencyContactPhone = cityTour.EmergencyContactPhone;
                existing.EmergencyContactRelation = cityTour.EmergencyContactRelation;
                existing.MeetingPersonName = cityTour.MeetingPersonName;
                existing.MeetingPointDetails = cityTour.MeetingPointDetails;
                existing.TourDifficultyLevel = cityTour.TourDifficultyLevel;
                existing.WeatherDependent = cityTour.WeatherDependent;
                existing.MinimumParticipantCount = cityTour.MinimumParticipantCount;
                existing.MaximumParticipantCount = cityTour.MaximumParticipantCount;
                existing.DietaryRequirements = cityTour.DietaryRequirements;
                existing.AccessibilityNeeds = cityTour.AccessibilityNeeds;
                existing.PhotographyAllowed = cityTour.PhotographyAllowed;
                existing.SpecialEquipment = cityTour.SpecialEquipment;
                existing.SupplierName = cityTour.SupplierName;
                existing.SupplierCost = cityTour.SupplierCost;
                existing.SupplierCurrency = cityTour.SupplierCurrency;
                existing.SupplierInvoiceNumber = cityTour.SupplierInvoiceNumber;
                existing.ConciergeInternalNotes = cityTour.ConciergeInternalNotes;

                // AUDIT TRACEABILITY: Mark as updated with personnel trace
                existing.MarkAsUpdated(cityTour.PersonnelId);

                await _cityTourRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir turu güncellendi: {cityTour.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu güncellenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir turu güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla bir şehir turunu siliyorum.
        public async Task<ServiceMessage> DeleteCityTour(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _cityTourRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir turu silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu silinirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir turu silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla belirli bir şehir turunu ID'sine göre getiriyorum.
        public async Task<GetCityTourDto> GetCityTourById(int id)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetByIdAsync(id);
                if (cityTour == null)
                    throw new Exception("Şehir turu bulunamadı.");

                var cityTourDto = _mapper.Map<GetCityTourDto>(cityTour);

                // Calculate payment status using PaymentStatusService
                var paymentStatus = await _paymentStatusService.GetServicePaymentStatusAsync(id, "CityTour");
                if (paymentStatus != null)
                {
                    cityTourDto.PaymentStatus = paymentStatus.PaymentStatus;
                    cityTourDto.PaidAmount = paymentStatus.PaidAmount;
                    cityTourDto.RemainingAmount = paymentStatus.RemainingAmount;
                    cityTourDto.PaidAmountByCurrency = paymentStatus.PaidAmountByCurrency;
                    cityTourDto.RemainingAmountByCurrency = paymentStatus.RemainingAmountByCurrency;
                }

                return cityTourDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turu getirilirken hata çıktı: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metodumla tüm şehir turlarını getiriyorum.
        public async Task<List<GetCityTourDto>> GetCityTours()
        {
            try
            {
                var cityTours = await _cityTourRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetCityTourDto>>(cityTours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turları listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sayfalanmış şehir turlarını getirir
        /// </summary>
        public async Task<PagedResult<GetCityTourDto>> GetCityToursPaged(int pageNumber, int pageSize, CityTourFilterParameters? filters = null, SortingParameters? sorting = null)
        {
            try
            {
                var query = _cityTourRepository.GetAll()
                    .ApplyCityTourFilters(filters)
                    .ApplyCityTourSorting(sorting);

                var totalCount = await query.CountAsync();
                var cityTours = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetCityTourDto>>(cityTours);
                return new PagedResult<GetCityTourDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış şehir turları listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        public async Task<CityTourDetailDto> GetCityTourDetailAsync(int id)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .FirstOrDefaultAsync(ct => ct.Id == id && !ct.IsDeleted);

                if (cityTour == null)
                    throw new Exception("Şehir turu bulunamadı.");

                return _mapper.Map<CityTourDetailDto>(cityTour);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turu detayı getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<ServiceMessage> CreateCityTourInvoiceAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var cityTour = await _cityTourRepository.GetByIdAsync(id);
                if (cityTour == null || cityTour.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu bulunamadı." };

                // Check if invoice already exists for this city tour
                var existingInvoice = await _invoiceRepository.GetAll()
                    .FirstOrDefaultAsync(i => i.InvoiceItems.Any(item => item.ServiceType == "CityTour" && item.ServiceId == id) && !i.IsDeleted);
                if (existingInvoice != null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu şehir turu için zaten fatura oluşturulmuş." };

                // Create invoice
                var invoice = new InvoicesEntity
                {
                    InvoiceNumber = await GenerateInvoiceNumber(),
                    IssueDate = DateTime.Now,
                    TotalAmount = cityTour.FinalPrice,
                    Currency = cityTour.Currency ?? "TRY",
                    Notes = $"Şehir Turu #{id} - {cityTour.Tour?.Name ?? "Tur"}",
                    GuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.CreatedByPersonnelId,
                    Status = InvoiceStatus.Draft,
                    IsPdfGenerated = false
                };

                await _invoiceRepository.AddAsync(invoice);
                await _unitOfWork.SaveChangesAsync(); // Save to get invoice.Id

                // Add invoice item for this city tour
                var vatRate = GetVatRateForServiceType("CityTour");
                var vatAmount = CalculateVatAmountFromVatInclusiveGross(cityTour.FinalPrice, vatRate);
                var invoiceItem = new InvoiceItemEntity
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "CityTour",
                    ServiceId = id,
                    Amount = cityTour.FinalPrice,
                    VatRate = vatRate,
                    VatAmount = vatAmount,
                    Currency = cityTour.Currency ?? "TRY",
                    Notes = $"Şehir Turu: {cityTour.Tour?.Name ?? "Tur"}"
                };

                await _invoiceItemRepository.AddAsync(invoiceItem);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir turu faturası oluşturuldu: {id} -> Invoice #{invoice.Id}");
                return new ServiceMessage { IsSuccess = true, Message = $"Fatura başarıyla oluşturuldu. Fatura No: {invoice.InvoiceNumber}" };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Fatura oluşturulurken hata: {ex.Message}. CityTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Fatura oluşturulurken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> SendCityTourConfirmationAsync(int id)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetByIdAsync(id);
                if (cityTour == null || cityTour.IsDeleted)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu bulunamadı." };

                var guest = await _guestRepository.GetByIdAsync(cityTour.OwnerGuestId);
                if (guest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bilgileri bulunamadı." };

                // Send confirmation email
                var subject = $"Şehir Turu Onayı - #{id}";
                var body = $@"
Merhaba {guest.FullName},

Şehir turunuz onaylanmıştır.

Tur Detayları:
- Tarih: {cityTour.TourDate:dd.MM.yyyy HH:mm}
- Tur: {cityTour.Tour?.Name ?? "Şehir Turu"}
- Fiyat: {cityTour.FinalPrice} {cityTour.Currency ?? "TRY"}

Saygılarımla,
Hotel Concierge Team
";

                await _emailService.SendEmailAsync(guest.Email, subject, body);

                _logger.LogInformation($"Şehir turu onay maili gönderildi: {id} -> {guest.Email}");
                return new ServiceMessage { IsSuccess = true, Message = "Onay maili başarıyla gönderildi." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Onay maili gönderilirken hata: {ex.Message}. CityTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Onay maili gönderilirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Update city tour status (mark completed/cancelled)
        /// </summary>
        public async Task<ServiceMessage> UpdateCityTourStatusAsync(int id, string status)
        {
            try
            {
                var cityTour = await _cityTourRepository.GetByIdAsync(id);
                if (cityTour == null || cityTour.IsDeleted)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir turu bulunamadı." };
                }

                // Validate status transitions
                if (!IsValidStatusTransition(cityTour.Status, status))
                {
                    return new ServiceMessage { IsSuccess = false, Message = $"Geçersiz durum geçişi: {cityTour.Status} -> {status}" };
                }

                cityTour.Status = status;
                cityTour.UpdatedDate = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                // Send live update
                if (_hubService != null)
                {
                    await _hubService.SendLiveUpdateAsync("CityTour", id, "status_updated");
                    await _hubService.SendDashboardUpdateAsync(new { });
                }

                _logger.LogInformation($"Şehir turu durumu güncellendi: {id} -> {status}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu durumu başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turu durumu güncellenirken hata: {ex.Message}. CityTourId: {id}");
                return new ServiceMessage { IsSuccess = false, Message = $"Şehir turu durumu güncellenirken hata: {ex.Message}" };
            }
        }

        /// <summary>
        /// Validate status transitions for city tours
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