using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
   public class AddCityTourDto
    {
        public DateTime TourDate { get; set; }
        public string Language { get; set; }
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
        public bool CreateInvoice { get; set; } 
        public decimal? DiscountPercentage { get; set; } 
        public string? InvoiceDescription { get; set; }
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, vb.)
    }
}
