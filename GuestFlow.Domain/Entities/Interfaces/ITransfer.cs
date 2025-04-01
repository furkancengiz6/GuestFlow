using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Interfaces
{
   public interface ITransfer
    {
        string PickupAddress { get; set; }
        string DropoffAddress { get; set; }
        DateTime TransferDate { get; set; }
        decimal Price { get; set; }
        string Note { get; set; }

    }
}
