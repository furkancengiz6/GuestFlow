using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class LoginAttemptConfiguration : BaseConfiguration<LoginAttemptEntity>
    {
        public override void Configure(EntityTypeBuilder<LoginAttemptEntity> builder)
        {
            base.Configure(builder);

            builder.Property(l => l.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(l => l.IpAddress)
                .HasMaxLength(45); // IPv6 support

            builder.Property(l => l.FailureReason)
                .HasMaxLength(100);

            // Indexes for efficient queries
            builder.HasIndex(l => new { l.Email, l.AttemptDate });
            builder.HasIndex(l => new { l.IpAddress, l.AttemptDate });
            builder.HasIndex(l => l.AttemptDate);

            // Foreign key to Personnel (optional)
            builder.HasOne(l => l.Personnel)
                .WithMany()
                .HasForeignKey(l => l.PersonnelId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
