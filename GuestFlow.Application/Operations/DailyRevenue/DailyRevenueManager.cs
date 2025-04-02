using GuestFlow.Application.Operations.DailyRevenue.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyRevenue
{
    public class DailyRevenueManager : IDailyRevenueService
    {
        // Burada kullanacağım üç değişkeni tanımlıyorum.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyorum (örneğin, transaction başlatmak, kaydetmek).
        // _dailyRevenueRepository: Günlük gelirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak (kaydetmek) için kullanıyorum.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<DailyRevenueEntity> _dailyRevenueRepository;
        private readonly ILogger<DailyRevenueManager> _logger;

        // Constructor (yapıcı metod): Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public DailyRevenueManager(
            IUnitOfWork unitOfWork,
            IRepository<DailyRevenueEntity> dailyRevenueRepository,
            ILogger<DailyRevenueManager> logger)
        {
            _unitOfWork = unitOfWork;
            _dailyRevenueRepository = dailyRevenueRepository;
            _logger = logger;
        }

        // Bu metodumla yeni bir günlük gelir kaydı ekliyorum.
        public async Task<ServiceMessage> AddDailyRevenue(AddDailyRevenueDto dailyRevenue)
        {
            try
            {
                
               

                // Yeni bir günlük gelir nesnesi oluşturuyorum ve DTO'dan gelen bilgileri buraya aktarıyorum.
                var dailyRevenueEntity = new DailyRevenueEntity
                {
                    Date = dailyRevenue.Date,
                    TotalRevenue = dailyRevenue.TotalRevenue,
                    CreatedDate = DateTime.UtcNow, // Şu anki tarihi ekliyorum.
                    IsDeleted = false // Silinmedi olarak işaretliyorum.
                };

                // Yeni günlük geliri veritabanına ekliyorum.
                await _dailyRevenueRepository.AddAsync(dailyRevenueEntity);
                // Değişiklikleri veritabanına kaydediyorum.
                await _unitOfWork.SaveChangesAsync();
                // İşlem başarılıysa transaction'ı tamamlıyorum (commit ediyorum).
                await _unitOfWork.CommitTransactionAsync();

                // Başarılı bir şekilde eklediğimi logluyorum, böylece sonradan bakarsam ne olduğunu görebilirim.
                _logger.LogInformation($"Günlük gelir eklendi: {dailyRevenueEntity.Id}");
                // Başarı mesajı döndürüyorum.
                return new ServiceMessage { IsSuccess = true, Message = "Günlük gelir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                
               
                // Hatayı logluyorum. Eğer bir iç hata (InnerException) varsa, onu da ekliyorum ki daha fazla bilgi alayım.
                _logger.LogError(ex, $"Günlük gelir eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                // Hata mesajını oluşturuyorum. İç hata varsa onu da ekliyorum.
                string errorMessage = $"Günlük gelir eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                // Hata mesajıyla birlikte başarısız bir sonuç döndürüyorum.
                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla mevcut bir günlük gelir kaydını güncelliyorum.
        public async Task<ServiceMessage> UpdateDailyRevenue(UpdateDailyRevenueDto dailyRevenue)
        {
            try
            {
                
              

                // Güncellenecek günlük geliri ID'sine göre veritabanından çekiyorum.
                var existing = await _dailyRevenueRepository.GetAsync(x => x.Id == dailyRevenue.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Günlük gelir bulunamadı." };

                // Güncel bilgileri mevcut kayda aktarıyorum.
                existing.Date = dailyRevenue.Date;
                existing.TotalRevenue = dailyRevenue.TotalRevenue;

                // Güncellenmiş kaydı veritabanına kaydediyorum.
                await _dailyRevenueRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Güncellemeyi logluyorum ki sonradan bakarsam ne yaptığımı hatırlayayım.
                _logger.LogInformation($"Günlük gelir güncellendi: {dailyRevenue.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Günlük gelir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, $"Günlük gelir güncellenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Günlük gelir güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla bir günlük gelir kaydını siliyorum.
        public async Task<ServiceMessage> DeleteDailyRevenue(int id)
        {
            try
            {
                

                // Silinecek günlük geliri ID'sine göre kontrol ediyorum.
                var dailyRevenue = await _dailyRevenueRepository.GetAsync(x => x.Id == id);
                if (dailyRevenue == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Günlük gelir bulunamadı." };

                // Kaydı veritabanından siliyorum.
                await _dailyRevenueRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // Silme işlemini logluyorum.
                _logger.LogInformation($"Günlük gelir silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Günlük gelir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, $"Günlük gelir silinirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Günlük gelir silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla belirli bir günlük geliri ID'sine göre getiriyorum.
        public async Task<GetDailyRevenueDto> GetDailyRevenueById(int id)
        {
            try
            {
                // Veritabanından günlük geliri ID'sine göre çekiyorum.
                var dailyRevenue = await _dailyRevenueRepository.GetByIdAsync(id);
                if (dailyRevenue == null)
                    throw new Exception("Günlük gelir bulunamadı.");

                // Günlük geliri bir DTO nesnesine çevirip geri döndürüyorum.
                return new GetDailyRevenueDto
                {
                    Id = dailyRevenue.Id,
                    Date = dailyRevenue.Date,
                    TotalRevenue = dailyRevenue.TotalRevenue,
                    CreatedDate = dailyRevenue.CreatedDate
                };
            }
            catch (Exception ex)
            {
                // Hata çıkarsa logluyorum ve hatayı yukarıya fırlatıyorum.
                _logger.LogError(ex, $"Günlük gelir getirilirken hata çıktı: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metodumla tüm günlük gelirleri getiriyorum.
        public async Task<List<GetDailyRevenueDto>> GetDailyRevenues()
        {
            try
            {
                // Veritabanından tüm günlük gelirleri çekiyorum ve her birini GetDailyRevenueDto'ya çeviriyorum.
                return await _dailyRevenueRepository.GetAll()
                    .Select(dr => new GetDailyRevenueDto
                    {
                        Id = dr.Id,
                        Date = dr.Date,
                        TotalRevenue = dr.TotalRevenue,
                        CreatedDate = dr.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Hata çıkarsa logluyorum ve hatayı yukarıya fırlatıyorum.
                _logger.LogError(ex, $"Günlük gelirler listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}