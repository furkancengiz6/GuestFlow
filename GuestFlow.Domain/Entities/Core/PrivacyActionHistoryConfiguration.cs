using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class PrivacyActionHistoryConfiguration : BaseConfiguration<PrivacyActionHistoryEntity>
    {
        public override void Configure(EntityTypeBuilder<PrivacyActionHistoryEntity> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ActionType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(p => p.Reason)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(p => new { p.GuestId, p.ActionDate });
            builder.HasIndex(p => p.ActionDate);

            // Foreign keys
            builder.HasOne(p => p.Guest)
                .WithMany()
                .HasForeignKey(p => p.GuestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.RequestedByPersonnel)
                .WithMany()
                .HasForeignKey(p => p.RequestedByPersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
