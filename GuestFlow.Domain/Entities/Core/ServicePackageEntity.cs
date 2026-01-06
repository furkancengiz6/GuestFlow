using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Servis paketi entity'si (Transfer, Tur, Restoran rezervasyonlarını içeren paketler)
    /// </summary>
    public class ServicePackageEntity : BaseEntity
    {
        /// <summary>
        /// Paket adı
        /// </summary>
        public string PackageName { get; set; } = string.Empty;

        /// <summary>
        /// Paket açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Paket tipi
        /// </summary>
        public PackageType PackageType { get; set; } = PackageType.Standard;

        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Toplam fiyat
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// İndirim yüzdesi
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// İndirimli fiyat
        /// </summary>
        public decimal FinalPrice { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Paket içeriği (JSON formatında: transfers, cityTours, yachtTours, restaurantReservations)
        /// </summary>
        public string? PackageContent { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        // Relational Properties
        public virtual ICollection<Operations.PackageTransferEntity> PackageTransfers { get; set; } = new List<Operations.PackageTransferEntity>();
        public virtual ICollection<Operations.PackageCityTourEntity> PackageCityTours { get; set; } = new List<Operations.PackageCityTourEntity>();
        public virtual ICollection<Operations.PackageYachtTourEntity> PackageYachtTours { get; set; } = new List<Operations.PackageYachtTourEntity>();
        public virtual ICollection<Operations.PackageRestaurantReservationEntity> PackageRestaurantReservations { get; set; } = new List<Operations.PackageRestaurantReservationEntity>();
    }

    public class ServicePackageConfiguration : BaseConfiguration<ServicePackageEntity>
    {
        public override void Configure(EntityTypeBuilder<ServicePackageEntity> builder)
        {
            base.Configure(builder);
            builder.Property(p => p.PackageName).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(2000);
            builder.Property(p => p.TotalPrice).HasPrecision(18, 2);
            builder.Property(p => p.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(p => p.FinalPrice).HasPrecision(18, 2);
            builder.Property(p => p.Currency).HasMaxLength(3);
            builder.Property(p => p.PackageContent).HasMaxLength(5000);
            builder.Property(p => p.Notes).HasMaxLength(2000);
            builder.Property(p => p.PackageType).HasConversion<int>();

            builder.HasIndex(p => p.PackageName);
            builder.HasIndex(p => p.PackageType);
        }
    }
}

