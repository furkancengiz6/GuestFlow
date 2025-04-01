
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    public class VehicleEntity : BaseEntity, IVehicle
    {
        public string Type { get; set; }
        public string PlateNumber { get; set; }
        public int Capacity { get; set; }
        public decimal DailyPrice { get; set; }

        // Relational Property
        public virtual ICollection<TransferEntity> Transfers { get; set; } = new List<TransferEntity>();
    }

    public class VehicleConfiguration : BaseConfiguration<VehicleEntity>
    {
        public override void Configure(EntityTypeBuilder<VehicleEntity> builder)
        {
            base.Configure(builder);
           
        }
    }
}