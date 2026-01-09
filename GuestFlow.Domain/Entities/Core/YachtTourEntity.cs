using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class YachtTourEntity : BaseEntity, IYachtTour
    {
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public int? ChildCount { get; set; } // Number of children
        public int? InfantCount { get; set; } // Number of infants
        public decimal Price { get; set; }
        public string? SpecialRequest { get; set; }
        public string? YachtName { get; set; } // Yacht model/type name (not specific vessel)

        // Group coordination fields
        public string? GroupLeaderName { get; set; } // Responsible person for the group
        public string? GroupLeaderPhone { get; set; } // Group leader contact phone
        public string? EmergencyContactName { get; set; } // Secondary emergency contact
        public string? EmergencyContactPhone { get; set; } // Secondary emergency phone
        public string? EmergencyContactRelation { get; set; } // Relationship to group leader

        public int OwnerGuestId { get; set; }
        public int? PersonnelId { get; set; } // Otomatik doldurulacak, nullable
        public int? YachtId { get; set; } // Specific yacht (from Yacht inventory)
        public int? CaptainId { get; set; } // Licensed captain (from Personnel)
        public int CityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)
        public TourCategory? TourCategory { get; set; } // Daily, Sunset

        // Safety & regulatory fields
        public bool? LifeJacketsProvided { get; set; } // Legal requirement
        public int? LifeJacketCount { get; set; } // Match to group size
        public bool? SafetyEquipmentCheck { get; set; } // Captain confirmed all equipment
        public string? EmergencyEquipment { get; set; } // First aid, VHF radio, etc.

        // Capacity & compliance fields
        public int? YachtCapacity { get; set; } // Legal maximum passengers
        public string? YachtType { get; set; } // Motor Yacht, Sailing Yacht, Catamaran
        public bool? YachtLicenceRequired { get; set; } // Special license needed?
        public bool? CoastGuardApproved { get; set; } // Regulatory compliance

        // İskele bilgileri
        public string? PickupPier { get; set; } // Alış iskelesi
        public string? DropoffPier { get; set; } // Bırakış iskelesi
        public string? PierAddress { get; set; } // Serbest metin
        
        // Otel bilgileri
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; } // Başlangıç saati
        public TimeSpan? EndTime { get; set; } // Bitiş saati
        public DateTime? SafetyBriefingTime { get; set; } // When safety briefing occurs
        public TimeSpan? MarinaPickupTime { get; set; } // Marina pickup time
        public DateTime? WeatherCheckTime { get; set; } // Last weather check time

        // Operational details
        public int? CrewSize { get; set; } // Total crew count (captain, crew, hostess, security)
        public string? CaptainExperience { get; set; } // Captain certifications and experience level
        public int? FuelRange { get; set; } // How far yacht can safely travel
        public string? WeatherBackupPlan { get; set; } // What happens if weather deteriorates
        public DateTime? FuelLevelCheck { get; set; } // When fuel was last checked

        // Kaptan bilgisi
        public string? CaptainPhone { get; set; }

        // Guest safety fields
        public string? SwimmingProficiency { get; set; } // All can swim, supervision needed
        public string? MedicalConditions { get; set; } // Motion sickness, heart conditions
        public string? AlcoholPolicy { get; set; } // Alcohol service policy and restrictions

        // Amenities & experience fields
        public bool? FoodBeverageIncluded { get; set; } // Guest expectation management
        public string? BeverageType { get; set; } // Available beverages (non-alcoholic and alcoholic)
        public bool? MusicSystem { get; set; } // Sound system available
        public string? WaterSportsEquipment { get; set; } // Snorkels, paddleboards, etc.
        public bool? LifeGuardCertified { get; set; } // Certified lifeguard on board
        public DateTime? CoastGuardInspectionDate { get; set; } // Last Coast Guard inspection

        // Coordination fields
        public string? MarinaContactName { get; set; } // Who to contact at marina
        public string? MarinaContactPhone { get; set; } // Direct marina contact


        // Supplier cost information (for internal tracking only)
        public string? SupplierName { get; set; }
        public decimal? SupplierCost { get; set; }
        public string? SupplierCurrency { get; set; }
        public string? SupplierInvoiceNumber { get; set; }

        // Status and control fields
        public string Status { get; set; } = "Pending"; // Tour status
        public bool IsDeleted { get; set; } = false; // Soft delete flag
        public bool WeatherDependent { get; set; } = false; // Can tour run in bad weather?

        // Internal coordination fields
        public string? ConciergeInternalNotes { get; set; } // For concierge staff only

        /// <summary>
        /// SERVICE INFORMATION PDF: Non-financial info PDF URL
        /// Contains date, time, location, notes - NO prices
        /// </summary>
        public string? ServiceInfoPdfUrl { get; set; }

        // Relational Properties
        public virtual GuestEntity OwnerGuest { get; set; } = null!; // Guest yerine OwnerGuest
        public virtual PersonnelEntity? Personnel { get; set; }
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual CityEntity City { get; set; } = null!;
        public virtual HotelEntity? PickupHotel { get; set; }
    }

    public class YachtTourConfiguration : BaseConfiguration<YachtTourEntity>
    {
        public override void Configure(EntityTypeBuilder<YachtTourEntity> builder)
        {
            base.Configure(builder);
            builder.Property(yt => yt.SpecialRequest).HasMaxLength(1000).IsRequired(false);
            builder.Property(yt => yt.YachtName).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.Price).HasPrecision(18, 2);
            builder.Property(yt => yt.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(yt => yt.FinalPrice).HasPrecision(18, 2);
            builder.Property(yt => yt.Currency).HasMaxLength(3).IsRequired(false);
            builder.Property(yt => yt.PickupPier).HasMaxLength(200).IsRequired(false);
            builder.Property(yt => yt.DropoffPier).HasMaxLength(200).IsRequired(false);
            builder.Property(yt => yt.PierAddress).HasMaxLength(500).IsRequired(false);
            builder.Property(yt => yt.CaptainPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(yt => yt.SupplierName).HasMaxLength(200).IsRequired(false);
            builder.Property(yt => yt.SupplierCost).HasPrecision(18,2).IsRequired(false);
            builder.Property(yt => yt.SupplierCurrency).HasMaxLength(3).IsRequired(false);
            builder.Property(yt => yt.SupplierInvoiceNumber).HasMaxLength(100).IsRequired(false);

            // New fields constraints
            builder.Property(yt => yt.GroupLeaderName).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.GroupLeaderPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(yt => yt.EmergencyContactName).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.EmergencyContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(yt => yt.EmergencyContactRelation).HasMaxLength(50).IsRequired(false);
            builder.Property(yt => yt.YachtType).HasMaxLength(50).IsRequired(false);
            builder.Property(yt => yt.EmergencyEquipment).HasMaxLength(500).IsRequired(false);
            builder.Property(yt => yt.CaptainExperience).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.WeatherBackupPlan).HasMaxLength(500).IsRequired(false);
            builder.Property(yt => yt.SwimmingProficiency).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.MedicalConditions).HasMaxLength(500).IsRequired(false);
            builder.Property(yt => yt.AlcoholPolicy).HasMaxLength(200).IsRequired(false);
            builder.Property(yt => yt.BeverageType).HasMaxLength(200).IsRequired(false);
            builder.Property(yt => yt.WaterSportsEquipment).HasMaxLength(300).IsRequired(false);
            builder.Property(yt => yt.MarinaContactName).HasMaxLength(100).IsRequired(false);
            builder.Property(yt => yt.MarinaContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(yt => yt.ConciergeInternalNotes).HasMaxLength(1000).IsRequired(false);
            
            builder.HasOne(yt => yt.PickupHotel)
                   .WithMany(h => h.PickupYachtTours)
                   .HasForeignKey(yt => yt.PickupHotelId)
                   .OnDelete(DeleteBehavior.SetNull);
                   
            builder.HasOne(yt => yt.OwnerGuest)
               .WithMany(g => g.YachtTours)
               .HasForeignKey(yt => yt.OwnerGuestId)
               .OnDelete(DeleteBehavior.NoAction);

            // Performance indexes for common queries
            builder.HasIndex(yt => yt.TourDate);
            builder.HasIndex(yt => yt.Status);
            builder.HasIndex(yt => new { yt.Status, yt.TourDate });
            builder.HasIndex(yt => new { yt.CaptainId, yt.TourDate });
            builder.HasIndex(yt => yt.OwnerGuestId);
            builder.HasIndex(yt => new { yt.TourDate, yt.Status, yt.IsDeleted });
            builder.HasIndex(yt => yt.WeatherDependent);
        }
    }
}
