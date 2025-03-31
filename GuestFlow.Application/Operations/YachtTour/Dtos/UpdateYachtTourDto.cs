using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
   public class UpdateYachtTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public string SpecialRequest { get; set; }
        public string YachtName { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
    }
}
