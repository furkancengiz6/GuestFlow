using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest.Dtos
{
   public class GetGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }
        public string? RoomNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
