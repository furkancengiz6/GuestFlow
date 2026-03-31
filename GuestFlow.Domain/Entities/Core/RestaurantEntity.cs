using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class RestaurantEntity : BaseEntity
    {
        public string RestaurantName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public int CityId { get; set; }
        public string? Description { get; set; }
        public string CuisineType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal AveragePricePerPerson { get; set; }

        public string? MenuUrl { get; set; }
        public string OperatingHours { get; set; } = string.Empty;
        public bool ReservationRequired { get; set; }
        public bool IsVip { get; set; }
        public bool IsActive { get; set; } = true;
        public double Rating { get; set; }

        // Koordinat bilgileri
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Relational Properties
        public virtual CityEntity City { get; set; } = null!;
        public virtual ICollection<RestaurantReservationEntity> Reservations { get; set; } = new List<RestaurantReservationEntity>();
        public virtual ICollection<TransferEntity> PickupTransfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<TransferEntity> DropoffTransfers { get; set; } = new List<TransferEntity>();
    }

    public class RestaurantConfiguration : BaseConfiguration<RestaurantEntity>
    {
        public override void Configure(EntityTypeBuilder<RestaurantEntity> builder)
        {
            base.Configure(builder);
            builder.Property(r => r.RestaurantName).HasMaxLength(200).IsRequired();
            builder.Property(r => r.Address).HasMaxLength(500).IsRequired();
            builder.Property(r => r.Phone).HasMaxLength(20).IsRequired();
            builder.Property(r => r.Email).HasMaxLength(100);
            builder.Property(r => r.CuisineType).HasMaxLength(100);
            builder.Property(r => r.AveragePricePerPerson).HasPrecision(18, 2);
            builder.Property(r => r.OperatingHours).HasMaxLength(200);

            builder.HasIndex(r => r.RestaurantName);
            builder.HasIndex(r => r.CuisineType);
            builder.HasIndex(r => r.IsVip);

            builder.HasOne(r => r.City)
                   .WithMany()
                   .HasForeignKey(r => r.CityId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(r => r.PickupTransfers)
                   .WithOne()
                   .HasForeignKey("PickupRestaurantId")
                   .IsRequired(false);

            builder.HasMany(r => r.DropoffTransfers)
                   .WithOne()
                   .HasForeignKey("DropoffRestaurantId")
                   .IsRequired(false);
        }
    }
}
