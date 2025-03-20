using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
   public interface IPersonnel
    {


        string FullName { get; set; }
        string Email { get; set; }
        string PasswordHash { get; set; }


    }
}
