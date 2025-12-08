using AutoMapper;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.City.Dtos;
using GuestFlow.Application.Operations.Cache;
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

namespace GuestFlow.Application.Operations.City
{
    public class CityManager : ICityService
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _unitOfWork: Veritabanı işlemlerini yönetmek için kullanıyorum.
        // _cityRepository: Şehirlerle ilgili veritabanı işlemlerini yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly ILogger<CityManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityManager(
            IUnitOfWork unitOfWork,
            IRepository<CityEntity> cityRepository,
            ILogger<CityManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _cityRepository = cityRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        // Bu metodumla yeni bir şehir ekliyorum.
        public async Task<ServiceMessage> AddCity(AddCityDto city)
        {
            try
            {
                // Veritabanında bir işlem başlatıyorum.
                await _unitOfWork.BeginTransactionAsync();

                // Yeni bir şehir nesnesi oluşturuyorum ve DTO'dan gelen bilgileri buraya aktarıyorum.
                var cityEntity = new CityEntity
                {
                    CityName = city.CityName,
                    Country = city.Country,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                // Yeni şehri veritabanına ekliyorum.
                await _cityRepository.AddAsync(cityEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir eklendi: {cityEntity.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir eklenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla mevcut bir şehri güncelliyorum.
        public async Task<ServiceMessage> UpdateCity(UpdateCityDto city)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Güncellenecek şehri ID'sine göre veritabanından çekiyorum.
                var existing = await _cityRepository.GetAsync(x => x.Id == city.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                // Güncel bilgileri mevcut kayda aktarıyorum.
                existing.CityName = city.CityName;
                existing.Country = city.Country;

                await _cityRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                _cacheService.RemoveByPattern("cities:*");
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir güncellendi: {city.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir güncellenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla bir şehri siliyorum.
        public async Task<ServiceMessage> DeleteCity(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Silinecek şehri ID'sine göre kontrol ediyorum.
                var city = await _cityRepository.GetAsync(x => x.Id == id);
                if (city == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                await _cityRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Şehir silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Şehir silinirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Şehir silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        // Bu metodumla belirli bir şehri ID'sine göre getiriyorum.
        public async Task<GetCityDto> GetCityById(int id)
        {
            try
            {
                var city = await _cityRepository.GetByIdAsync(id);
                if (city == null)
                    throw new Exception("Şehir bulunamadı.");

                return _mapper.Map<GetCityDto>(city);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehir getirilirken hata çıktı: {ex.Message}. Id: {id}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        // Bu metodumla tüm şehirleri getiriyorum.
        public async Task<List<GetCityDto>> GetCities()
        {
            try
            {
                var cities = await _cityRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetCityDto>>(cities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şehirler listelenirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sayfalanmış şehirleri getirir
        /// </summary>
        public async Task<PagedResult<GetCityDto>> GetCitiesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            try
            {
                var query = _cityRepository.GetAll()
                    .ApplyCitySorting(sorting);

                var totalCount = await query.CountAsync();
                var cities = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetCityDto>>(cities);
                return new PagedResult<GetCityDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış şehirler listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}