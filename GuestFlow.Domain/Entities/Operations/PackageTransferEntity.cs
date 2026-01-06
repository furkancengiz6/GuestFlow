using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Paket-Transfer ilişki entity'si
    /// </summary>
    public class PackageTransferEntity
    {
        public int PackageId { get; set; }
        public int TransferId { get; set; }

        // Relational Properties
        public virtual ServicePackageEntity Package { get; set; } = null!;
        public virtual TransferEntity Transfer { get; set; } = null!;
    }

    public class PackageTransferConfiguration : IEntityTypeConfiguration<PackageTransferEntity>
    {
        public void Configure(EntityTypeBuilder<PackageTransferEntity> builder)
        {
            builder.HasKey(pt => new { pt.PackageId, pt.TransferId });

            builder.HasOne(pt => pt.Package)
                   .WithMany(p => p.PackageTransfers)
                   .HasForeignKey(pt => pt.PackageId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pt => pt.Transfer)
                   .WithMany()
                   .HasForeignKey(pt => pt.TransferId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

