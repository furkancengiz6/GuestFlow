using GuestFlow.Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Rezervasyon entity'si
    /// Transfer, CityTour ve YachtTour için rezervasyon yönetimi
    /// </summary>
    public class ReservationEntity : BaseEntity
    {
        /// <summary>
        /// Rezervasyon numarası (benzersiz)
        /// </summary>
        public string ReservationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Personel ID
        /// </summary>
        public int PersonnelId { get; set; }

        /// <summary>
        /// Rezervasyon tipi (Transfer, CityTour, YachtTour)
        /// </summary>
        public string ServiceType { get; set; } = string.Empty;

        /// <summary>
        /// Servis ID (TransferId, CityTourId veya YachtTourId)
        /// </summary>
        public int ServiceId { get; set; }

        /// <summary>
        /// Rezervasyon durumu
        /// </summary>
        public ReservationStatus Status { get; set; }

        /// <summary>
        /// Rezervasyon tarihi
        /// </summary>
        public DateTime ReservationDate { get; set; }

        /// <summary>
        /// Onay tarihi (null ise henüz onaylanmamış)
        /// </summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>
        /// İptal tarihi (null ise iptal edilmemiş)
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// İptal nedeni
        /// </summary>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Check-in tarihi (Otel rezervasyonları için)
        /// </summary>
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Check-out tarihi (Otel rezervasyonları için)
        /// </summary>
        public DateTime? CheckOutDate { get; set; }

        /// <summary>
        /// Oda Tipi ID (Otel rezervasyonları için)
        /// </summary>
        public int? RoomTypeId { get; set; }

        /// <summary>
        /// Toplam tutar
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        // Relational Properties
        public virtual GuestEntity Guest { get; set; } = null!;
        public virtual PersonnelEntity Personnel { get; set; } = null!;
    }

    public class ReservationConfiguration : BaseConfiguration<ReservationEntity>
    {
        public override void Configure(EntityTypeBuilder<ReservationEntity> builder)
        {
            base.Configure(builder);
            
            builder.Property(r => r.ReservationNumber)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.ServiceType)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(r => r.Status)
                .HasConversion<int>()
                .IsRequired();
            
            builder.Property(r => r.CancellationReason)
                .HasMaxLength(500);
            
            builder.Property(r => r.Notes)
                .HasMaxLength(1000);
            
            builder.Property(r => r.TotalAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            
            builder.Property(r => r.Currency)
                .HasMaxLength(3)
                .IsRequired();

            // Rezervasyon numarası benzersiz olmalı
            builder.HasIndex(r => r.ReservationNumber)
                .IsUnique();

            // Foreign key ilişkileri
            builder.HasOne(r => r.Guest)
                .WithMany()
                .HasForeignKey(r => r.GuestId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Personnel)
                .WithMany()
                .HasForeignKey(r => r.PersonnelId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

