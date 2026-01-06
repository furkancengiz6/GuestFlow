using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.ServicePackage.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.ServicePackage
{
    public interface IServicePackageService
    {
        Task<ServiceMessage<GetServicePackageDto>> AddServicePackage(AddServicePackageDto package);
        Task<ServiceMessage> UpdateServicePackage(UpdateServicePackageDto package);
        Task<ServiceMessage> DeleteServicePackage(int id);
        Task<GetServicePackageDto> GetServicePackageById(int id);
        Task<List<GetServicePackageDto>> GetServicePackages();
        Task<PagedResult<GetServicePackageDto>> GetServicePackagesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
        Task<ServiceMessage> AddTransferToPackage(int packageId, int transferId);
        Task<ServiceMessage> AddCityTourToPackage(int packageId, int cityTourId);
        Task<ServiceMessage> AddYachtTourToPackage(int packageId, int yachtTourId);
        Task<ServiceMessage> AddRestaurantReservationToPackage(int packageId, int reservationId);
        Task<ServiceMessage> RemoveTransferFromPackage(int packageId, int transferId);
        Task<ServiceMessage> RemoveCityTourFromPackage(int packageId, int cityTourId);
        Task<ServiceMessage> RemoveYachtTourFromPackage(int packageId, int yachtTourId);
        Task<ServiceMessage> RemoveRestaurantReservationFromPackage(int packageId, int reservationId);
        Task<decimal> CalculatePackageTotalCost(int packageId);
    }
}

