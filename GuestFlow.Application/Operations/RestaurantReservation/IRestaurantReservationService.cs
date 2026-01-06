using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.RestaurantReservation.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.RestaurantReservation
{
    public interface IRestaurantReservationService
    {
        Task<ServiceMessage<GetRestaurantReservationDto>> AddRestaurantReservation(AddRestaurantReservationDto reservation);
        Task<ServiceMessage> UpdateRestaurantReservation(UpdateRestaurantReservationDto reservation);
        Task<ServiceMessage> DeleteRestaurantReservation(int id);
        Task<GetRestaurantReservationDto> GetRestaurantReservationById(int id);
        Task<List<GetRestaurantReservationDto>> GetRestaurantReservationsByGuestId(int guestId);
        Task<List<GetRestaurantReservationDto>> GetRestaurantReservationsByRestaurantId(int restaurantId);
        Task<PagedResult<GetRestaurantReservationDto>> GetRestaurantReservationsPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
        Task<ServiceMessage> UpdateRestaurantReservationStatus(int id, Domain.Entities.Enum.ReservationStatus status);
        Task<ServiceMessage> ConfirmRestaurantReservation(int id);
        Task<ServiceMessage> CancelRestaurantReservation(int id, string? reason = null);
    }
}

