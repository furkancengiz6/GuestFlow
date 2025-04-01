using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Airport.Dtos
{
   public class AddAirportDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int CityId { get; set; }
    }
}
