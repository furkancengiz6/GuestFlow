using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Hotel.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Hotel
{
    public interface IHotelService
    {
        Task<ServiceMessage> AddHotel(AddHotelDto hotel);
        Task<ServiceMessage> UpdateHotel(UpdateHotelDto hotel);
        Task<ServiceMessage> DeleteHotel(int id);
        Task<GetHotelDto> GetHotelById(int id);
        Task<List<GetHotelDto>> GetHotels();
        Task<PagedResult<GetHotelDto>> GetHotelsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
        Task<List<GetHotelDto>> GetHotelsByCityId(int cityId);
    }
}

