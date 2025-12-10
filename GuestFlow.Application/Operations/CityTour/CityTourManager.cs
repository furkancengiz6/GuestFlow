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

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityTourManager(
            IUnitOfWork unitOfWork,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
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
            IInvoiceCreationService invoiceCreationService)
        {
            _unitOfWork = unitOfWork;
            _cityTourRepository = cityTourRepository;
            _guestRepository = guestRepository;
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

        // Bu metodumla yeni bir şehir turu ekliyorum.
        public async Task<ServiceMessage> AddCityTour(AddCityTourDto cityTour)
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
                    CityId = cityTour.CityId
                });

                if (!fkValidation.IsValid)
                {
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // İş Kuralı Validasyonları
                // 1. Tur tarihi geçmişte olamaz
                if (cityTour.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur tarihi bugünden önceki bir tarih olamaz." };
                }

                // 2. Süre kontrolü (1-24 saat arası)
                if (cityTour.DurationHours < 1 || cityTour.DurationHours > 24)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur süresi 1 ile 24 saat arasında olmalıdır." };
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
                    DiscountPercentage = cityTour.DiscountPercentage,
                    FinalPrice = finalPrice,
                    Currency = currency
                };

                // Yeni şehir turunu veritabanına ekliyorum.
                await _cityTourRepository.AddAsync(cityTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(cityTour.OwnerGuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // Eğer fatura oluşturulması isteniyorsa, bir fatura oluşturuyorum.
                if (cityTour.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (cityTour.PersonnelId > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(cityTour.PersonnelId);
                    }

                    // Para birimi cityTourEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = cityTourEntity.Currency;

                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = cityTour.InvoiceDescription ?? "Şehir turu faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = cityTour.OwnerGuestId,
                        PersonnelId = cityTour.PersonnelId > 0 ? cityTour.PersonnelId : null,
                        CityTourId = cityTourEntity.Id,
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
                return new ServiceMessage { IsSuccess = true, Message = "Şehir turu başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir turu eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir turu eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
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
                // 1. Tur tarihi geçmişte olamaz
                if (cityTour.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur tarihi bugünden önceki bir tarih olamaz." };
                }

                // 2. Süre kontrolü (1-24 saat arası)
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

                return _mapper.Map<GetCityTourDto>(cityTour);
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
    }
}