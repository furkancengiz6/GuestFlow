using System;

namespace GuestFlow.Domain.Entities.Core
{
    public class GraphAuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string Operation { get; set; } = null!; // DiscoverConnections, PredictRisks, etc.
        public string? QueryParameters { get; set; } // JSON
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
