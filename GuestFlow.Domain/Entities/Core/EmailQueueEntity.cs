using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// E-posta kuyruğu entity'si
    /// </summary>
    public class EmailQueueEntity : BaseEntity
    {
        /// <summary>
        /// Alıcı e-posta adresi
        /// </summary>
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// E-posta konusu
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// E-posta içeriği (HTML veya text)
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// HTML formatında mı?
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// E-posta durumu (Pending, Processing, Sent, Failed)
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Öncelik (1-10, 10 en yüksek)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Gönderim denemesi sayısı
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Maksimum deneme sayısı
        /// </summary>
        public int MaxRetryCount { get; set; } = 3;

        /// <summary>
        /// Son deneme tarihi
        /// </summary>
        public DateTime? LastAttemptDate { get; set; }

        /// <summary>
        /// Gönderim tarihi
        /// </summary>
        public DateTime? SentDate { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Şablon adı (varsa)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Şablon değişkenleri (JSON formatında)
        /// </summary>
        public string? TemplateVariables { get; set; }

        /// <summary>
        /// Ek dosyalar (virgülle ayrılmış dosya yolları)
        /// </summary>
        public string? Attachments { get; set; }

        /// <summary>
        /// İlişkili entity tipi
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Planlanmış gönderim tarihi
        /// </summary>
        public DateTime? ScheduledDate { get; set; }
    }

    /// <summary>
    /// EmailQueue entity yapılandırması
    /// </summary>
    public class EmailQueueConfiguration : BaseConfiguration<EmailQueueEntity>
    {
        public override void Configure(EntityTypeBuilder<EmailQueueEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.To)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(e => e.Subject)
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(e => e.Body)
                .IsRequired();

            builder.Property(e => e.Status)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.TemplateName)
                .HasMaxLength(100);

            builder.Property(e => e.TemplateVariables);

            builder.Property(e => e.Attachments)
                .HasMaxLength(2000);

            builder.Property(e => e.RelatedEntityType)
                .HasMaxLength(50);

            builder.Property(e => e.ErrorMessage)
                .HasMaxLength(2000);

            // Index'ler
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => new { e.Status, e.Priority });
            builder.HasIndex(e => e.ScheduledDate);
        }
    }
}

