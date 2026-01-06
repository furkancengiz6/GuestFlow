using GuestFlow.Domain.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.TransferModel
{
    public class AddTransferRequest
    {
        [Required]
        public DateTime TransferDate { get; set; }

        [Required]
        [StringLength(500)]
        public string PickupAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string DropoffAddress { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public int GuestId { get; set; }

        // PersonnelId artık zorunlu değil, otomatik doldurulacak
        public int? PersonnelId { get; set; }

        // AirportId artık zorunlu değil
        public int? AirportId { get; set; }

        // VehicleId artık zorunlu değil
        public int? VehicleId { get; set; }

        public string? Note { get; set; }

        public string? Status { get; set; }

        public bool IsFromAirport { get; set; }

        public TransferType TransferType { get; set; } = TransferType.Custom; // Transfer tipi

        // PickupCityId artık zorunlu değil
        public int? PickupCityId { get; set; }

        // HotelId ve RestaurantId (TransferType'a göre kullanılacak)
        public int? HotelId { get; set; } // Otel ID (AirportToHotel, HotelToAirport, vb. için)
        public int? RestaurantId { get; set; } // Restoran ID (HotelToRestaurant, RestaurantToHotel için)

        // DropoffCityId artık zorunlu değil
        public int? DropoffCityId { get; set; }
        
        /// <summary>
        /// INVOICE REALITY: Invoices are NOT created automatically.
        /// Default is FALSE - invoice creation is time-based (checkout, end-of-day, manual).
        /// </summary>
        public bool CreateInvoice { get; set; } = false;
        public decimal? DiscountPercentage { get; set; }
        public string? InvoiceDescription { get; set; }
        [StringLength(3)]
        public string? Currency { get; set; } // Para birimi (TRY, USD, EUR, GBP, RUB)
        
        // Ödeme bilgileri
        public PaymentMethod? PaymentMethod { get; set; } // Nakit, Kredi Kartı, Odaya Charge
        
        /// <summary>
        /// DEPRECATED: Payment status is calculated from PaymentEntity.
        /// This field is ignored - use Payments module to record payments.
        /// </summary>
        [Obsolete("Payment status is calculated from PaymentEntity. This field is ignored.")]
        public bool IsPaymentReceived { get; set; } // DEPRECATED - Do not use
        
        public string? PaymentNote { get; set; } // Ödeme notu
        
        // Şoför bilgileri
        [StringLength(200)]
        public string? DriverName { get; set; } // Şoför isim soyisim
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        [StringLength(20)]
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        [StringLength(200)]
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        [StringLength(20)]
        public string? ExternalDriverPhone { get; set; } // Dışarıdan çekilen şoför telefon numarası

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
