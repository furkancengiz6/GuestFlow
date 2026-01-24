using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class PermissionConfiguration : BaseConfiguration<PermissionEntity>
    {
        public override void Configure(EntityTypeBuilder<PermissionEntity> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.Code)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(p => p.Description)
                .HasMaxLength(500);

            builder.Property(p => p.Category)
                .HasMaxLength(50)
                .IsRequired();

            // Unique constraint on Code
            builder.HasIndex(p => p.Code)
                .IsUnique();
        }
    }

    public class RolePermissionConfiguration : BaseConfiguration<RolePermissionEntity>
    {
        public override void Configure(EntityTypeBuilder<RolePermissionEntity> builder)
        {
            base.Configure(builder);

            builder.Property(rp => rp.RoleName)
                .HasMaxLength(50)
                .IsRequired();

            // Unique constraint on RoleName + PermissionId
            builder.HasIndex(rp => new { rp.RoleName, rp.PermissionId })
                .IsUnique();

            // Foreign key to Permission
            builder.HasOne(rp => rp.Permission)
                .WithMany()
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
