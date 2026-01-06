using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// İtinerary item entity'si (Transfer, Tur, Restoran rezervasyonu vb.)
    /// </summary>
    public class ItineraryItemEntity : BaseEntity
    {
        /// <summary>
        /// İtinerary ID
        /// </summary>
        public int ItineraryId { get; set; }

        /// <summary>
        /// Item tipi (Transfer, CityTour, YachtTour, RestaurantReservation)
        /// </summary>
        public ItineraryItemType ItemType { get; set; }

        /// <summary>
        /// Servis ID (TransferId, CityTourId, YachtTourId, RestaurantReservationId)
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Planlanan tarih ve saat
        /// </summary>
        public DateTime ScheduledDateTime { get; set; }

        /// <summary>
        /// Sıra numarası (itinerary içindeki sıralama)
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Durum
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        // Relational Properties
        public virtual ItineraryEntity Itinerary { get; set; } = null!;
    }

    public class ItineraryItemConfiguration : BaseConfiguration<ItineraryItemEntity>
    {
        public override void Configure(EntityTypeBuilder<ItineraryItemEntity> builder)
        {
            base.Configure(builder);
            builder.Property(i => i.Status).HasMaxLength(50);
            builder.Property(i => i.Notes).HasMaxLength(1000);
            builder.Property(i => i.ItemType).HasConversion<int>();

            builder.HasOne(i => i.Itinerary)
                   .WithMany(it => it.Items)
                   .HasForeignKey(i => i.ItineraryId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .IsRequired();

            builder.HasIndex(i => i.ItineraryId);
            builder.HasIndex(i => i.ScheduledDateTime);
            builder.HasIndex(i => new { i.ItineraryId, i.Order });
        }
    }
}

