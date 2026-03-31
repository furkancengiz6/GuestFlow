using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestCityTour : BaseEntity
    {
        public int GuestId { get; set; }
        public int CityTourId { get; set; }

        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual CityTourEntity CityTour { get; set; } = null!;
    }

    public class GuestCityTourConfiguration : BaseConfiguration<GuestCityTour>
    {
        public override void Configure(EntityTypeBuilder<GuestCityTour> builder)
        {
            builder.HasKey(gct => new { gct.GuestId, gct.CityTourId });
            
            // BaseEntity'den gelen junction table için gereksiz alanları yoksay
            builder.Ignore(gct => gct.Id); 
            builder.Ignore(gct => gct.CreatedDate); 
            builder.Ignore(gct => gct.UpdatedDate);
            builder.Ignore(gct => gct.CreatedByPersonnelId);
            builder.Ignore(gct => gct.UpdatedByPersonnelId);
            builder.Ignore(gct => gct.IsDeleted);
        }
    }
}
