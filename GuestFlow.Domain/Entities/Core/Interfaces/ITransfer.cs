using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
   public interface ITransfer
    {
        string PickupAdress { get; set; }
        string DropoffAdress { get; set; }
        DateTime TransferDate { get; set; }
        decimal Price { get; set; }
        string Note { get; set; }

    }
}
