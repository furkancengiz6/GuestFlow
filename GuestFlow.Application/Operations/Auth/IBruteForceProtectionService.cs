namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Brute-force protection service interface
    /// Tracks login attempts and implements rate limiting
    /// </summary>
    public interface IBruteForceProtectionService
    {
        /// <summary>
        /// Check if login is allowed for email/IP (not locked due to too many failed attempts)
        /// </summary>
        Task<bool> IsLoginAllowedAsync(string email, string? ipAddress);

        /// <summary>
        /// Record a login attempt (successful or failed)
        /// </summary>
        Task RecordLoginAttemptAsync(string email, string? ipAddress, bool isSuccessful, string? failureReason, int? personnelId = null);

        /// <summary>
        /// Get remaining lockout time in seconds (0 if not locked)
        /// </summary>
        Task<int> GetRemainingLockoutTimeAsync(string email, string? ipAddress);

        /// <summary>
        /// Get failed attempt count in the lockout window
        /// </summary>
        Task<int> GetFailedAttemptCountAsync(string email, string? ipAddress);

        /// <summary>
        /// Clear failed attempts for email/IP (after successful login)
        /// </summary>
        Task ClearFailedAttemptsAsync(string email, string? ipAddress);
    }
}
