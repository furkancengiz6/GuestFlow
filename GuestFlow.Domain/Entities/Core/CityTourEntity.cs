using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class CityTourEntity : BaseEntity, ICityTour
    {
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)


        // Relational Properties
        public virtual GuestEntity OwnerGuest { get; set; } = null!; 
        public virtual PersonnelEntity Personnel { get; set; } = null!;
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();

        public virtual CityEntity City { get; set; } = null!;
    }

    public class CityTourConfiguration : BaseConfiguration<CityTourEntity>
    {
        public override void Configure(EntityTypeBuilder<CityTourEntity> builder)
        {
            base.Configure(builder);
            builder.Property(ct => ct.Language).HasMaxLength(50);
            builder.Property(ct => ct.Price).HasPrecision(18, 2);
            builder.Property(ct => ct.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(ct => ct.FinalPrice).HasPrecision(18, 2);
            builder.Property(ct => ct.Currency).HasMaxLength(3);

            builder.HasOne(ct => ct.OwnerGuest)
               .WithMany(g => g.CityTours) 
               .HasForeignKey(ct => ct.OwnerGuestId)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
