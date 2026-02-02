namespace GuestFlow.Domain.Entities.Core
{
    public class AuditLog : BaseEntity
    {
        public string? TableName { get; set; }
        public string? Action { get; set; } // INSERT, UPDATE, DELETE
        public string? OldValues { get; set; } // JSON serialized
        public string? NewValues { get; set; } // JSON serialized
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? SessionId { get; set; }
        public string? CorrelationId { get; set; }

        // Navigation property (if needed)
        // public virtual User User { get; set; }
    }
}