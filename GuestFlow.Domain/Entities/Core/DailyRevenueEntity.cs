// GuestFlow.Domain/Entities/Core/DailyRevenueEntity.cs
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Günlük gelir entity'si - Tahsilat bazlı (PaymentEntity'den hesaplanır)
    /// </summary>
    public class DailyRevenueEntity : BaseEntity, IDailyRevenue
    {
        /// <summary>
        /// Gelir tarihi
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Toplam tahsilat (tamamlanmış ödemeler)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Transfer tahsilatları
        /// </summary>
        public decimal TransferRevenue { get; set; }

        /// <summary>
        /// Şehir turu tahsilatları
        /// </summary>
        public decimal CityTourRevenue { get; set; }

        /// <summary>
        /// Yat turu tahsilatları
        /// </summary>
        public decimal YachtTourRevenue { get; set; }

        /// <summary>
        /// Genel tahsilatlar (servise bağlı olmayan)
        /// </summary>
        public decimal GeneralRevenue { get; set; }

        /// <summary>
        /// Toplam ödeme sayısı
        /// </summary>
        public int PaymentCount { get; set; }

        /// <summary>
        /// İade edilen tutar (negatif gelir)
        /// </summary>
        public decimal RefundedAmount { get; set; }

        /// <summary>
        /// Net gelir (TotalRevenue - RefundedAmount)
        /// </summary>
        public decimal NetRevenue { get; set; }
    }

    public class DailyRevenueConfiguration : BaseConfiguration<DailyRevenueEntity>
    {
        public override void Configure(EntityTypeBuilder<DailyRevenueEntity> builder)
        {
            base.Configure(builder);

            builder.Property(dr => dr.Currency)
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(dr => dr.TotalRevenue)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.TransferRevenue)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.CityTourRevenue)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.YachtTourRevenue)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.GeneralRevenue)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.RefundedAmount)
                .HasPrecision(18, 2);

            builder.Property(dr => dr.NetRevenue)
                .HasPrecision(18, 2);

            // Unique index: her gün + para birimi için tek kayıt
            builder.HasIndex(dr => new { dr.Date, dr.Currency })
                .IsUnique();
        }
    }
}
