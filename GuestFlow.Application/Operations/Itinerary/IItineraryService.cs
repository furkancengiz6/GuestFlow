using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Itinerary.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Itinerary
{
    public interface IItineraryService
    {
        Task<ServiceMessage<GetItineraryDto>> AddItinerary(AddItineraryDto itinerary);
        Task<ServiceMessage> UpdateItinerary(UpdateItineraryDto itinerary);
        Task<ServiceMessage> DeleteItinerary(int id);
        Task<GetItineraryDto> GetItineraryById(int id);
        Task<List<GetItineraryDto>> GetItinerariesByGuestId(int guestId);
        Task<PagedResult<GetItineraryDto>> GetItinerariesPaged(int pageNumber, int pageSize, SortingParameters? sorting = null);
        Task<ItineraryTimelineDto> GetItineraryTimeline(int itineraryId);
        Task<ServiceMessage> AddItineraryItem(int itineraryId, AddItineraryItemDto item);
        Task<ServiceMessage> UpdateItineraryItem(int itineraryId, int itemId, AddItineraryItemDto item);
        Task<ServiceMessage> DeleteItineraryItem(int itineraryId, int itemId);
        Task<ServiceMessage> UpdateItineraryStatus(int id, Domain.Entities.Enum.ItineraryStatus status);
        Task<decimal> CalculateItineraryTotalCost(int itineraryId);
    }
}

