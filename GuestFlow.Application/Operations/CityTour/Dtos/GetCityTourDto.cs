using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
    public class GetCityTourDto
    {
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; }
        public int CityId { get; set; }
        public int? TourId { get; set; } // Nullable - mevcut kayıtlarda null olabilir
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public DateTime CreatedDate { get; set; }
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
        // Şoför ve araç bilgileri
        public int? VehicleId { get; set; }
        public string? DriverName { get; set; }
        
        // Rehber bilgileri
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }

        // Payment status (calculated from PaymentEntity)
        public string? PaymentStatus { get; set; } // Unpaid, PartiallyPaid, Paid
        public decimal? PaidAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        public Dictionary<string, decimal>? PaidAmountByCurrency { get; set; }
        public Dictionary<string, decimal>? RemainingAmountByCurrency { get; set; }
    }
}
