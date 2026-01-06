using GuestFlow.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class RestaurantEntity : BaseEntity, IRestaurant
    {
        public string RestaurantName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int CityId { get; set; }
        public string? CuisineType { get; set; } // Mutfak tipi (Türk, İtalyan, vb.)
        public int? Capacity { get; set; } // Kapasite
        public string? OperatingHours { get; set; } // JSON formatında çalışma saatleri
        public bool ReservationRequired { get; set; } // Rezervasyon gerekli mi?
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        // Relational Properties
        public virtual CityEntity City { get; set; } = null!;
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
            builder.Property(r => r.Phone).HasMaxLength(20);
            builder.Property(r => r.Email).HasMaxLength(255);
            builder.Property(r => r.CuisineType).HasMaxLength(100);
            builder.Property(r => r.OperatingHours).HasMaxLength(500);
            builder.Property(r => r.Notes).HasMaxLength(1000);

            builder.HasOne(r => r.City)
                   .WithMany()
                   .HasForeignKey(r => r.CityId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasIndex(r => r.RestaurantName);
            builder.HasIndex(r => r.CityId);
        }
    }
}

