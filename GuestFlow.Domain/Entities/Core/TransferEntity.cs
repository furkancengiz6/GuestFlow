using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class TransferEntity : BaseEntity, ITransfer
    {
        public string PickupAddress { get; set; } = string.Empty; // Zorunlu alan
        public string DropoffAddress { get; set; } = string.Empty; // Zorunlu alan
        public DateTime TransferDate { get; set; }
        public TimeSpan? PickupTime { get; set; } // Pickup time (different from service start)
        public TimeSpan? ServiceStartTime { get; set; } // When actual transport service begins
        public DateTime? PickupConfirmationTime { get; set; } // When driver confirms pickup
        public DateTime? DropoffConfirmationTime { get; set; } // When driver confirms dropoff
        public decimal Price { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
        public bool IsFromAirport { get; set; }
        public TransferType TransferType { get; set; } = TransferType.Custom; // Transfer tipi

        // Communication fields
        public string? ContactPersonName { get; set; } // Who to contact for coordination (not pickup person)
        public string? MeetingPointDetails { get; set; } // Specific pickup instructions

        // Group management fields
        public int? GroupSize { get; set; } // Total number of people
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants

        // Communication fields
        public string? GuestLanguage { get; set; } // Communication language
        public string? EmergencyContactPhone { get; set; } // Emergency contact phone
        public string? PrimaryContactPhone { get; set; } // Main contact phone (concierge)
        public string? SecondaryContactPhone { get; set; } // Secondary contact phone (group transfers)

        public int GuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int? DriverId { get; set; } // Assigned driver (from Personnel)
        public int? AirportId { get; set; } // Zorunlu değil
        public int? VehicleId { get; set; } // Zorunlu değil
        public int? PickupCityId { get; set; } // Zorunlu değil
        public int? DropoffCityId { get; set; } // Zorunlu değil
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)

        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }
        public string? SupplierContactPhone { get; set; } // Primary supplier contact
        public string? SupplierEmergencyContact { get; set; } // Emergency supplier contact
        
        // Driver information
        public string? DriverName { get; set; } // Assigned driver name (from Personnel table)
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        public string? ExternalDriverPhone { get; set; } // External driver phone (legal requirement for non-company drivers)

        // Service quality fields
        public string? AccessibilityRequirements { get; set; } // Wheelchair, walking assistance, etc.
        public string? SpecialHandlingNotes { get; set; } // VIP handling, medical conditions, etc.

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only
        public string? GuestVisibleNotes { get; set; } // Information sent to guest

        /// <summary>
        /// SERVICE INFORMATION PDF: Non-financial info PDF URL
        /// Contains date, time, pickup, notes - NO prices
        /// </summary>
        public string? ServiceInfoPdfUrl { get; set; }

        // Priority and transport properties
        public TransferPriority Priority { get; set; } = TransferPriority.Normal; // Service priority level
        public TransportMode? TransportMode { get; set; } // Type of vehicle required
        public int? LuggageCount { get; set; } // Number of luggage pieces
        public int? ReturnTransferId { get; set; } // Linked return transfer
        public bool IsVip { get; set; } = false; // VIP service indicator

        // Relational Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity? Personnel { get; set; }
        public virtual AirportEntity? Airport { get; set; }
        public virtual VehicleEntity? Vehicle { get; set; }
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual CityEntity? PickupCity { get; set; }
        public virtual CityEntity? DropoffCity { get; set; }
        public virtual ICollection<PaymentEntity> Payments { get; set; } = new List<PaymentEntity>();
    }

    public class TransferConfiguration : BaseConfiguration<TransferEntity>
    {
        public override void Configure(EntityTypeBuilder<TransferEntity> builder)
        {
            base.Configure(builder);
            builder.Property(t => t.PickupAddress).HasMaxLength(500).IsRequired();
            builder.Property(t => t.DropoffAddress).HasMaxLength(500).IsRequired();
            builder.Property(t => t.Note).HasMaxLength(1000).IsRequired(false);
            builder.Property(t => t.Status).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.Price).HasPrecision(18, 2);
            builder.Property(t => t.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(t => t.FinalPrice).HasPrecision(18, 2);
            builder.Property(t => t.Currency).HasMaxLength(3).IsRequired(false);
            builder.Property(t => t.SupplierName).HasMaxLength(200).IsRequired(false);
            builder.Property(t => t.SupplierCurrency).HasMaxLength(3).IsRequired(false);
            builder.Property(t => t.SupplierInvoiceNumber).HasMaxLength(100).IsRequired(false);
            builder.Property(t => t.DriverName).HasMaxLength(200).IsRequired(false);
            builder.Property(t => t.ExternalVehiclePlate).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.ExternalDriverName).HasMaxLength(200).IsRequired(false);
            builder.Property(t => t.ExternalDriverPhone).HasMaxLength(20).IsRequired(false);

            // New fields constraints
            builder.Property(t => t.ContactPersonName).HasMaxLength(100).IsRequired(false);
            builder.Property(t => t.MeetingPointDetails).HasMaxLength(500).IsRequired(false);
            builder.Property(t => t.GuestLanguage).HasMaxLength(50).IsRequired(false);
            builder.Property(t => t.EmergencyContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.PrimaryContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.SecondaryContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.AccessibilityRequirements).HasMaxLength(500).IsRequired(false);
            builder.Property(t => t.SpecialHandlingNotes).HasMaxLength(1000).IsRequired(false);
            builder.Property(t => t.ConciergeInternalNotes).HasMaxLength(1000).IsRequired(false);
            builder.Property(t => t.GuestVisibleNotes).HasMaxLength(500).IsRequired(false);
            builder.Property(t => t.SupplierContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.SupplierEmergencyContact).HasMaxLength(20).IsRequired(false);
            builder.Property(t => t.TransferType).HasConversion<int>(); // Enum'u int olarak sakla

            // New properties constraints
            builder.Property(t => t.Priority).HasConversion<int>().IsRequired();
            builder.Property(t => t.TransportMode).HasConversion<int>().IsRequired(false);
            builder.Property(t => t.LuggageCount).IsRequired(false);

            // Performance indexes for common queries
            builder.HasIndex(t => t.TransferDate);
            builder.HasIndex(t => t.Status);
            builder.HasIndex(t => new { t.Status, t.TransferDate });
            builder.HasIndex(t => new { t.DriverId, t.TransferDate });
            builder.HasIndex(t => new { t.VehicleId, t.TransferDate });
            builder.HasIndex(t => t.GuestId);
            builder.HasIndex(t => new { t.TransferDate, t.Status, t.IsDeleted });
            builder.HasIndex(t => t.Priority);
            builder.HasIndex(t => t.IsVip);
            builder.HasIndex(t => t.TransportMode);
            builder.HasIndex(t => new { t.TransferDate, t.DriverId, t.Status });
            builder.HasIndex(t => new { t.TransferDate, t.VehicleId, t.Status });

        }
    }
}
