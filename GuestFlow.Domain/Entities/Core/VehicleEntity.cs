using GuestFlow.Domain.Entities.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Core
{
    public class VehicleEntity:BaseEntity,IVehicle
    {

       
        public string Type { get; set; }
        public string PlateNumber { get; set; }
        public int Capacity { get; set; }
        public decimal DailyPrice { get; set; }

        //Relational Property
        public virtual ICollection<TransferEntity> Transfers { get; set; }
    }
}
