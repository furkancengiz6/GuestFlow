using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Extensions;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Vehicle
{
    public class VehicleManager : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<VehicleEntity> _vehicleRepository;
        private readonly ILogger<VehicleManager> _logger;
        private readonly IMapper _mapper;
        private readonly ICacheService _cacheService;

        public VehicleManager(
            IUnitOfWork unitOfWork,
            IRepository<VehicleEntity> vehicleRepository,
            ILogger<VehicleManager> logger,
            IMapper mapper,
            ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
            _mapper = mapper;
            _cacheService = cacheService;
        }

        public async Task<ServiceMessage> AddVehicle(AddVehicleDto vehicle)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyon
                if (await _vehicleRepository.GetAll(x => x.PlateNumber == vehicle.PlateNumber).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Bu plaka numarası zaten kayıtlı." };

                // Araç oluşturma
                var vehicleEntity = new VehicleEntity
                {
                    Type = vehicle.Type,
                    PlateNumber = vehicle.PlateNumber,
                    Capacity = vehicle.Capacity,
                    DailyPrice = vehicle.DailyPrice
                };

                await _vehicleRepository.AddAsync(vehicleEntity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Araç eklendi: {vehicle.PlateNumber}");
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Araç eklenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Araç eklenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> UpdateVehicle(UpdateVehicleDto vehicle)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validasyon
                var existing = await _vehicleRepository.GetAsync(x => x.Id == vehicle.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Araç bulunamadı." };

                if (await _vehicleRepository.GetAll(x => x.PlateNumber == vehicle.PlateNumber && x.Id != vehicle.Id).AnyAsync())
                    return new ServiceMessage { IsSuccess = false, Message = "Bu plaka numarası başka bir araçta kullanılıyor." };

                // Güncelleme
                existing.Type = vehicle.Type;
                existing.PlateNumber = vehicle.PlateNumber;
                existing.Capacity = vehicle.Capacity;
                existing.DailyPrice = vehicle.DailyPrice;

                await _vehicleRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();

                // Cache'i temizle
                _cacheService.RemoveByPattern("vehicles:*");
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Araç güncellendi: {vehicle.Id}");
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Araç güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Araç güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<ServiceMessage> DeleteVehicle(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                await _vehicleRepository.DeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Araç silindi: {id}");
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Araç silinirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Araç silinirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }

        public async Task<GetVehicleDto> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(id);
                if (vehicle == null)
                    throw new Exception("Araç bulunamadı.");

                return new GetVehicleDto
                {
                    Id = vehicle.Id,
                    Type = vehicle.Type,
                    PlateNumber = vehicle.PlateNumber,
                    Capacity = vehicle.Capacity,
                    DailyPrice = vehicle.DailyPrice,
                    CreatedDate = vehicle.CreatedDate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Araç getirilirken hata: {ex.Message}. Id: {id}");
                throw;
            }
        }

        public async Task<List<GetVehicleDto>> GetVehicles()
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAll().ToListAsync();
                return _mapper.Map<List<GetVehicleDto>>(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Araçlar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sayfalanmış araçları getirir
        /// </summary>
        public async Task<PagedResult<GetVehicleDto>> GetVehiclesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null)
        {
            try
            {
                var query = _vehicleRepository.GetAll()
                    .ApplyVehicleSorting(sorting);

                var totalCount = await query.CountAsync();
                var vehicles = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<GetVehicleDto>>(vehicles);
                return new PagedResult<GetVehicleDto>(dtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış araçlar listelenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw;
            }
        }
    }
}