using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Bildirim entity'si
    /// </summary>
    public class NotificationEntity : BaseEntity
    {
        /// <summary>
        /// Bildirim başlığı
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Bildirim içeriği
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Bildirim tipi (Email, SMS, InApp, etc.)
        /// </summary>
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// Alıcı e-posta adresi
        /// </summary>
        public string? RecipientEmail { get; set; }

        /// <summary>
        /// Alıcı personel ID (varsa)
        /// </summary>
        public int? RecipientPersonnelId { get; set; }

        /// <summary>
        /// Alıcı misafir ID (varsa)
        /// </summary>
        public int? RecipientGuestId { get; set; }

        /// <summary>
        /// Bildirim durumu (Sent, Failed, Pending)
        /// </summary>
        public string Status { get; set; } = "Pending";

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
        /// İlişkili entity tipi (Invoice, Transfer, Tour, etc.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// İlişkili entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Okundu mu?
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Okunma tarihi
        /// </summary>
        public DateTime? ReadDate { get; set; }
    }
}

