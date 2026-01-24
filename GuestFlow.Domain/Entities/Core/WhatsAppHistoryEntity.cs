// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// WhatsApp gönderim geçmişi entity'si
    /// </summary>
    public class WhatsAppHistoryEntity : BaseEntity
    {
        /// <summary>
        /// Alıcı telefon numarası (WhatsApp formatında: 905551234567)
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Mesaj içeriği
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Mesaj durumu (Pending, Sent, Delivered, Read, Failed)
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Gönderim tarihi
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Teslim tarihi
        /// </summary>
        public DateTime? DeliveredDate { get; set; }

        /// <summary>
        /// Okunma tarihi
        /// </summary>
        public DateTime? ReadDate { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// WhatsApp Business API provider
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// WhatsApp mesaj ID (wamid)
        /// </summary>
        public string? MessageId { get; set; }

        /// <summary>
        /// Gateway yanıtı (JSON formatında)
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>
        /// Şablon adı (varsa)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Şablon parametreleri (JSON formatında)
        /// </summary>
        public string? TemplateParameters { get; set; }

        /// <summary>
        /// Mesaj tipi (Text, Template, Interactive, Document, Image, Location)
        /// </summary>
        public string MessageType { get; set; } = "Text";

        /// <summary>
        /// Rich message data (JSON formatında - butonlar, dokümanlar, vb.)
        /// </summary>
        public string? RichMessageData { get; set; }

        /// <summary>
        /// İlişkili entity tipi (Transfer, CityTour, YachtTour, Reservation, vb.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Misafir ID (varsa)
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Personel ID (varsa)
        /// </summary>
        public int? PersonnelId { get; set; }

        /// <summary>
        /// Mesaj tipi (Reminder, Confirmation, Notification, vb.)
        /// </summary>
        public string? MessageCategory { get; set; }

        // Relational Properties
        public virtual GuestEntity? Guest { get; set; }
        public virtual PersonnelEntity? Personnel { get; set; }
    }

    /// <summary>
    /// WhatsAppHistory entity yapılandırması
    /// </summary>
    public class WhatsAppHistoryConfiguration : BaseConfiguration<WhatsAppHistoryEntity>
    {
        public override void Configure(EntityTypeBuilder<WhatsAppHistoryEntity> builder)
        {
            base.Configure(builder);

            builder.Property(w => w.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(w => w.Message)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(w => w.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(w => w.Provider)
                .HasMaxLength(50);

            builder.Property(w => w.MessageId)
                .HasMaxLength(200);

            builder.Property(w => w.GatewayResponse)
                .HasMaxLength(4000);

            builder.Property(w => w.TemplateName)
                .HasMaxLength(100);

            builder.Property(w => w.TemplateParameters)
                .HasMaxLength(2000);

            builder.Property(w => w.MessageType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(w => w.RichMessageData)
                .HasMaxLength(4000);

            builder.Property(w => w.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(w => w.MessageCategory)
                .HasMaxLength(50);

            builder.Property(w => w.ErrorMessage)
                .HasMaxLength(500);

            // Foreign Key Relationships
            builder.HasOne(w => w.Guest)
                .WithMany()
                .HasForeignKey(w => w.GuestId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(w => w.Personnel)
                .WithMany()
                .HasForeignKey(w => w.PersonnelId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(w => w.PhoneNumber);
            builder.HasIndex(w => w.Status);
            builder.HasIndex(w => w.SentDate);
            builder.HasIndex(w => w.MessageId);
            builder.HasIndex(w => w.GuestId);
            builder.HasIndex(w => new { w.RelatedEntityType, w.RelatedEntityId });
        }
    }
}
