using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Misafir seyahat planı (İtinerary) entity'si
    /// </summary>
    public class ItineraryEntity : BaseEntity
    {
        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Personel ID (oluşturan)
        /// </summary>
        public int PersonnelId { get; set; }

        /// <summary>
        /// Başlangıç tarihi
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Bitiş tarihi
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Durum
        /// </summary>
        public ItineraryStatus Status { get; set; } = ItineraryStatus.Draft;

        /// <summary>
        /// Toplam maliyet
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// İtinerary numarası (benzersiz)
        /// </summary>
        public string ItineraryNumber { get; set; } = string.Empty;

        // Relational Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity Personnel { get; set; } = null!;
        public virtual ICollection<ItineraryItemEntity> Items { get; set; } = new List<ItineraryItemEntity>();
    }

    public class ItineraryConfiguration : BaseConfiguration<ItineraryEntity>
    {
        public override void Configure(EntityTypeBuilder<ItineraryEntity> builder)
        {
            base.Configure(builder);
            builder.Property(i => i.TotalCost).HasPrecision(18, 2);
            builder.Property(i => i.Currency).HasMaxLength(3);
            builder.Property(i => i.Notes).HasMaxLength(2000);
            builder.Property(i => i.ItineraryNumber).HasMaxLength(50).IsRequired();
            builder.Property(i => i.Status).HasConversion<int>();

            builder.HasOne(i => i.Guest)
                   .WithMany()
                   .HasForeignKey(i => i.GuestId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasOne(i => i.Personnel)
                   .WithMany()
                   .HasForeignKey(i => i.PersonnelId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasIndex(i => i.ItineraryNumber).IsUnique();
            builder.HasIndex(i => i.GuestId);
            builder.HasIndex(i => i.StartDate);
            builder.HasIndex(i => i.EndDate);
        }
    }
}

