using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class TransferEntity : BaseEntity, ITransfer
    {
        public string PickupAddress { get; set; } = string.Empty; 
        public string DropoffAddress { get; set; } = string.Empty; 
        public DateTime TransferDate { get; set; }
        public decimal Price { get; set; }
        public string Note { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsFromAirport { get; set; }
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }
        public int AirportId { get; set; }
        public int VehicleId { get; set; }
        public int PickupCityId { get; set; } 
        public int DropoffCityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }
        public string Currency { get; set; } = "TRY"; // Para birimi (TRY, USD, EUR, GBP, RUB)


        // Relational Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity Personnel { get; set; } = null!;
        public virtual AirportEntity Airport { get; set; } = null!;
        public virtual VehicleEntity Vehicle { get; set; } = null!;
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();
        public virtual CityEntity PickupCity { get; set; } = null!;
        public virtual CityEntity DropoffCity { get; set; } = null!;
    }

    public class TransferConfiguration : BaseConfiguration<TransferEntity>
    {
        public override void Configure(EntityTypeBuilder<TransferEntity> builder)
        {
            base.Configure(builder);
            builder.Property(t => t.PickupAddress).HasMaxLength(500);
            builder.Property(t => t.DropoffAddress).HasMaxLength(500);
            builder.Property(t => t.Note).HasMaxLength(1000);
            builder.Property(t => t.Status).HasMaxLength(50);
            builder.Property(t => t.Price).HasPrecision(18, 2);
            builder.Property(t => t.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(t => t.FinalPrice).HasPrecision(18, 2);
            builder.Property(t => t.Currency).HasMaxLength(3);
        }
    }
}
