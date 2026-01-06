using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
    public class GetYachtTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string? SpecialRequest { get; set; }
        public string? YachtName { get; set; }
        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int CityId { get; set; }
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // İskele bilgileri
        public string? PickupPier { get; set; }
        public string? DropoffPier { get; set; }
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // Payment status (calculated from PaymentEntity)
        public string? PaymentStatus { get; set; } // Unpaid, PartiallyPaid, Paid
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public Dictionary<string, decimal>? PaidAmountByCurrency { get; set; }
        public Dictionary<string, decimal>? RemainingAmountByCurrency { get; set; }
    }
}
