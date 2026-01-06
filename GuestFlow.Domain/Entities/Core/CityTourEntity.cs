using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class CityTourEntity : BaseEntity, ICityTour
    {
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; } // Total tour duration in hours
        public decimal Price { get; set; }

        // Group composition fields
        public int? AdultCount { get; set; } // Number of adults
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants

        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int? TourGuideId { get; set; } // Main tour guide (from Personnel)
        public int? AssistantGuideId { get; set; } // Assistant guide (from Personnel)
        public int CityId { get; set; }
        public int? TourId { get; set; } // Admin tarafından eklenen tur (Kapadokya Turu, Pamukkale Turu vb.) - nullable
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati
        public TimeSpan? PickupTime { get; set; } // Hotel pickup time
        public DateTime? TourConfirmationTime { get; set; } // When tour starts
        
        // Safety & emergency fields
        public string? GroupLeaderName { get; set; } // Responsible person for the group
        public string? GroupLeaderPhone { get; set; } // Group leader contact phone
        public string? EmergencyContactName { get; set; } // Secondary emergency contact
        public string? EmergencyContactPhone { get; set; } // Secondary emergency phone
        public string? EmergencyContactRelation { get; set; } // Relationship to group leader

        // Coordination fields
        public string? MeetingPersonName { get; set; } // Who to meet at pickup point
        public string? MeetingPointDetails { get; set; } // Detailed pickup instructions

        // Otel bilgileri
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        public string? PickupLocation { get; set; } // Alış noktası (otel dışında bir yer)
        public string? DropoffLocation { get; set; } // Bırakış noktası
        
        // Şoför ve araç bilgileri
        public int? VehicleId { get; set; } // Assigned vehicle for transportation
        public string? DriverName { get; set; } // Şoför isim soyisim
        
        // Rehber bilgileri
        public string? GuideName { get; set; } // Rehber isim
        public string? GuidePhone { get; set; } // Rehber telefon numarası
        public string? GuideLanguages { get; set; } // Languages guide speaks (comma-separated)
        public string? BackupGuideName { get; set; } // Backup guide if primary unavailable
        public string? BackupGuidePhone { get; set; } // Backup guide contact
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; } // Dışarıdan çekilen araç plakası
        public string? ExternalDriverName { get; set; } // Dışarıdan çekilen şoför isim soyisim
        public string? ExternalDriverPhone { get; set; } // Dışarıdan çekilen şoför telefon numarası

        // Operational details
        public string? TourDifficultyLevel { get; set; } // Easy/Moderate/Challenging with specific requirements
        public bool? WeatherDependent { get; set; } // Can tour run in bad weather?
        public int? MinimumParticipantCount { get; set; } // Minimum participants required
        public int? MaximumParticipantCount { get; set; } // Maximum participants allowed

        // Guest experience fields
        public string? DietaryRequirements { get; set; } // Food allergies, preferences
        public string? AccessibilityNeeds { get; set; } // Mobility assistance, etc.
        public bool? PhotographyAllowed { get; set; } // Commercial photography rights (not personal photos)
        public string? SpecialEquipment { get; set; } // Wheelchair, mobility aids, etc.



        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Status and control fields
        public string Status { get; set; } = "Pending"; // Tour status
        public bool IsVipGroup { get; set; } = false; // VIP group flag
        public bool IsDeleted { get; set; } = false; // Soft delete flag

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only

        /// <summary>
        /// SERVICE INFORMATION PDF: Non-financial info PDF URL
        /// Contains date, time, location, notes - NO prices
        /// </summary>
        public string? ServiceInfoPdfUrl { get; set; }

        // Relational Properties
        public virtual GuestEntity OwnerGuest { get; set; } = null!; 
        public virtual PersonnelEntity? Personnel { get; set; }
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual CityEntity City { get; set; } = null!;
        public virtual TourEntity? Tour { get; set; } // Admin tarafından eklenen tur - nullable
        public virtual VehicleEntity? Vehicle { get; set; }
        public virtual HotelEntity? PickupHotel { get; set; }
    }

    public class CityTourConfiguration : BaseConfiguration<CityTourEntity>
    {
        public override void Configure(EntityTypeBuilder<CityTourEntity> builder)
        {
            base.Configure(builder);
            builder.Property(ct => ct.Language).HasMaxLength(50);
            builder.Property(ct => ct.Price).HasPrecision(18, 2);
            builder.Property(ct => ct.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(ct => ct.FinalPrice).HasPrecision(18, 2);
            builder.Property(ct => ct.Currency).HasMaxLength(3);
            builder.Property(ct => ct.DriverName).HasMaxLength(200).IsRequired(false);
            builder.Property(ct => ct.GuideName).HasMaxLength(200).IsRequired(false);
            builder.Property(ct => ct.GuidePhone).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.ExternalVehiclePlate).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.ExternalDriverName).HasMaxLength(200).IsRequired(false);
            builder.Property(ct => ct.ExternalDriverPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.PickupLocation).HasMaxLength(500).IsRequired(false);
            builder.Property(ct => ct.DropoffLocation).HasMaxLength(500).IsRequired(false);
            builder.Property(ct => ct.SupplierName).HasMaxLength(200).IsRequired(false);
            builder.Property(ct => ct.SupplierCurrency).HasMaxLength(3).IsRequired(false);
            builder.Property(ct => ct.SupplierInvoiceNumber).HasMaxLength(100).IsRequired(false);

            // New fields constraints
            builder.Property(ct => ct.GroupLeaderName).HasMaxLength(100).IsRequired(false);
            builder.Property(ct => ct.GroupLeaderPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.EmergencyContactName).HasMaxLength(100).IsRequired(false);
            builder.Property(ct => ct.EmergencyContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.EmergencyContactRelation).HasMaxLength(50).IsRequired(false);
            builder.Property(ct => ct.MeetingPersonName).HasMaxLength(100).IsRequired(false);
            builder.Property(ct => ct.MeetingPointDetails).HasMaxLength(500).IsRequired(false);
            builder.Property(ct => ct.GuideLanguages).HasMaxLength(200).IsRequired(false);
            builder.Property(ct => ct.BackupGuideName).HasMaxLength(100).IsRequired(false);
            builder.Property(ct => ct.BackupGuidePhone).HasMaxLength(20).IsRequired(false);
            builder.Property(ct => ct.TourDifficultyLevel).HasMaxLength(50).IsRequired(false);
            builder.Property(ct => ct.DietaryRequirements).HasMaxLength(500).IsRequired(false);
            builder.Property(ct => ct.AccessibilityNeeds).HasMaxLength(500).IsRequired(false);
            builder.Property(ct => ct.ConciergeInternalNotes).HasMaxLength(1000).IsRequired(false);
            builder.Property(ct => ct.SpecialEquipment).HasMaxLength(500).IsRequired(false);

            builder.HasOne(ct => ct.PickupHotel)
                   .WithMany(h => h.PickupCityTours)
                   .HasForeignKey(ct => ct.PickupHotelId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(ct => ct.OwnerGuest)
               .WithMany(g => g.CityTours) 
               .HasForeignKey(ct => ct.OwnerGuestId)
               .OnDelete(DeleteBehavior.NoAction);
            
            builder.HasOne(ct => ct.Tour)
                .WithMany(t => t.CityTours)
                .HasForeignKey(ct => ct.TourId)
                .OnDelete(DeleteBehavior.Restrict);

            // Performance indexes for common queries
            builder.HasIndex(ct => ct.TourDate);
            builder.HasIndex(ct => ct.Status);
            builder.HasIndex(ct => new { ct.Status, ct.TourDate });
            builder.HasIndex(ct => new { ct.TourGuideId, ct.TourDate });
            builder.HasIndex(ct => new { ct.VehicleId, ct.TourDate });
            builder.HasIndex(ct => ct.OwnerGuestId);
            builder.HasIndex(ct => new { ct.TourDate, ct.Status, ct.IsDeleted });
            builder.HasIndex(ct => ct.IsVipGroup);
        }
    }
}
