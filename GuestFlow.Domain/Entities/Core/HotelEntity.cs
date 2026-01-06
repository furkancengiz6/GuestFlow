using GuestFlow.Domain.Entities.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class HotelEntity : BaseEntity, IHotel
    {
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int CityId { get; set; }
        public int? StarRating { get; set; } // 1-5 yıldız
        public TimeSpan? CheckInTime { get; set; } // Varsayılan check-in saati
        public TimeSpan? CheckOutTime { get; set; } // Varsayılan check-out saati
        public string? RoomTypes { get; set; } // JSON formatında oda tipleri
        public string? Amenities { get; set; } // JSON formatında olanaklar
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;

        // Relational Properties
        public virtual CityEntity City { get; set; } = null!;
        public virtual ICollection<TransferEntity> PickupTransfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<TransferEntity> DropoffTransfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<CityTourEntity> PickupCityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<YachtTourEntity> PickupYachtTours { get; set; } = new List<YachtTourEntity>();
    }

    public class HotelConfiguration : BaseConfiguration<HotelEntity>
    {
        public override void Configure(EntityTypeBuilder<HotelEntity> builder)
        {
            base.Configure(builder);
            builder.Property(h => h.HotelName).HasMaxLength(200).IsRequired();
            builder.Property(h => h.Address).HasMaxLength(500).IsRequired();
            builder.Property(h => h.Phone).HasMaxLength(20);
            builder.Property(h => h.Email).HasMaxLength(255);
            builder.Property(h => h.StarRating).HasMaxLength(1);
            builder.Property(h => h.RoomTypes).HasMaxLength(1000);
            builder.Property(h => h.Amenities).HasMaxLength(1000);
            builder.Property(h => h.Notes).HasMaxLength(1000);

            builder.HasOne(h => h.City)
                   .WithMany()
                   .HasForeignKey(h => h.CityId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasIndex(h => h.HotelName);
            builder.HasIndex(h => h.CityId);
        }
    }
}

