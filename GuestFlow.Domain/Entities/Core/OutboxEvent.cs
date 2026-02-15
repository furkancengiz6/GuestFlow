using System;

namespace GuestFlow.Domain.Entities.Core
{
    public class OutboxEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string EventType { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAtUtc { get; set; }
        public string? Error { get; set; }
    }
}
