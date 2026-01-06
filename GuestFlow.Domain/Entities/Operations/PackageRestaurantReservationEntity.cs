using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// Paket-Restoran Rezervasyonu ilişki entity'si
    /// </summary>
    public class PackageRestaurantReservationEntity
    {
        public int PackageId { get; set; }
        public int RestaurantReservationId { get; set; }

        // Relational Properties
        public virtual ServicePackageEntity Package { get; set; } = null!;
        public virtual RestaurantReservationEntity RestaurantReservation { get; set; } = null!;
    }

    public class PackageRestaurantReservationConfiguration : IEntityTypeConfiguration<PackageRestaurantReservationEntity>
    {
        public void Configure(EntityTypeBuilder<PackageRestaurantReservationEntity> builder)
        {
            builder.HasKey(prr => new { prr.PackageId, prr.RestaurantReservationId });

            builder.HasOne(prr => prr.Package)
                   .WithMany(p => p.PackageRestaurantReservations)
                   .HasForeignKey(prr => prr.PackageId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(prr => prr.RestaurantReservation)
                   .WithMany()
                   .HasForeignKey(prr => prr.RestaurantReservationId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

