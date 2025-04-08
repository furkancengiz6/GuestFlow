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
            builder.HasKey(gct => new { gct.GuestId, gct.CityTourId });
            builder.Ignore(gct => gct.Id); // BaseEntity'den gelen Id'yi yok say
            builder.Ignore(gct => gct.CreatedDate); 
            builder.Ignore(gct => gct.IsDeleted);

        }
    }
}
