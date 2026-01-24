using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Login attempt tracking for brute-force protection
    /// </summary>
    public class LoginAttemptEntity : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string? IpAddress { get; set; }
        public bool IsSuccessful { get; set; }
        public string? FailureReason { get; set; } // "InvalidPassword", "UserNotFound", "2FAFailed", etc.
        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;
        public int? PersonnelId { get; set; } // Null if user not found

        // Navigation
        public virtual PersonnelEntity? Personnel { get; set; }
    }
}
