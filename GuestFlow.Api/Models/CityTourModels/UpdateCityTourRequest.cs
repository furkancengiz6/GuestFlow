using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityTourModels
{
    public class UpdateCityTourRequest
    {
        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; } = string.Empty;

        [Required]
        [Range(1, 24)]
        public int DurationHours { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int OwnerGuestId { get; set; }

        public int? PersonnelId { get; set; }

        [Required]
        public int CityId { get; set; }

        // Opsiyonel alanlar
        public int? TourId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int? VehicleId { get; set; }
        public string? DriverName { get; set; }
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }
        public string? CaptainPhone { get; set; }
        
        /// <summary>
        /// DEPRECATED: Payment status is calculated from PaymentEntity.
        /// This field is ignored - use Payments module to record payments.
        /// </summary>
        [Obsolete("Payment status is calculated from PaymentEntity. This field is ignored.")]
        public bool IsPaymentReceived { get; set; } // DEPRECATED - Do not use
        
        public string? PaymentNote { get; set; }
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierPaymentStatus { get; set; }
        public DateTime? SupplierPaymentDate { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
    }
}
