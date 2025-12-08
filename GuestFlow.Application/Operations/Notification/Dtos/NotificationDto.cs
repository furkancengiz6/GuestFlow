using System;

namespace GuestFlow.Application.Operations.Notification.Dtos
{
    /// <summary>
    /// Bildirim DTO
    /// </summary>
    public class NotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string? RecipientEmail { get; set; }
        public int? RecipientPersonnelId { get; set; }
        public int? RecipientGuestId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDate { get; set; }
    }

    /// <summary>
    /// Bildirim oluşturma DTO
    /// </summary>
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "Email"; // Email, SMS, InApp
        public string? RecipientEmail { get; set; }
        public int? RecipientPersonnelId { get; set; }
        public int? RecipientGuestId { get; set; }
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    /// <summary>
    /// Bildirim şablonu DTO
    /// </summary>
    public class NotificationTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string NotificationType { get; set; } = "Email";
        public string? Description { get; set; }
        public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Test e-postası gönderme DTO
    /// </summary>
    public class SendTestEmailDto
    {
        public string ToEmail { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }

    /// <summary>
    /// Bildirim istatistikleri DTO
    /// </summary>
    public class NotificationStatisticsDto
    {
        public int TotalNotifications { get; set; }
        public int SentNotifications { get; set; }
        public int FailedNotifications { get; set; }
        public int PendingNotifications { get; set; }
        public Dictionary<string, int> NotificationsByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> NotificationsByStatus { get; set; } = new Dictionary<string, int>();
    }
}

