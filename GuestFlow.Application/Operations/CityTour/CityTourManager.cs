using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Application.Operations.DailyRevenue;
using GuestFlow.Application.Operations.Invoice.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<CityTourManager> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityTourManager(
            IUnitOfWork unitOfWork,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<PersonnelEntity> personnelRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            DailyRevenueJob dailyRevenueJob,
            ILogger<CityTourManager> logger)
        {
            _unitOfWork = unitOfWork;
            _cityTourRepository = cityTourRepository;
            _guestRepository = guestRepository;
            _cityRepository = cityRepository;
            _personnelRepository = personnelRepository;
            _invoiceRepository = invoiceRepository;
            _dailyRevenueJob = dailyRevenueJob;
            _logger = logger;
        }

        // Bu metodumla yeni bir şehir turu ekliyorum.
        public async Task<ServiceMessage> AddCityTour(AddCityTourDto cityTour)
        {
            try
            {
                // Veritabanında bir işlem başlatıyorum.
                await _unitOfWork.BeginTransactionAsync();

                // Misafirin var olup olmadığını kontrol ediyorum.
                var guestExists = await _guestRepository.GetAll(x => x.Id == cityTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                // Şehrin var olup olmadığını kontrol ediyorum.
                var cityExists = await _cityRepository.GetAll(x => x.Id == cityTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                // Personelin var olup olmadığını kontrol ediyorum.
                var personnelExists = await _personnelRepository.GetAll(x => x.Id == cityTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

                // İndirim varsa, son fiyatı hesaplıyorum.
                decimal finalPrice = cityTour.Price;
                if (cityTour.DiscountPercentage.HasValue)
                    finalPrice -= finalPrice * (cityTour.DiscountPercentage.Value / 100);

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
                    FinalPrice = finalPrice
                };

                // Yeni şehir turunu veritabanına ekliyorum.
                await _cityTourRepository.AddAsync(cityTourEntity);
                await _unitOfWork.SaveChangesAsync();

                // Eğer fatura oluşturulması isteniyorsa, bir fatura oluşturuyorum.
                if (cityTour.CreateInvoice)
                {
                    var invoice = new InvoicesEntity
                    {
                        InvoiceNumber = await GenerateInvoiceNumber(),
                        IssueDate = DateTime.UtcNow,
                        TotalAmount = finalPrice,
                        Currency = "TRY",
                        Notes = cityTour.InvoiceDescription ?? "Şehir turu faturası",
                        PdfUrl = $"https://example.com/invoices/invoice_{Guid.NewGuid()}.pdf", // PDF URL'sini dinamik olarak oluşturuyorum.
                        GuestId = cityTour.OwnerGuestId,
                        CityTourId = cityTourEntity.Id,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _invoiceRepository.AddAsync(invoice);
                    await _unitOfWork.SaveChangesAsync();
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
            // Veritabanındaki son faturayı çekiyorum ve numarasını alıyorum.
            var lastInvoice = await _invoiceRepository.GetAll().OrderByDescending(x => x.InvoiceNumber).FirstOrDefaultAsync();
            // Eğer fatura varsa, son numarayı bir artırıyorum; yoksa 1000'den başlıyorum.
            return lastInvoice != null ? lastInvoice.InvoiceNumber + 1 : 1000;
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

                // Misafirin, şehrin ve personelin var olup olmadığını kontrol ediyorum.
                var guestExists = await _guestRepository.GetAll(x => x.Id == cityTour.OwnerGuestId).AnyAsync();
                if (!guestExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Misafir bulunamadı." };

                var cityExists = await _cityRepository.GetAll(x => x.Id == cityTour.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var personnelExists = await _personnelRepository.GetAll(x => x.Id == cityTour.PersonnelId).AnyAsync();
                if (!personnelExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı." };

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

                return new GetCityTourDto
                {
                    Id = cityTour.Id,
                    TourDate = cityTour.TourDate,
                    Language = cityTour.Language,
                    DurationHours = cityTour.DurationHours,
                    Price = cityTour.Price,
                    OwnerGuestId = cityTour.OwnerGuestId,
                    PersonnelId = cityTour.PersonnelId,
                    CityId = cityTour.CityId,
                    CreatedDate = cityTour.CreatedDate
                };
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
                return await _cityTourRepository.GetAll()
                    .Select(ct => new GetCityTourDto
                    {
                        Id = ct.Id,
                        TourDate = ct.TourDate,
                        Language = ct.Language,
                        DurationHours = ct.DurationHours,
                        Price = ct.Price,
                        OwnerGuestId = ct.OwnerGuestId,
                        PersonnelId = ct.PersonnelId,
                        CityId = ct.CityId,
                        CreatedDate = ct.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir turları listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}