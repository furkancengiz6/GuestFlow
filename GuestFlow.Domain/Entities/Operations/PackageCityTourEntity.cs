using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Paket-Şehir Turu ilişki entity'si
    /// </summary>
    public class PackageCityTourEntity
    {
        public int PackageId { get; set; }
        public int CityTourId { get; set; }

        // Relational Properties
        public virtual ServicePackageEntity Package { get; set; } = null!;
        public virtual CityTourEntity CityTour { get; set; } = null!;
    }

    public class PackageCityTourConfiguration : IEntityTypeConfiguration<PackageCityTourEntity>
    {
        public void Configure(EntityTypeBuilder<PackageCityTourEntity> builder)
        {
            builder.HasKey(pct => new { pct.PackageId, pct.CityTourId });

            builder.HasOne(pct => pct.Package)
                   .WithMany(p => p.PackageCityTours)
                   .HasForeignKey(pct => pct.PackageId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pct => pct.CityTour)
                   .WithMany()
                   .HasForeignKey(pct => pct.CityTourId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

