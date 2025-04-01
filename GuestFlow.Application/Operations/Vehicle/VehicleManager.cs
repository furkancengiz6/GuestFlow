using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Vehicle.Dtos;
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

        public VehicleManager(
            IUnitOfWork unitOfWork,
            IRepository<VehicleEntity> vehicleRepository,
            ILogger<VehicleManager> logger)
        {
            _unitOfWork = unitOfWork;
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        public async Task<ServiceMessage> AddVehicle(AddVehicleDto vehicle)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var plateExists = await _vehicleRepository.GetAll(x => x.PlateNumber == vehicle.PlateNumber).AnyAsync();
                if (plateExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu plaka numarası zaten kayıtlı." };

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

                _logger.LogInformation("Araç eklendi: {PlateNumber}", vehicle.PlateNumber);
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla eklendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Araç eklenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Araç eklenirken hata: " + ex.Message };
            }
        }

        public async Task<ServiceMessage> UpdateVehicle(UpdateVehicleDto vehicle)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _vehicleRepository.GetAsync(x => x.Id == vehicle.Id);
                if (existing == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Araç bulunamadı." };

                var plateExists = await _vehicleRepository.GetAll(x => x.PlateNumber == vehicle.PlateNumber && x.Id != vehicle.Id).AnyAsync();
                if (plateExists)
                    return new ServiceMessage { IsSuccess = false, Message = "Bu plaka numarası başka bir araçta kullanılıyor." };

                existing.Type = vehicle.Type;
                existing.PlateNumber = vehicle.PlateNumber;
                existing.Capacity = vehicle.Capacity;
                existing.DailyPrice = vehicle.DailyPrice;

                await _vehicleRepository.UpdateAsync(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Araç güncellendi: {Id}", vehicle.Id);
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla güncellendi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Araç güncellenirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Araç güncellenirken hata: " + ex.Message };
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

                _logger.LogInformation("Araç silindi: {Id}", id);
                return new ServiceMessage { IsSuccess = true, Message = "Araç başarıyla silindi." };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Araç silinirken hata oluştu.");
                return new ServiceMessage { IsSuccess = false, Message = "Araç silinirken hata: " + ex.Message };
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
                _logger.LogError(ex, "Araç getirilirken hata oluştu: {Id}", id);
                throw;
            }
        }

        public async Task<List<GetVehicleDto>> GetVehicles()
        {
            try
            {
                var vehicles = await _vehicleRepository.GetAll()
                    .Select(v => new GetVehicleDto
                    {
                        Id = v.Id,
                        Type = v.Type,
                        PlateNumber = v.PlateNumber,
                        Capacity = v.Capacity,
                        DailyPrice = v.DailyPrice,
                        CreatedDate = v.CreatedDate
                    })
                    .ToListAsync();

                return vehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Araçlar listelenirken hata oluştu.");
                throw;
            }
        }
    }
}