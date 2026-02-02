using GuestFlow.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Persistence.Configurations
{
    public class PricingRuleConfiguration : IEntityTypeConfiguration<PricingRuleEntity>
    {
        public void Configure(EntityTypeBuilder<PricingRuleEntity> builder)
        {
            builder.ToTable("PricingRules");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.RuleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Description)
                .HasMaxLength(500);

            builder.Property(e => e.RuleType)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(e => e.ConditionValue)
                .HasPrecision(18, 2);

            builder.Property(e => e.AdjustmentType)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(e => e.AdjustmentValue)
                .HasPrecision(18, 2);

            builder.HasIndex(e => e.IsActive);
            builder.HasIndex(e => e.Priority);
        }
    }
}
