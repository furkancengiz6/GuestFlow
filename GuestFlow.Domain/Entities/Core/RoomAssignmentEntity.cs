using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Room Assignment Entity - Time-based room tracking
    ///
    /// ROOM NUMBER & ROOM-DATE CONTEXT REALITY (LOCKED PRODUCT DECISION):
    /// - Room number is NOT a static guest field
    /// - Room number is meaningful ONLY with a date range
    /// - Guests may change rooms during a single stay
    /// - Room changes create NEW time-based contexts
    /// - Historical room associations must remain visible
    /// - PMS remains source of truth; GuestFlow stores operational context history
    /// </summary>
    public class RoomAssignmentEntity : BaseEntity
    {
        public int GuestId { get; set; }

        /// <summary>
        /// Hotel where the room is located (nullable for flexibility)
        /// </summary>
        public int? HotelId { get; set; }

        /// <summary>
        /// Room number during this assignment period
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;

        /// <summary>
        /// Start date of this room assignment (inclusive)
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of this room assignment (inclusive, null if current)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Source of this room assignment (Manual, PMS, Reception)
        /// </summary>
        public string Source { get; set; } = "Manual";

        /// <summary>
        /// Notes about this room assignment
        /// </summary>
        public string? Notes { get; set; }

        // Relational Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual HotelEntity? Hotel { get; set; }
    }
    
    public class RoomAssignmentConfiguration : BaseConfiguration<RoomAssignmentEntity>
    {
        public override void Configure(EntityTypeBuilder<RoomAssignmentEntity> builder)
        {
            base.Configure(builder);

            builder.Property(r => r.RoomNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(r => r.Source)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Notes)
                .HasMaxLength(500)
                .IsRequired(false);

            // Index for efficient Room+Date queries
            builder.HasIndex(r => new { r.RoomNumber, r.StartDate });
            builder.HasIndex(r => new { r.GuestId, r.StartDate });
            builder.HasIndex(r => new { r.HotelId, r.RoomNumber, r.StartDate });

            // Index for preventing overlapping assignments (checked in business logic)
            builder.HasIndex(r => new { r.GuestId, r.StartDate, r.EndDate });

            // Foreign key relationships
            builder.HasOne(r => r.Guest)
                .WithMany(g => g.RoomAssignments)
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Hotel)
                .WithMany()
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

