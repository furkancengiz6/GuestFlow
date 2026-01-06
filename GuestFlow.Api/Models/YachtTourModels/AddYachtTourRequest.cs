using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.YachtTourModels
{
    public class AddYachtTourRequest
    {
        [Required]
        public DateTime TourDate { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int NumberOfPeople { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        public string? SpecialRequest { get; set; }

        [StringLength(100)]
        public string? YachtName { get; set; }

        [Required]
        public int OwnerGuestId { get; set; }

        // PersonnelId artık zorunlu değil, otomatik doldurulacak
        public int? PersonnelId { get; set; }

        [Required] 
        public int CityId { get; set; }
        
        /// <summary>
        /// INVOICE REALITY: Invoices are NOT created automatically.
        /// Default is FALSE - invoice creation is time-based (checkout, end-of-day, manual).
        /// </summary>
        public bool CreateInvoice { get; set; } = false;
        public decimal? DiscountPercentage { get; set; } 
        public string? InvoiceDescription { get; set; }
        [StringLength(3)]
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, GBP, RUB)
        
        // İskele bilgileri
        [StringLength(200)]
        public string? PickupPier { get; set; } // Alış iskelesi
        [StringLength(200)]
        public string? DropoffPier { get; set; } // Bırakış iskelesi
        [StringLength(500)]
        public string? PierAddress { get; set; } // Serbest metin
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati

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
