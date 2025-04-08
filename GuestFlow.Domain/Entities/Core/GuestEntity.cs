using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class GuestEntity : BaseEntity, IGuest
    {
        public string FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; }
        public string GuestCode { get; set; } // Benzersiz misafir kodu
        public bool IsSpecialGuest { get; set; } // Özel misafir mi?

        // Relational Properties
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual ICollection<GuestYachtTour> GuestYachtTours { get; set; } = new List<GuestYachtTour>();
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<CityTourEntity> CityTours { get; set; } = new List<CityTourEntity>();
        public virtual ICollection<YachtTourEntity> YachtTours { get; set; } = new List<YachtTourEntity>();
    }

    public class GuestConfiguration : BaseConfiguration<GuestEntity>
    {
        public override void Configure(EntityTypeBuilder<GuestEntity> builder)
        {
            base.Configure(builder);
            builder.Property(g => g.FullName).HasMaxLength(200);
            builder.Property(g => g.Email).HasMaxLength(255);
            builder.Property(g => g.PhoneNumber).HasMaxLength(20);
            builder.Property(g => g.Nationality).HasMaxLength(100);
            builder.Property(g => g.GuestCode).HasMaxLength(50); 
            builder.HasIndex(g => g.GuestCode).IsUnique();
        }
    }
}
