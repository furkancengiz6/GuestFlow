/*using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.City.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.City
{
    public class CityManager : ICityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly ILogger<CityManager> _logger;

        public CityManager(
            IUnitOfWork unitOfWork,
            IRepository<CityEntity> cityRepository,
            ILogger<CityManager> logger)
        {
            _unitOfWork = unitOfWork;
            _cityRepository = cityRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddCity(AddCityDto city)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var nameExists = await _cityRepository.GetAll(x => x.Name == city.Name && !x.IsDeleted).AnyAsync();
                if (nameExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu şehir zaten mevcut." };

                var cityEntity = new CityEntity
                {
                    Name = city.Name,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _cityRepository.AddAsync(cityEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir eklendi: {Name}", city.Name);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir eklenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Şehir eklenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> UpdateCity(UpdateCityDto city)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _cityRepository.GetAsync(x => x.Id == city.Id && !x.IsDeleted);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                var nameExists = await _cityRepository.GetAll(x => x.Name == city.Name && x.Id != city.Id && !x.IsDeleted).AnyAsync();
                if (nameExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu isimde başka bir şehir mevcut." };

                existing.Name = city.Name;

                await _cityRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir güncellendi: {Id}", city.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Şehir güncellenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> DeleteCity(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var city = await _cityRepository.GetAsync(x => x.Id == id && !x.IsDeleted);
                if (city == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Şehir bulunamadı." };

                await _cityRepository.DeleteAsync(city);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Şehir silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Şehir başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Şehir silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Şehir silinirken hata: " + ex.Message };
            }
        }

        public async Task<GetCityDto> GetCityById(int id)
        {
            try
            {
                var city = await _cityRepository.GetAll(x => x.Id == id && !x.IsDeleted)
                    .Select(c => new GetCityDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CreatedDate = c.CreatedDate
                    })
                    .FirstOrDefaultAsync();

                return city ?? throw new Exception("Şehir bulunamadı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şehir getirilirken hata oluştu: {Id}", id);
                return null;
            }
        }

        public async Task<List<GetCityDto>> GetCities()
        {
            try
            {
                var cities = await _cityRepository.GetAll(x => !x.IsDeleted)
                    .Select(c => new GetCityDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CreatedDate = c.CreatedDate
                    })
                    .ToListAsync();

                return cities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şehirler listelenirken hata oluştu.");
                return new List<GetCityDto>();
            }
        }
    }
}
*/