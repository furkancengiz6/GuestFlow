using GuestFlow.Domain.Entities.Interfaces;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class CityTourEntity : BaseEntity, ICityTour
    {
        public DateTime TourDate { get; set; }
        public string Language { get; set; }
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public int OwnerGuestId { get; set; }
        public int PersonnelId { get; set; }
        public int CityId { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal FinalPrice { get; set; }


        // Relational Properties
        public virtual GuestEntity OwnerGuest { get; set; } 
        public virtual PersonnelEntity Personnel { get; set; }
        public virtual ICollection<GuestCityTour> GuestCityTours { get; set; } = new List<GuestCityTour>();
        public virtual ICollection<InvoicesEntity> Invoices { get; set; } = new List<InvoicesEntity>();

        public virtual CityEntity City { get; set; }
    }

    public class CityTourConfiguration : BaseConfiguration<CityTourEntity>
    {
        public override void Configure(EntityTypeBuilder<CityTourEntity> builder)
        {
            base.Configure(builder);
            
        }
    }
}