using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Restaurant.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Restaurant
{
    public interface IRestaurantService
    {
        Task<ServiceMessage> AddRestaurant(AddRestaurantDto restaurant);
        Task<ServiceMessage> UpdateRestaurant(UpdateRestaurantDto restaurant);
        Task<ServiceMessage> DeleteRestaurant(int id);
        Task<GetRestaurantDto> GetRestaurantById(int id);
        Task<List<GetRestaurantDto>> GetRestaurants();
        Task<PagedResult<GetRestaurantDto>> GetRestaurantsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
        Task<List<GetRestaurantDto>> GetRestaurantsByCityId(int cityId);
    }
}

