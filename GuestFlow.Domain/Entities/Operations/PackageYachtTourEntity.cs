using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Paket-Yat Turu ilişki entity'si
    /// </summary>
    public class PackageYachtTourEntity
    {
        public int PackageId { get; set; }
        public int YachtTourId { get; set; }

        // Relational Properties
        public virtual ServicePackageEntity Package { get; set; } = null!;
        public virtual YachtTourEntity YachtTour { get; set; } = null!;
    }

    public class PackageYachtTourConfiguration : IEntityTypeConfiguration<PackageYachtTourEntity>
    {
        public void Configure(EntityTypeBuilder<PackageYachtTourEntity> builder)
        {
            builder.HasKey(pyt => new { pyt.PackageId, pyt.YachtTourId });

            builder.HasOne(pyt => pyt.Package)
                   .WithMany(p => p.PackageYachtTours)
                   .HasForeignKey(pyt => pyt.PackageId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pyt => pyt.YachtTour)
                   .WithMany()
                   .HasForeignKey(pyt => pyt.YachtTourId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

