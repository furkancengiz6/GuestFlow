using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.CityTourModels
{
    public class AddCityTourRequest
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

        // PersonnelId artık zorunlu değil, otomatik doldurulacak
        public int? PersonnelId { get; set; }

        [Required] 
        public int CityId { get; set; }
        
        [Required]
        public int TourId { get; set; } // Admin tarafından eklenen tur
        
        /// <summary>
        /// INVOICE REALITY: Invoices are NOT created automatically.
        /// Default is FALSE - invoice creation is time-based (checkout, end-of-day, manual).
        /// </summary>
        public bool CreateInvoice { get; set; } = false;
        public decimal? DiscountPercentage { get; set; } //  İndirim yüzdesi
        public string? InvoiceDescription { get; set; }
        [StringLength(3)]
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, GBP, RUB)
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati
        
        // Şoför ve araç bilgileri
        public int? VehicleId { get; set; } // Zorunlu değil
        [StringLength(200)]
        public string? DriverName { get; set; } // Şoför isim soyisim
        [StringLength(20)]
        public string? CaptainPhone { get; set; } // Kaptan telefonu
        
        // Rehber bilgileri
        [StringLength(200)]
        public string? GuideName { get; set; } // Rehber isim
        [StringLength(20)]
        public string? GuidePhone { get; set; } // Rehber telefon numarası
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        [StringLength(20)]
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        [StringLength(200)]
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        [StringLength(20)]
        public string? ExternalDriverPhone { get; set; } // Dışarıdan çekilen şoför telefon numarası

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
