using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Sustainability
{
    /// <summary>
    /// Rewards available to or earned by guests for high sustainability scores.
    /// </summary>
    public class SustainabilityReward : BaseEntity
    {
        public int GuestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RequiredScore { get; set; }
        public bool IsClaimed { get; set; }
        public DateTime? ClaimedDate { get; set; }
        public string? RewardCode { get; set; } // e.g., Coupon code
        
        public virtual GuestEntity? Guest { get; set; }
    }

    public class SustainabilityRewardConfiguration : BaseConfiguration<SustainabilityReward>
    {
        public override void Configure(EntityTypeBuilder<SustainabilityReward> builder)
        {
            base.Configure(builder);
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.RewardCode).HasMaxLength(50);
            
            builder.HasOne(x => x.Guest)
                   .WithMany() // Will update GuestEntity later
                   .HasForeignKey(x => x.GuestId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
