using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Vehicle.Dtos
{
   public  class GetVehicleDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string PlateNumber { get; set; }
        public int Capacity { get; set; }
        public decimal DailyPrice { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
