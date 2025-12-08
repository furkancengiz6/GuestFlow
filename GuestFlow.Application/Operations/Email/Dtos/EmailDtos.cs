using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Email.Dtos
{
    /// <summary>
    /// E-posta kuyruğu DTO
    /// </summary>
    public class EmailQueueDto
    {
        public int Id { get; set; }
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public string Status { get; set; } = "Pending";
        public int Priority { get; set; } = 5;
        public int RetryCount { get; set; }
        public int MaxRetryCount { get; set; } = 3;
        public DateTime? LastAttemptDate { get; set; }
        public DateTime? SentDate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TemplateName { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
        public List<string>? Attachments { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// E-posta kuyruğu oluşturma DTO
    /// </summary>
    public class CreateEmailQueueDto
    {
        public string To { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public int Priority { get; set; } = 5;
        public string? TemplateName { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
        public List<string>? Attachments { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public DateTime? ScheduledDate { get; set; }
    }

    /// <summary>
    /// E-posta şablonu DTO
    /// </summary>
    public class EmailTemplateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Category { get; set; }
        public Dictionary<string, string>? VariablesDescription { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDefault { get; set; } = false;
        public DateTime? LastModifiedDate { get; set; }
        public int? ModifiedByPersonnelId { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// E-posta şablonu oluşturma/güncelleme DTO
    /// </summary>
    public class CreateEmailTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string? Category { get; set; }
        public Dictionary<string, string>? VariablesDescription { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// E-posta geçmişi DTO
    /// </summary>
    public class EmailHistoryDto
    {
        public int Id { get; set; }
        public string To { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Status { get; set; } = "Sent";
        public DateTime SentDate { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
        public long? EmailSize { get; set; }
        public int AttachmentCount { get; set; }
        public string? SmtpResponse { get; set; }
        public bool IsOpened { get; set; }
        public DateTime? OpenedDate { get; set; }
        public int ClickCount { get; set; }
    }

    /// <summary>
    /// Toplu e-posta gönderim isteği DTO
    /// </summary>
    public class BulkEmailRequestDto
    {
        public List<string> Recipients { get; set; } = new List<string>();
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; } = true;
        public string? TemplateName { get; set; }
        public Dictionary<string, string>? TemplateVariables { get; set; }
        public List<string>? Attachments { get; set; }
        public int Priority { get; set; } = 5;
        public DateTime? ScheduledDate { get; set; }
    }

    /// <summary>
    /// E-posta istatistikleri DTO
    /// </summary>
    public class EmailStatisticsDto
    {
        public int TotalSent { get; set; }
        public int TotalFailed { get; set; }
        public int TotalPending { get; set; }
        public int TotalInQueue { get; set; }
        public decimal SuccessRate { get; set; }
        public Dictionary<string, int> SentByDay { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> SentByTemplate { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> FailedByReason { get; set; } = new Dictionary<string, int>();
        public int AverageDeliveryTime { get; set; } // seconds
        public int TotalOpened { get; set; }
        public decimal OpenRate { get; set; }
        public int TotalClicks { get; set; }
        public decimal ClickRate { get; set; }
    }
}

