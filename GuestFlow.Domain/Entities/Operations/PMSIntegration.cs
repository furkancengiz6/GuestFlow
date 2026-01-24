// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Operations
{
    /// <summary>
    /// PMS (Property Management System) entegrasyonu entity'si
    /// Opera, Elektraweb gibi otel yönetim sistemleri ile entegrasyon için
    /// </summary>
    public class PMSIntegration : BaseEntity
    {
        /// <summary>
        /// PMS provider adı (Opera, Elektraweb, vb.)
        /// </summary>
        public string ProviderName { get; set; } = string.Empty;

        /// <summary>
        /// PMS provider kodu (OPERA, ELEKTRAWEB, vb.)
        /// </summary>
        public string ProviderCode { get; set; } = string.Empty;

        /// <summary>
        /// API endpoint URL
        /// </summary>
        public string ApiEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// API key veya client ID
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// API secret veya client secret
        /// </summary>
        public string? ApiSecret { get; set; }

        /// <summary>
        /// Access token (OAuth 2.0 için)
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Token expiration date
        /// </summary>
        public DateTime? TokenExpiresAt { get; set; }

        /// <summary>
        /// Refresh token (OAuth 2.0 için)
        /// </summary>
        public string? RefreshToken { get; set; }

        /// <summary>
        /// Entegrasyon aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Webhook URL (PMS'den gelen webhook'lar için)
        /// </summary>
        public string? WebhookUrl { get; set; }

        /// <summary>
        /// Webhook secret (webhook signature doğrulama için)
        /// </summary>
        public string? WebhookSecret { get; set; }

        /// <summary>
        /// Son senkronizasyon durumu (Success, Failed, InProgress)
        /// </summary>
        public string? LastSyncStatus { get; set; }

        /// <summary>
        /// Son senkronizasyon tarihi
        /// </summary>
        public DateTime? LastSyncDate { get; set; }

        /// <summary>
        /// Senkronizasyon hata mesajı
        /// </summary>
        public string? SyncErrorMessage { get; set; }

        /// <summary>
        /// Senkronizasyon modu (RealTime, Polling, Batch)
        /// </summary>
        public PMSSyncMode SyncMode { get; set; } = PMSSyncMode.Polling;

        /// <summary>
        /// Polling interval (dakika) - Polling modunda kullanılır
        /// </summary>
        public int PollingIntervalMinutes { get; set; } = 5;

        /// <summary>
        /// Son başarılı bağlantı testi tarihi
        /// </summary>
        public DateTime? LastConnectionTestDate { get; set; }

        /// <summary>
        /// Bağlantı testi sonucu
        /// </summary>
        public bool? LastConnectionTestResult { get; set; }

        // Navigation properties
        public virtual ICollection<PMSSyncHistory> SyncHistories { get; set; } = new List<PMSSyncHistory>();
        public virtual ICollection<PMSGuestMapping> GuestMappings { get; set; } = new List<PMSGuestMapping>();
        public virtual ICollection<PMSReservationMapping> ReservationMappings { get; set; } = new List<PMSReservationMapping>();
    }

    /// <summary>
    /// PMS senkronizasyon geçmişi - audit log için
    /// </summary>
    public class PMSSyncHistory : BaseEntity
    {
        public int PMSIntegrationId { get; set; }
        public PMSSyncType SyncType { get; set; }
        public string EntityType { get; set; } = string.Empty; // Guest, Reservation, Room, Folio
        public string? EntityId { get; set; } // PMS entity ID
        public PMSSyncStatus Status { get; set; }
        public DateTime SyncStartTime { get; set; }
        public DateTime? SyncEndTime { get; set; }
        public int? RecordsProcessed { get; set; }
        public int? RecordsSucceeded { get; set; }
        public int? RecordsFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SyncDetails { get; set; } // JSON formatında detaylar

        // Navigation properties
        public virtual PMSIntegration PMSIntegration { get; set; } = null!;
    }

    /// <summary>
    /// PMS misafir - GuestFlow misafir eşleştirmesi
    /// </summary>
    public class PMSGuestMapping : BaseEntity
    {
        public int PMSIntegrationId { get; set; }
        public string PMSGuestId { get; set; } = string.Empty; // PMS'deki misafir ID'si
        public int GuestFlowGuestId { get; set; } // GuestFlow'daki misafir ID'si
        public DateTime LastSyncedAt { get; set; }
        public string? SyncStatus { get; set; } // Synced, Conflict, Error
        public string? ConflictDetails { get; set; } // Conflict durumunda detaylar

        // Navigation properties
        public virtual PMSIntegration PMSIntegration { get; set; } = null!;
        public virtual GuestEntity GuestFlowGuest { get; set; } = null!;
    }

    /// <summary>
    /// PMS rezervasyon - GuestFlow rezervasyon eşleştirmesi
    /// </summary>
    public class PMSReservationMapping : BaseEntity
    {
        public int PMSIntegrationId { get; set; }
        public string PMSReservationId { get; set; } = string.Empty; // PMS'deki rezervasyon ID'si
        public int? GuestFlowReservationId { get; set; } // GuestFlow'daki rezervasyon ID'si (nullable - henüz oluşturulmamış olabilir)
        public DateTime LastSyncedAt { get; set; }
        public string? SyncStatus { get; set; } // Synced, Conflict, Error
        public string? ConflictDetails { get; set; }

        // Navigation properties
        public virtual PMSIntegration PMSIntegration { get; set; } = null!;
    }

    /// <summary>
    /// PMS provider enum
    /// </summary>
    public enum PMSProvider
    {
        Opera,
        Elektraweb,
        Mews,
        Cloudbeds,
        LittleHotelier,
        Other
    }

    /// <summary>
    /// PMS senkronizasyon modu
    /// </summary>
    public enum PMSSyncMode
    {
        RealTime,   // Webhook-based real-time sync
        Polling,    // Scheduled polling (her X dakikada bir)
        Batch       // Günlük toplu senkronizasyon
    }

    /// <summary>
    /// PMS senkronizasyon tipi
    /// </summary>
    public enum PMSSyncType
    {
        Guest,
        Reservation,
        Room,
        Folio,
        FullSync
    }

    /// <summary>
    /// PMS senkronizasyon durumu
    /// </summary>
    public enum PMSSyncStatus
    {
        InProgress,
        Success,
        Failed,
        PartialSuccess
    }
}
