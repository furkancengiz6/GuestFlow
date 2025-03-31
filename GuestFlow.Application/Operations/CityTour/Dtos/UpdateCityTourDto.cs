using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
    public class UpdateCityTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public string Language { get; set; }
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
    }
}
