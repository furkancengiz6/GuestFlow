using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Guest entity with room-date context support.
    /// 
    /// ROOM NUMBER & ROOM-DATE CONTEXT REALITY (LOCKED PRODUCT DECISION):
    /// - RoomNumber is the CURRENT room (convenience field)
    /// - Room number can change during a stay
    /// - RoomHistory tracks all room assignments over time
    /// - When searching by room, the DATE must be considered
    /// - PMS is the source of truth for room assignments
    /// </summary>
    public class GuestEntity : BaseEntity, IGuest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty; // Benzersiz misafir kodu
        public bool IsSpecialGuest { get; set; } // Özel misafir mi?

        // Emergency contact information
        public string? EmergencyContactName { get; set; } // Who to call if guest unreachable
        public string? EmergencyContactPhone { get; set; } // Emergency contact phone
        public string? EmergencyContactRelation { get; set; } // Relationship to guest
        
        /// <summary>
        /// Current room number (convenience field - latest value from RoomHistory)
        /// For historical queries, use RoomHistory instead
        /// </summary>
        public string? RoomNumber { get; set; }
        
        public DateTime? CheckInDate { get; set; } // Otel giriş tarihi
        public DateTime? CheckOutDate { get; set; } // Otel çıkış tarihi
        public int? HotelId { get; set; } // Otel ID (misafir hangi otelde kalıyor)

        // Relational Properties
        /// <summary>
        /// Room assignment history - for time-based room tracking
        /// </summary>
        public virtual ICollection<RoomAssignmentEntity> RoomAssignments { get; set; } = new List<RoomAssignmentEntity>();
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>();
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<YachtTourEntity> YachtTours { get; set; } = new List<YachtTourEntity>();
        public virtual HotelEntity? Hotel { get; set; }
        
        /// <summary>
        /// Room history - tracks all room assignments over time.
        /// Use this for historical room queries instead of RoomNumber field.
        /// </summary>
        public virtual ICollection<GuestRoomHistoryEntity> RoomHistory { get; set; } = new List<GuestRoomHistoryEntity>();
    }

    public class GuestConfiguration : BaseConfiguration<GuestEntity>
    {
        public override void Configure(EntityTypeBuilder<GuestEntity> builder)
        {
            base.Configure(builder);
            builder.Property(g => g.FullName).HasMaxLength(200);
            builder.Property(g => g.Email).HasMaxLength(255);
            builder.Property(g => g.PhoneNumber).HasMaxLength(20);
            builder.Property(g => g.Nationality).HasMaxLength(100);
            builder.Property(g => g.GuestCode).HasMaxLength(50);
            builder.Property(g => g.RoomNumber).HasMaxLength(50).IsRequired(false);
            builder.HasIndex(g => g.GuestCode).IsUnique();

            // Emergency contact fields
            builder.Property(g => g.EmergencyContactName).HasMaxLength(100).IsRequired(false);
            builder.Property(g => g.EmergencyContactPhone).HasMaxLength(20).IsRequired(false);
            builder.Property(g => g.EmergencyContactRelation).HasMaxLength(50).IsRequired(false);

            builder.HasOne(g => g.Hotel)
                   .WithMany()
                   .HasForeignKey(g => g.HotelId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
