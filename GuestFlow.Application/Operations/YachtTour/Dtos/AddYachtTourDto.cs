using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
    public class AddYachtTourDto
    {
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public string SpecialRequest { get; set; }
        public string YachtName { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
        public bool CreateInvoice { get; set; } 
        public decimal? DiscountPercentage { get; set; } 
        public string? InvoiceDescription { get; set; } 
    }
}
