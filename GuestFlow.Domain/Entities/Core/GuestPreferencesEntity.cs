// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GuestFlow.Domain.Converters;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Misafir tercihleri entity'si
    /// Oda, yemek, aktivite ve iletişim tercihlerini yönetir
    /// </summary>
    public class GuestPreferencesEntity : BaseEntity
    {
        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        // Oda Tercihleri
        /// <summary>
        /// Oda tipi tercihi (örn: Deluxe, Suite, Sea View)
        /// </summary>
        public string? PreferredRoomType { get; set; }

        /// <summary>
        /// Özel oda istekleri (high floor, sea view, quiet room, vb.)
        /// </summary>
        public string? RoomSpecialRequests { get; set; }

        /// <summary>
        /// Yatak tercihi (twin, double, king, vb.)
        /// </summary>
        public string? BedPreference { get; set; }

        /// <summary>
        /// Sigara tercihi (smoking, non-smoking)
        /// </summary>
        public string? SmokingPreference { get; set; }

        // Yemek Tercihleri
        /// <summary>
        /// Diyet tercihleri (vegan, vegetarian, halal, kosher, vb.)
        /// </summary>
        public string? DietaryPreferences { get; set; }

        /// <summary>
        /// Gıda alerjileri (peanut, dairy, gluten, vb.)
        /// </summary>
        public string? FoodAllergies { get; set; }

        /// <summary>
        /// Özel yemek istekleri
        /// </summary>
        public string? SpecialFoodRequests { get; set; }

        // Aktivite Tercihleri
        /// <summary>
        /// Aktivite tercihleri (spor, kültür, eğlence, vb.)
        /// </summary>
        public string? ActivityPreferences { get; set; }

        /// <summary>
        /// İlgi alanları (müze, plaj, gece hayatı, spa, vb.)
        /// </summary>
        public string? Interests { get; set; }

        // İletişim Tercihleri
        /// <summary>
        /// E-posta iletişim tercihi (true = tercih ediyor)
        /// </summary>
        public bool PrefersEmail { get; set; } = true;

        /// <summary>
        /// SMS iletişim tercihi (true = tercih ediyor)
        /// </summary>
        public bool PrefersSMS { get; set; } = true;

        /// <summary>
        /// WhatsApp iletişim tercihi (true = tercih ediyor)
        /// </summary>
        public bool PrefersWhatsApp { get; set; } = false;

        /// <summary>
        /// Telefon iletişim tercihi (true = tercih ediyor)
        /// </summary>
        public bool PrefersPhone { get; set; } = true;

        /// <summary>
        /// Tercih edilen iletişim dili
        /// </summary>
        public string? PreferredLanguage { get; set; }

        // Genel Notlar
        /// <summary>
        /// Genel tercih notları
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Kaynak (PMS, Manual, Guest Input, vb.)
        /// </summary>
        public string Source { get; set; } = "Manual";

        // Navigation Properties
        public virtual GuestEntity Guest { get; set; } = null!;
    }

    public class GuestPreferencesConfiguration : BaseConfiguration<GuestPreferencesEntity>
    {
        public override void Configure(EntityTypeBuilder<GuestPreferencesEntity> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.PreferredRoomType).HasMaxLength(100);
            builder.Property(p => p.Notes).HasMaxLength(1000).HasConversion<EncryptedValueConverter>();
            builder.Property(p => p.RoomSpecialRequests).HasMaxLength(500).HasConversion<EncryptedValueConverter>();
            builder.Property(p => p.DietaryPreferences).HasMaxLength(200).HasConversion<EncryptedValueConverter>();
            builder.Property(p => p.FoodAllergies).HasMaxLength(500).HasConversion<EncryptedValueConverter>();
            builder.Property(p => p.SpecialFoodRequests).HasMaxLength(500).HasConversion<EncryptedValueConverter>();
            builder.Property(p => p.Source).HasMaxLength(50);

            // Guest ile ilişki (one-to-one)
            builder.HasOne(p => p.Guest)
                   .WithOne()
                   .HasForeignKey<GuestPreferencesEntity>(p => p.GuestId)
                   .OnDelete(DeleteBehavior.Cascade);

            // GuestId unique index (her misafir için tek tercih kaydı)
            builder.HasIndex(p => p.GuestId).IsUnique();
        }
    }
}
