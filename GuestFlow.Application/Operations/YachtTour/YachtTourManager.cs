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
using GuestFlow.Application.Operations.YachtTour.Dtos;
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

        public YachtTourManager(
            IUnitOfWork unitOfWork,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
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
            IInvoiceCreationService invoiceCreationService)
        {
            _unitOfWork = unitOfWork;
            _yachtTourRepository = yachtTourRepository;
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

        public async Task<ServiceMessage> AddYachtTour(AddYachtTourDto yachtTour)
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
                    return new ServiceMessage { IsSuccess = false, Message = fkValidation.ErrorMessage };
                }

                // İş Kuralı Validasyonları
                // 1. Tur tarihi geçmişte olamaz
                if (yachtTour.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur tarihi bugünden önceki bir tarih olamaz." };
                }

                // 2. Kişi sayısı kontrolü (1-100 arası)
                if (yachtTour.NumberOfPeople < 1 || yachtTour.NumberOfPeople > 100)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Kişi sayısı 1 ile 100 arasında olmalıdır." };
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
                    Currency = currency
                };

                await _yachtTourRepository.AddAsync(yachtTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Misafir bilgisini al (rezervasyon onay e-postası için)
                var guest = await _guestRepository.GetByIdAsync(yachtTour.OwnerGuestId);
                if (guest == null)
                    throw new Exception("Misafir bulunamadı.");

                // Fatura oluşturma
                if (yachtTour.CreateInvoice)
                {

                    PersonnelEntity? personnel = null;
                    if (yachtTour.PersonnelId > 0)
                    {
                        personnel = await _personnelRepository.GetByIdAsync(yachtTour.PersonnelId);
                    }

                    // Para birimi yachtTourEntity'den alınır (zaten set edilmiş)
                    var invoiceCurrency = yachtTourEntity.Currency;

                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = invoiceCurrency,
                        Notes = yachtTour.InvoiceDescription ?? "Yat turu faturası",
                        PdfUrl = string.Empty, // PDF oluşturulduktan sonra güncellenecek
                        GuestId = yachtTour.OwnerGuestId,
                        PersonnelId = yachtTour.PersonnelId > 0 ? yachtTour.PersonnelId : null,
                        YachtTourId = yachtTourEntity.Id,
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
                return new ServiceMessage { IsSuccess = true, Message = "Yat turu başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Yat turu eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Yat turu eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
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
                // 1. Tur tarihi geçmişte olamaz
                if (yachtTour.TourDate.Date < DateTime.UtcNow.Date)
                {
                    return new ServiceMessage { IsSuccess = false, Message = "Tur tarihi bugünden önceki bir tarih olamaz." };
                }

                // 2. Kişi sayısı kontrolü (1-100 arası)
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

                return _mapper.Map<GetYachtTourDto>(yachtTour);
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

                var detail = new YachtTourDetailDto
                {
                    Id = yachtTour.Id,
                    TourDate = yachtTour.TourDate,
                    NumberOfPeople = yachtTour.NumberOfPeople,
                    Price = yachtTour.Price,
                    FinalPrice = yachtTour.FinalPrice,
                    SpecialRequest = yachtTour.SpecialRequest,
                    YachtName = yachtTour.YachtName,
                    CreatedDate = yachtTour.CreatedDate,
                    Guest = yachtTour.OwnerGuest != null ? new TourGuestDto
                    {
                        Id = yachtTour.OwnerGuest.Id,
                        FullName = yachtTour.OwnerGuest.FullName,
                        GuestCode = yachtTour.OwnerGuest.GuestCode,
                        Email = yachtTour.OwnerGuest.Email,
                        PhoneNumber = yachtTour.OwnerGuest.PhoneNumber,
                        Nationality = yachtTour.OwnerGuest.Nationality,
                        IsSpecialGuest = yachtTour.OwnerGuest.IsSpecialGuest
                    } : null,
                    Personnel = yachtTour.Personnel != null ? new TourPersonnelDto
                    {
                        Id = yachtTour.Personnel.Id,
                        FullName = yachtTour.Personnel.FullName,
                        Email = yachtTour.Personnel.Email,
                        UserType = yachtTour.Personnel.UserType.ToString()
                    } : null,
                    City = yachtTour.City != null ? new TourCityDto
                    {
                        Id = yachtTour.City.Id,
                        CityName = yachtTour.City.CityName,
                        Country = yachtTour.City.Country
                    } : null
                };

                return detail;
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
    }
}