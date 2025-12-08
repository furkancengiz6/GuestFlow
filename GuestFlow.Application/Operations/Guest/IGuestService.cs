using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest
{
    public interface IGuestService
    {
        Task<ServiceMessage> AddGuest(AddGuestDto guest);
        Task<ServiceMessage> UpdateGuest(UpdateGuestDto guest);
        Task<ServiceMessage> DeleteGuest(int id);
        Task<GetGuestDto> GetGuestById(int id);
        Task<List<GetGuestDto>> GetGuests();
        Task<PagedResult<GetGuestDto>> GetGuestsPaged(int pageNumber, int pageSize, GuestFilterParameters? filters = null, SortingParameters? sorting = null);
        
        /// <summary>
        /// Misafir detayını getirir (geçmiş ile)
        /// </summary>
        Task<GuestDetailDto> GetGuestDetailAsync(int id);
        
        /// <summary>
        /// Misafir faturalarını getirir
        /// </summary>
        Task<List<GuestInvoiceDto>> GetGuestInvoicesAsync(int guestId);
        
        /// <summary>
        /// Misafir zaman çizelgesini getirir (transferler, turlar kronolojik sırada)
        /// </summary>
        Task<List<GuestTimelineItemDto>> GetGuestTimelineAsync(int guestId);
    }
}