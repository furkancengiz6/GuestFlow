using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class YachtTourEntity : BaseEntity, IYachtTour
    {
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public string SpecialRequest { get; set; } = string.Empty;
        public string YachtName { get; set; } 
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)



        // Relational Properties
        public virtual GuestEntity OwnerGuest { get; set; } = null!; // Guest yerine OwnerGuest
        public virtual PersonnelEntity Personnel { get; set; } = null!;
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual CityEntity City { get; set; } = null!;
    }

    public class YachtTourConfiguration : BaseConfiguration<YachtTourEntity>
    {
        public override void Configure(EntityTypeBuilder<YachtTourEntity> builder)
        {
            base.Configure(builder);
            builder.Property(yt => yt.SpecialRequest).HasMaxLength(1000);
            builder.Property(yt => yt.YachtName).HasMaxLength(100);
            builder.Property(yt => yt.Price).HasPrecision(18, 2);
            builder.Property(yt => yt.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(yt => yt.FinalPrice).HasPrecision(18, 2);
            builder.Property(yt => yt.Currency).HasMaxLength(3);
            builder.HasOne(yt => yt.OwnerGuest)
               .WithMany(g => g.YachtTours) 
               .HasForeignKey(yt => yt.OwnerGuestId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
