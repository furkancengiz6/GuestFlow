using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestCityTour : BaseEntity
    {
        public int GuestId { get; set; }
        public int CityTourId { get; set; }

        public virtual GuestEntity Guest { get; set; }
        public virtual CityTourEntity CityTour { get; set; }
    }

    public class GuestCityTourConfiguration : BaseConfiguration<GuestCityTour>
    {
        public override void Configure(EntityTypeBuilder<GuestCityTour> builder)
        {
            base.Configure(builder);
           
        }
    }
}