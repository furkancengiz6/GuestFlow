using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
   public interface IGuest
    {
        string FullName { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        string Nationality { get; set; }
    }
}
