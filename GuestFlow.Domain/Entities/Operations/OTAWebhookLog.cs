// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// OTA Webhook Log Entity - Webhook işlemlerini takip eder
    /// </summary>
    public class OTAWebhookLog : BaseEntity
    {
        /// <summary>
        /// OTA Integration ID
        /// </summary>
        public int OTAIntegrationId { get; set; }

        /// <summary>
        /// Provider kodu (BKG, EXP, vb.)
        /// </summary>
        public string ProviderCode { get; set; } = string.Empty;

        /// <summary>
        /// Idempotency key - Aynı webhook'un tekrar işlenmesini önler
        /// </summary>
        public string IdempotencyKey { get; set; } = string.Empty;

        /// <summary>
        /// Webhook event tipi
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Rezervasyon ID (varsa)
        /// </summary>
        public string? ReservationId { get; set; }

        /// <summary>
        /// Webhook payload (JSON)
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// Webhook signature
        /// </summary>
        public string? Signature { get; set; }

        /// <summary>
        /// İşlem durumu (Pending, Processing, Success, Failed, DeadLetter)
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Retry sayısı
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Maksimum retry sayısı
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Son retry zamanı
        /// </summary>
        public DateTime? LastRetryAt { get; set; }

        /// <summary>
        /// Sonraki retry zamanı (backoff ile)
        /// </summary>
        public DateTime? NextRetryAt { get; set; }

        /// <summary>
        /// İşlem başlangıç zamanı
        /// </summary>
        public DateTime ProcessedAt { get; set; }

        /// <summary>
        /// İşlem bitiş zamanı
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Hata mesajı (varsa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Hata detayları (JSON formatında)
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// Dead-letter queue'ya gönderildi mi?
        /// </summary>
        public bool IsDeadLetter { get; set; } = false;

        /// <summary>
        /// Dead-letter queue'ya gönderilme zamanı
        /// </summary>
        public DateTime? DeadLetterAt { get; set; }

        /// <summary>
        /// IP adresi (webhook gönderen)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// User agent (webhook gönderen)
        /// </summary>
        public string? UserAgent { get; set; }

        // Navigation properties
        public virtual OTAIntegration? OTAIntegration { get; set; }
    }

    public class OTAWebhookLogConfiguration : BaseConfiguration<OTAWebhookLog>
    {
        public override void Configure(EntityTypeBuilder<OTAWebhookLog> builder)
        {
            base.Configure(builder);

            builder.Property(w => w.ProviderCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(w => w.IdempotencyKey)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(w => w.IdempotencyKey)
                .IsUnique();

            builder.Property(w => w.EventType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(w => w.ReservationId)
                .HasMaxLength(100);

            builder.Property(w => w.Payload)
                .IsRequired();

            builder.Property(w => w.Signature)
                .HasMaxLength(500);

            builder.Property(w => w.Status)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(w => w.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(l => l.ErrorDetails).IsRequired(false);

            builder.Property(w => w.IpAddress)
                .HasMaxLength(50);

            builder.Property(w => w.UserAgent)
                .HasMaxLength(500);

            // Indexes for performance
            builder.HasIndex(w => new { w.OTAIntegrationId, w.Status });
            builder.HasIndex(w => new { w.ProviderCode, w.Status });
            builder.HasIndex(w => w.NextRetryAt)
                .HasFilter("NextRetryAt IS NOT NULL AND Status = 'Failed'");
            builder.HasIndex(w => w.IsDeadLetter)
                .HasFilter("IsDeadLetter = 1");
        }
    }
}
