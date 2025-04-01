using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Airport
{
    public class AirportManager : IAirportService
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyorum.
        // _airportRepository: Havalimanlarıyla ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _cityRepository: Şehirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<AirportEntity> _airportRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly ILogger<AirportManager> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public AirportManager(
            IUnitOfWork unitOfWork,
            IRepository<AirportEntity> airportRepository,
            IRepository<CityEntity> cityRepository,
            ILogger<AirportManager> logger)
        {
            _unitOfWork = unitOfWork;
            _airportRepository = airportRepository;
            _cityRepository = cityRepository;
            _logger = logger;
        }

        // Bu metodumla yeni bir havalimanı ekliyorum.
        public async Task<ServiceMessage> AddAirport(AddAirportDto airport)
        {
            try
            {
                // Veritabanında bir işlem başlatıyorum.
                await _unitOfWork.BeginTransactionAsync();

                // Şehrin var olup olmadığını kontrol ediyorum.
                var cityExists = await _cityRepository.GetAll(x => x.Id == airport.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                // Havalimanı kodunun daha önce kullanılıp kullanılmadığını kontrol ediyorum.
                var codeExists = await _airportRepository.GetAll(x => x.Code == airport.Code).AnyAsync();
                if (codeExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu havalimanı kodu zaten kullanılıyor." };

                // Yeni bir havalimanı nesnesi oluşturuyorum ve DTO'dan gelen bilgileri buraya aktarıyorum.
                var airportEntity = new AirportEntity
                {
                    Name = airport.Name,
                    Code = airport.Code,
                    CityId = airport.CityId
                    // CreatedDate ve IsDeleted gibi alanlar BaseEntity tarafından otomatik ayarlanıyor.
                };

                // Yeni havalimanını veritabanına ekliyorum.
                await _airportRepository.AddAsync(airportEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Havalimanı eklendi: {airport.Name}");
                return new ServiceMessage { IsSuccess = true, Message = "Havalimanı başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Havalimanı eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Havalimanı eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla mevcut bir havalimanını güncelliyorum.
        public async Task<ServiceMessage> UpdateAirport(UpdateAirportDto airport)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek havalimanını ID'sine göre veritabanından çekiyorum.
                var existing = await _airportRepository.GetAsync(x => x.Id == airport.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Havalimanı bulunamadı." };

                // Şehrin var olup olmadığını kontrol ediyorum.
                var cityExists = await _cityRepository.GetAll(x => x.Id == airport.CityId).AnyAsync();
                if (!cityExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Belirtilen şehir bulunamadı." };

                // Havalimanı kodunun başka bir havalimanı tarafından kullanılıp kullanılmadığını kontrol ediyorum.
                var codeExists = await _airportRepository.GetAll(x => x.Code == airport.Code && x.Id != airport.Id).AnyAsync();
                if (codeExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu havalimanı kodu başka bir havalimanı tarafından kullanılıyor." };

                // Güncel bilgileri mevcut kayda aktarıyorum.
                existing.Name = airport.Name;
                existing.Code = airport.Code;
                existing.CityId = airport.CityId;

                await _airportRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Havalimanı güncellendi: {airport.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Havalimanı başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Havalimanı güncellenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Havalimanı güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla bir havalimanını siliyorum.
        public async Task<ServiceMessage> DeleteAirport(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _airportRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Havalimanı silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Havalimanı başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Havalimanı silinirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Havalimanı silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla belirli bir havalimanını ID'sine göre getiriyorum.
        public async Task<GetAirportDto> GetAirportById(int id)
        {
            try
            {
                var airport = await _airportRepository.GetByIdAsync(id);
                if (airport == null)
                    throw new Exception("Havalimanı bulunamadı.");

                return new GetAirportDto
                {
                    Id = airport.Id,
                    Name = airport.Name,
                    Code = airport.Code,
                    CityId = airport.CityId,
                    CreatedDate = airport.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Havalimanı getirilirken hata çıktı: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metodumla tüm havalimanlarını getiriyorum.
        public async Task<List<GetAirportDto>> GetAirports()
        {
            try
            {
                return await _airportRepository.GetAll()
                    .Select(a => new GetAirportDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Code = a.Code,
                        CityId = a.CityId,
                        CreatedDate = a.CreatedDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Havalimanları listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}