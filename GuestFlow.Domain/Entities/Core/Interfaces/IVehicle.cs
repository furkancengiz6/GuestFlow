using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core.Interfaces
{
   public interface IVehicle
    {
       
        string Type { get; set; }
       string PlateNumber { get; set; }
       int Capacity { get; set; }
       decimal DailyPrice { get; set; }
    }
}
