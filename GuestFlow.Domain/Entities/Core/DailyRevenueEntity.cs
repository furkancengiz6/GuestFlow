// GuestFlow.Domain/Entities/Core/DailyRevenueEntity.cs
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    public class DailyRevenueEntity : BaseEntity, IDailyRevenue
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyRevenueConfiguration : BaseConfiguration<DailyRevenueEntity>
    {
        public override void Configure(EntityTypeBuilder<DailyRevenueEntity> builder)
        {
            base.Configure(builder);
            builder.Property(dr => dr.TotalRevenue)
               .HasPrecision(18, 2); // 18 basamak, 2 ondalık
        }
    }
}
