using GuestFlow.Domain.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestCityTour:BaseEntity
    {
        public int GuestId { get; set; }
        public virtual GuestEntity Guest { get; set; }
        public int CityTourId { get; set; }
        public virtual CityTourEntity CityTour { get; set; }
    }
}
