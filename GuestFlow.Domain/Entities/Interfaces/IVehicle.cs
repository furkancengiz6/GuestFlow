using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Interfaces
{
   public interface IVehicle
    {
       
        string Type { get; set; }
       string PlateNumber { get; set; }
       int Capacity { get; set; }
       decimal DailyPrice { get; set; }
    }
}
