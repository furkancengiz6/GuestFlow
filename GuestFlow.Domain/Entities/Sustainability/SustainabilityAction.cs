using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Sustainability
{
    /// <summary>
    /// Tracks a specific sustainable action taken by a guest.
    /// </summary>
    public class SustainabilityAction : BaseEntity
    {
        public int GuestId { get; set; }
        public SustainabilityActionType ActionType { get; set; }
        public DateTime ActionDate { get; set; }
        public string? Description { get; set; }
        public int ImpactScore { get; set; } // Points awarded for this action
        
        public virtual GuestEntity? Guest { get; set; }
    }

    public class SustainabilityActionConfiguration : BaseConfiguration<SustainabilityAction>
    {
        public override void Configure(EntityTypeBuilder<SustainabilityAction> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.ActionDate).IsRequired();
            builder.Property(x => x.ImpactScore).IsRequired();
            
            builder.HasOne(x => x.Guest)
                   .WithMany() // Will update GuestEntity later
                   .HasForeignKey(x => x.GuestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
