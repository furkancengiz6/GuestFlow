using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Vehicle
{
    public interface IVehicleService
    {
        Task<ServiceMessage> AddVehicle(AddVehicleDto vehicle);
        Task<ServiceMessage> UpdateVehicle(UpdateVehicleDto vehicle);
        Task<ServiceMessage> DeleteVehicle(int id);
        Task<GetVehicleDto> GetVehicleById(int id);
        Task<List<GetVehicleDto>> GetVehicles();
        
        /// <summary>
        /// Sayfalanmış araçları getirir
        /// </summary>
        Task<PagedResult<GetVehicleDto>> GetVehiclesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
    }
}