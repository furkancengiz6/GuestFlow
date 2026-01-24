using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class FeatureFlagConfiguration : BaseConfiguration<FeatureFlagEntity>
    {
        public override void Configure(EntityTypeBuilder<FeatureFlagEntity> builder)
        {
            base.Configure(builder);

            builder.Property(f => f.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(f => f.Description)
                .HasMaxLength(500);

            builder.Property(f => f.Environment)
                .HasMaxLength(50)
                .IsRequired()
                .HasDefaultValue("Production");

            builder.Property(f => f.RolloutPercentage)
                .HasPrecision(5, 2)
                .IsRequired();

            builder.Property(f => f.TargetRoles)
                .HasMaxLength(200);

            builder.Property(f => f.TargetUserIds)
                .HasMaxLength(1000);

            builder.Property(f => f.EnabledBy)
                .HasMaxLength(200);

            builder.Property(f => f.Notes)
                .HasMaxLength(1000);

            // Unique constraint on Name + Environment
            builder.HasIndex(f => new { f.Name, f.Environment })
                .IsUnique();
        }
    }
}
