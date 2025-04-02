using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue
{
    public class DailyRevenueJob
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyorum (örneğin, transaction başlatmak, kaydetmek).
        // _cityTourRepository: Şehir turlarıyla ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _transferRepository: Transferlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _yachtTourRepository: Yat turlarıyla ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _dailyRevenueRepository: Günlük gelirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak (kaydetmek) için kullanıyorum.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<DailyRevenueEntity> _dailyRevenueRepository;
        private readonly ILogger<DailyRevenueJob> _logger;

        // Constructor (yapıcı metod): Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public DailyRevenueJob(
            IUnitOfWork unitOfWork,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            ILogger<DailyRevenueJob> logger)
        {
            _unitOfWork = unitOfWork;
            _cityTourRepository = cityTourRepository;
            _transferRepository = transferRepository;
            _yachtTourRepository = yachtTourRepository;
            _dailyRevenueRepository = dailyRevenueRepository;
            _logger = logger;
        }

        // Bu metodumla belirli bir gün için toplam geliri hesaplıyorum.
        public async Task CalculateDailyRevenue(DateTime date)
        {
            try
            {
                // Önce veritabanında bir işlem (transaction) başlatıyorum. Eğer bir hata çıkarsa, yaptığım değişiklikleri geri alacağım.
                

                // İlk olarak, o gün için şehir turlarından gelen geliri hesaplıyorum.
                // Veritabanından şehir turlarını çekiyorum, ama sadece o güne ait olanları (TourDate) ve FinalPrice'ı null olmayanları alıyorum.
                // SumAsync ile tüm FinalPrice'ları topluyorum.
                var cityTourRevenue = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date == date.Date && ct.FinalPrice != null)
                    .SumAsync(ct => ct.FinalPrice);

                // Şehir turları gelirini logluyorum ki sonradan bakarsam ne kadar kazandığımı görebileyim.
                _logger.LogInformation($"Şehir turları geliri ({date:yyyy-MM-dd}): {cityTourRevenue}");

                // Şimdi o gün için transferlerden gelen geliri hesaplıyorum.
                // Yine aynı şekilde, o güne ait transferleri ve FinalPrice'ı null olmayanları alıyorum.
                var transferRevenue = await _transferRepository.GetAll()
                    .Where(t => t.TransferDate.Date == date.Date && t.FinalPrice != null)
                    .SumAsync(t => t.FinalPrice);

                // Transfer gelirini logluyorum.
                _logger.LogInformation($"Transfer geliri ({date:yyyy-MM-dd}): {transferRevenue}");

                // Şimdi de o gün için yat turlarından gelen geliri hesaplıyorum.
                // Aynı mantıkla, o güne ait yat turlarını ve FinalPrice'ı null olmayanları alıyorum.
                var yachtTourRevenue = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date == date.Date && yt.FinalPrice != null)
                    .SumAsync(yt => yt.FinalPrice);

                // Yat turları gelirini logluyorum.
                _logger.LogInformation($"Yat turları geliri ({date:yyyy-MM-dd}): {yachtTourRevenue}");

                // Tüm gelirleri toplayarak o günün toplam gelirini buluyorum.
                var totalRevenue = cityTourRevenue + transferRevenue + yachtTourRevenue;

                // Toplam geliri logluyorum ki ne kadar kazandığımı bileyim.
                _logger.LogInformation($"Toplam gelir ({date:yyyy-MM-dd}): {totalRevenue}");

                // Şimdi, bu tarih için daha önce bir günlük gelir kaydı var mı diye kontrol ediyorum.
                var existingRevenue = await _dailyRevenueRepository.GetAsync(dr => dr.Date.Date == date.Date);
                if (existingRevenue != null)
                {
                    // Eğer bir kayıt varsa, sadece toplam geliri güncelliyorum.
                    existingRevenue.TotalRevenue = totalRevenue;
                    await _dailyRevenueRepository.UpdateAsync(existingRevenue);
                    _logger.LogInformation($"Mevcut günlük gelir güncellendi ({date:yyyy-MM-dd}): {totalRevenue}");
                }
                else
                {
                    // Eğer kayıt yoksa, yeni bir günlük gelir kaydı oluşturuyorum.
                    var dailyRevenue = new DailyRevenueEntity
                    {
                        Date = date.Date,
                        TotalRevenue = totalRevenue,
                        CreatedDate = DateTime.UtcNow, // Şu anki tarihi ekliyorum.
                        IsDeleted = false // Silinmedi olarak işaretliyorum.
                    };
                    await _dailyRevenueRepository.AddAsync(dailyRevenue);
                    _logger.LogInformation($"Yeni günlük gelir eklendi ({date:yyyy-MM-dd}): {totalRevenue}");
                }

                // Değişiklikleri veritabanına kaydediyorum.
                await _unitOfWork.SaveChangesAsync();
                // İşlem başarılıysa transaction'ı tamamlıyorum (commit ediyorum).
                await _unitOfWork.CommitTransactionAsync();

                // Her şeyin yolunda gittiğini logluyorum.
                _logger.LogInformation($"Günlük gelir hesaplandı ({date:yyyy-MM-dd}): {totalRevenue}");
            }
            catch (Exception ex)
            {
                // Eğer bir hata çıkarsa, transaction'ı geri alıyorum (rollback yapıyorum).
                
                // Hatayı logluyorum. İç hata (InnerException) varsa onu da ekliyorum ki daha fazla bilgi alayım.
                _logger.LogError(ex, $"Günlük gelir hesaplanırken hata çıktı ({date:yyyy-MM-dd}): {ex.Message}. InnerException: {ex.InnerException?.Message}");
                // Hata olduğu için bu hatayı yukarıya fırlatıyorum (throw).
                throw;
            }
        }
        public async Task UpdateDailyRevenue(DateTime date, decimal amount)
        {
            try
            {
                
                var dailyRevenue = await _dailyRevenueRepository.GetAsync(x => x.Date.Date == date.Date);
                if (dailyRevenue == null)
                {
                    dailyRevenue = new DailyRevenueEntity
                    {
                        Date = date.Date,
                        TotalRevenue = amount,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    await _dailyRevenueRepository.AddAsync(dailyRevenue);
                    _logger.LogInformation($"Yeni günlük gelir eklendi ({date:yyyy-MM-dd}): {amount}");
                }
                else
                {
                    dailyRevenue.TotalRevenue += amount;
                    await _dailyRevenueRepository.UpdateAsync(dailyRevenue);
                    _logger.LogInformation($"Günlük gelir güncellendi ({date:yyyy-MM-dd}): {dailyRevenue.TotalRevenue}");
                }

                await _unitOfWork.SaveChangesAsync(); // Mevcut transaction içinde çalışacak
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Günlük gelir güncellenirken hata çıktı ({date:yyyy-MM-dd}): {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}