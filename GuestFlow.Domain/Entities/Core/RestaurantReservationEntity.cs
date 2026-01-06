using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Restoran rezervasyon entity'si
    /// </summary>
    public class RestaurantReservationEntity : BaseEntity
    {
        /// <summary>
        /// Restoran ID
        /// </summary>
        public int RestaurantId { get; set; }

        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Personel ID (oluşturan)
        /// </summary>
        public int PersonnelId { get; set; }

        /// <summary>
        /// Rezervasyon tarihi
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Rezervasyon saati
        /// </summary>
        public TimeSpan ReservationTime { get; set; }

        /// <summary>
        /// Kişi sayısı
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Masa numarası (opsiyonel)
        /// </summary>
        public string? TableNumber { get; set; }

        /// <summary>
        /// Özel istekler
        /// </summary>
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Durum
        /// </summary>
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        /// <summary>
        /// Onay numarası
        /// </summary>
        public string? ConfirmationNumber { get; set; }

        /// <summary>
        /// Transfer ID (otel→restoran transferi, opsiyonel)
        /// </summary>
        public int? TransferId { get; set; }

        /// <summary>
        /// Dönüş transferi ID (restoran→otel transferi, opsiyonel)
        /// </summary>
        public int? ReturnTransferId { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        // Relational Properties
        public virtual RestaurantEntity Restaurant { get; set; } = null!;
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity Personnel { get; set; } = null!;
        public virtual TransferEntity? Transfer { get; set; }
        public virtual TransferEntity? ReturnTransfer { get; set; }
    }

    public class RestaurantReservationConfiguration : BaseConfiguration<RestaurantReservationEntity>
    {
        public override void Configure(EntityTypeBuilder<RestaurantReservationEntity> builder)
        {
            base.Configure(builder);
            builder.Property(r => r.TableNumber).HasMaxLength(50);
            builder.Property(r => r.SpecialRequests).HasMaxLength(1000);
            builder.Property(r => r.ConfirmationNumber).HasMaxLength(50);
            builder.Property(r => r.Notes).HasMaxLength(1000);
            builder.Property(r => r.Status).HasConversion<int>();

            builder.HasOne(r => r.Restaurant)
                   .WithMany()
                   .HasForeignKey(r => r.RestaurantId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasOne(r => r.Guest)
                   .WithMany()
                   .HasForeignKey(r => r.GuestId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasOne(r => r.Personnel)
                   .WithMany()
                   .HasForeignKey(r => r.PersonnelId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .IsRequired();

            builder.HasOne(r => r.Transfer)
                   .WithMany()
                   .HasForeignKey(r => r.TransferId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.ReturnTransfer)
                   .WithMany()
                   .HasForeignKey(r => r.ReturnTransferId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasIndex(r => r.RestaurantId);
            builder.HasIndex(r => r.GuestId);
            builder.HasIndex(r => r.ReservationDate);
            builder.HasIndex(r => r.ConfirmationNumber);
        }
    }
}

