using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.YachtTourModels
{
    public class UpdateYachtTourRequest
    {
        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfPeople { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string SpecialRequest { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string YachtName { get; set; } = string.Empty;

        [Required]
        public int OwnerGuestId { get; set; }

        [Required]
        public int PersonnelId { get; set; }

        [Required] 
        public int CityId { get; set; }

        // İskele bilgileri
        [StringLength(200)]
        public string? PickupPier { get; set; }
        [StringLength(200)]
        public string? DropoffPier { get; set; }
        [StringLength(500)]
        public string? PierAddress { get; set; }
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // Kategori ve kaptan
        public GuestFlow.Domain.Entities.Enum.TourCategory? TourCategory { get; set; }
        [StringLength(20)]
        public string? CaptainPhone { get; set; }

        // Ödeme takibi
        /// <summary>
        /// DEPRECATED: Payment status is calculated from PaymentEntity.
        /// This field is ignored - use Payments module to record payments.
        /// </summary>
        [Obsolete("Payment status is calculated from PaymentEntity. This field is ignored.")]
        public bool IsPaymentReceived { get; set; } // DEPRECATED - Do not use
        
        [StringLength(500)]
        public string? PaymentNote { get; set; }

        // Tedarikçi maliyet bilgileri
        [StringLength(200)]
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        [StringLength(3)]
        public string? SupplierCurrency { get; set; }
        [StringLength(20)]
        public string? SupplierPaymentStatus { get; set; } // Paid, Unpaid, Partial
        public DateTime? SupplierPaymentDate { get; set; }
        [StringLength(100)]
        public string? SupplierInvoiceNumber { get; set; }
    }
}
