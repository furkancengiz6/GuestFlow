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
    }
}