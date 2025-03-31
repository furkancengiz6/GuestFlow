using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Guest.Dtos
{
  public class UpdateGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; }
        public bool IsSpecialGuest { get; set; }


    }
}
