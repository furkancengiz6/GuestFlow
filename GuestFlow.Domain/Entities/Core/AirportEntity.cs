using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class AirportEntity : BaseEntity, IAirport
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int CityId { get; set; }

        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
        public virtual CityEntity City { get; set; }
    }

    public class AirportConfiguration : BaseConfiguration<AirportEntity>
    {
        public override void Configure(EntityTypeBuilder<AirportEntity> builder)
        {
            base.Configure(builder);
           
        }
    }
}