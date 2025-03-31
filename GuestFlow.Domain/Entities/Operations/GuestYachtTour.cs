using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Operations
{
    public class GuestYachtTour : BaseEntity
    {
        public int GuestId { get; set; }
        public int YachtTourId { get; set; }

        public virtual GuestEntity Guest { get; set; }
        public virtual YachtTourEntity YachtTour { get; set; }
    }

    public class GuestYachtTourConfiguration : BaseConfiguration<GuestYachtTour>
    {
        public override void Configure(EntityTypeBuilder<GuestYachtTour> builder)
        {
            base.Configure(builder);

            //builder.Ignore(x => x.Id);
           // builder.HasKey(gyt => new { gyt.GuestId, gyt.YachtTourId });
           
        }
    }
}
