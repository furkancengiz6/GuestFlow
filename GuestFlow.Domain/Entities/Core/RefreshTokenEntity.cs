using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class RefreshTokenEntity : BaseEntity
    {
        public string Token { get; set; } = string.Empty;
        public int PersonnelId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? CreatedByIp { get; set; }

        // Relational Properties
        public virtual PersonnelEntity Personnel { get; set; } = null!;
    }

    public class RefreshTokenConfiguration : BaseConfiguration<RefreshTokenEntity>
    {
        public override void Configure(EntityTypeBuilder<RefreshTokenEntity> builder)
        {
            base.Configure(builder);
            builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
            builder.Property(rt => rt.PersonnelId).IsRequired();
            builder.Property(rt => rt.ExpiresAt).IsRequired();
            builder.Property(rt => rt.IsRevoked).IsRequired();
            builder.Property(rt => rt.RevokedByIp).HasMaxLength(50);
            builder.Property(rt => rt.CreatedByIp).HasMaxLength(50);

            builder.HasIndex(rt => rt.Token).IsUnique();
            builder.HasIndex(rt => rt.PersonnelId);
            builder.HasIndex(rt => new { rt.PersonnelId, rt.IsRevoked });

            builder.HasOne(rt => rt.Personnel)
                .WithMany()
                .HasForeignKey(rt => rt.PersonnelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

