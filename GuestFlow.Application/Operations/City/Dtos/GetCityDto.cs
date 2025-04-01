using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.City.Dtos
{
    public class GetCityDto
    {
        public int Id { get; set; }
        public string CityName { get; set; }
        public string Country { get; set; }
    }
}
