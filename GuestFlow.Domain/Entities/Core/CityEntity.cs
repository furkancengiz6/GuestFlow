
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class CityEntity : BaseEntity, ICity
    {
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public virtual ICollection<AirportEntity> Airports { get; set; } = new List<AirportEntity>();
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<TransferEntity> PickupTransfers { get; set; } = new List<TransferEntity>(); // PickupCityId için
        public virtual ICollection<TransferEntity> DropoffTransfers { get; set; } = new List<TransferEntity>(); // DropoffCityId için
        public virtual ICollection<TourEntity> Tours { get; set; } = new List<TourEntity>(); // TourEntity için
    }

    public class CityConfiguration : BaseConfiguration<CityEntity>
    {
        public override void Configure(EntityTypeBuilder<CityEntity> builder)
        {
            base.Configure(builder);
            builder.Property(c => c.CityName).HasMaxLength(100);
            builder.Property(c => c.Country).HasMaxLength(100);

            builder.HasMany(c => c.PickupTransfers)
                   .WithOne(t => t.PickupCity)
                   .HasForeignKey(t => t.PickupCityId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.DropoffTransfers)
                   .WithOne(t => t.DropoffCity)
                   .HasForeignKey(t => t.DropoffCityId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
