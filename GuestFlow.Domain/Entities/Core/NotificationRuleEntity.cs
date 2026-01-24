// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Bildirim kuralı entity'si - Otomatik bildirimler için kural tanımları
    /// </summary>
    public class NotificationRuleEntity : BaseEntity
    {
        /// <summary>
        /// Kural adı
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Kural açıklaması
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Kural kategorisi (Payment, Service, Assignment, vb.)
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Kural tipi (OverduePayment, UpcomingService, UnassignedDriver, vb.)
        /// </summary>
        public string RuleType { get; set; } = string.Empty;

        /// <summary>
        /// Kural koşulları (JSON formatında - esnek koşul tanımlama)
        /// Örnek: {"EntityType": "Invoice", "Condition": "DaysOverdue > 3", "Field": "DueDate"}
        /// </summary>
        public string Conditions { get; set; } = string.Empty;

        /// <summary>
        /// Bildirim kanalı (Email, SMS, InApp, All)
        /// </summary>
        public string NotificationChannel { get; set; } = "Email";

        /// <summary>
        /// Bildirim şablonu adı (EmailTemplate veya SMS template)
        /// </summary>
        public string? TemplateName { get; set; }

        /// <summary>
        /// Bildirim alıcı tipi (Guest, Personnel, Admin, All)
        /// </summary>
        public string RecipientType { get; set; } = "Guest";

        /// <summary>
        /// Bildirim alıcı ID (belirli bir misafir/personel için) - null ise tümüne gönder
        /// </summary>
        public int? RecipientId { get; set; }

        /// <summary>
        /// Kural aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Kural önceliği (1-10, 10 en yüksek)
        /// </summary>
        public int Priority { get; set; } = 5;

        /// <summary>
        /// Kural tetiklenme sıklığı (her X dakika/saat/gün)
        /// </summary>
        public int CheckIntervalMinutes { get; set; } = 60;

        /// <summary>
        /// Son kontrol tarihi
        /// </summary>
        public DateTime? LastCheckedAt { get; set; }

        /// <summary>
        /// Son tetiklenme tarihi
        /// </summary>
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// Toplam tetiklenme sayısı
        /// </summary>
        public int TriggerCount { get; set; } = 0;

        /// <summary>
        /// Kural parametreleri (JSON formatında - ek ayarlar)
        /// Örnek: {"MinAmount": 1000, "MaxRetries": 3}
        /// </summary>
        public string? Parameters { get; set; }
    }

    /// <summary>
    /// NotificationRule entity yapılandırması
    /// </summary>
    public class NotificationRuleConfiguration : BaseConfiguration<NotificationRuleEntity>
    {
        public override void Configure(EntityTypeBuilder<NotificationRuleEntity> builder)
        {
            base.Configure(builder);

            builder.Property(r => r.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(r => r.Description)
                .HasMaxLength(1000);

            builder.Property(r => r.Category)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.RuleType)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Conditions)
                .HasMaxLength(4000)
                .IsRequired();

            builder.Property(r => r.NotificationChannel)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.TemplateName)
                .HasMaxLength(100);

            builder.Property(r => r.RecipientType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(r => r.Parameters)
                .HasMaxLength(2000);

            // Index'ler
            builder.HasIndex(r => r.RuleType);
            builder.HasIndex(r => r.Category);
            builder.HasIndex(r => r.IsActive);
            builder.HasIndex(r => new { r.IsActive, r.Priority });
        }
    }
}
