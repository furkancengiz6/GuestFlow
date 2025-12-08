using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// E-posta gönderim geçmişi entity'si
    /// </summary>
    public class EmailHistoryEntity : BaseEntity
    {
        /// <summary>
        /// Alıcı e-posta adresi
        /// </summary>
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Gönderen e-posta adresi
        /// </summary>
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// E-posta konusu
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// E-posta durumu (Sent, Failed, Bounced)
        /// </summary>
        public string Status { get; set; } = "Sent";

        /// <summary>
        /// Gönderim tarihi
        /// </summary>
        public DateTime SentDate { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Şablon adı (varsa)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// İlişkili entity tipi
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// E-posta boyutu (bytes)
        /// </summary>
        public long? EmailSize { get; set; }

        /// <summary>
        /// Ek dosya sayısı
        /// </summary>
        public int AttachmentCount { get; set; } = 0;

        /// <summary>
        /// SMTP sunucu yanıtı
        /// </summary>
        public string? SmtpResponse { get; set; }

        /// <summary>
        /// Açıldı mı? (tracking için)
        /// </summary>
        public bool IsOpened { get; set; } = false;

        /// <summary>
        /// Açılma tarihi
        /// </summary>
        public DateTime? OpenedDate { get; set; }

        /// <summary>
        /// Tıklama sayısı (link tracking için)
        /// </summary>
        public int ClickCount { get; set; } = 0;
    }

    /// <summary>
    /// EmailHistory entity yapılandırması
    /// </summary>
    public class EmailHistoryConfiguration : BaseConfiguration<EmailHistoryEntity>
    {
        public override void Configure(EntityTypeBuilder<EmailHistoryEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.To)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.From)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Subject)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.TemplateName)
                .HasMaxLength(100);

            builder.Property(e => e.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(e => e.SmtpResponse)
                .HasMaxLength(4000);

            // Index'ler
            builder.HasIndex(e => e.To);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.SentDate);
            builder.HasIndex(e => e.TemplateName);
            builder.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId });
        }
    }
}

