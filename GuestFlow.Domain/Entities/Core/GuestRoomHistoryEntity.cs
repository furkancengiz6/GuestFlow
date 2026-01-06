using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Tracks guest room assignments over time.
    /// 
    /// ROOM NUMBER & ROOM-DATE CONTEXT REALITY (LOCKED PRODUCT DECISION):
    /// - Room number is NOT a static guest field
    /// - Room number can change during a stay (upgrades, maintenance)
    /// - Multiple guests may use the same room sequentially
    /// - When searching by room, the DATE must be considered
    /// - Historical room associations must be preserved
    /// - PMS is the source of truth for room assignments
    /// </summary>
    public class GuestRoomHistoryEntity : BaseEntity
    {
        /// <summary>
        /// Guest ID
        /// </summary>
        public int GuestId { get; set; }
        
        /// <summary>
        /// Room number at this point in time
        /// </summary>
        public string RoomNumber { get; set; } = string.Empty;
        
        /// <summary>
        /// When this room assignment started
        /// </summary>
        public DateTime AssignedDate { get; set; }
        
        /// <summary>
        /// When this room assignment ended (null if current)
        /// </summary>
        public DateTime? EndDate { get; set; }
        
        /// <summary>
        /// Is this the current room assignment?
        /// </summary>
        public bool IsCurrent { get; set; } = true;
        
        /// <summary>
        /// Source of this room assignment (Manual, PMS, Reception, etc.)
        /// </summary>
        public string Source { get; set; } = "Manual";
        
        /// <summary>
        /// Notes about this room change
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Personnel who made this change
        /// </summary>
        public int? AssignedByPersonnelId { get; set; }
        
        // Navigation Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity? AssignedByPersonnel { get; set; }
    }

    public class GuestRoomHistoryConfiguration : BaseConfiguration<GuestRoomHistoryEntity>
    {
        public override void Configure(EntityTypeBuilder<GuestRoomHistoryEntity> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.RoomNumber).HasMaxLength(50).IsRequired();
            builder.Property(x => x.Source).HasMaxLength(50);
            builder.Property(x => x.Notes).HasMaxLength(500);
            
            // Index for room + date range queries
            builder.HasIndex(x => new { x.RoomNumber, x.AssignedDate, x.EndDate });
            builder.HasIndex(x => new { x.GuestId, x.IsCurrent });
            
            // Relationships
            builder.HasOne(x => x.Guest)
                .WithMany(g => g.RoomHistory)
                .HasForeignKey(x => x.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(x => x.AssignedByPersonnel)
                .WithMany()
                .HasForeignKey(x => x.AssignedByPersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

