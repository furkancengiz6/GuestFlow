using GuestFlow.Application.Models.Responses.Auth;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Two-Factor Authentication (2FA) service interface
    /// Supports TOTP (Time-based One-Time Password) - Google Authenticator compatible
    /// </summary>
    public interface ITwoFactorService
    {
        /// <summary>
        /// Generate 2FA setup for a user (secret + QR code)
        /// </summary>
        Task<TwoFactorSetupResponse> GenerateSetupAsync(int personnelId, string email, string issuer = "GuestFlow");

        /// <summary>
        /// Verify and enable 2FA for a user
        /// </summary>
        Task<bool> VerifyAndEnableAsync(int personnelId, string code);

        /// <summary>
        /// Verify 2FA code during login
        /// </summary>
        Task<bool> VerifyCodeAsync(int personnelId, string code);

        /// <summary>
        /// Verify recovery code and remove it from list
        /// </summary>
        Task<bool> VerifyRecoveryCodeAsync(int personnelId, string recoveryCode);

        /// <summary>
        /// Disable 2FA for a user
        /// </summary>
        Task<bool> DisableAsync(int personnelId);

        /// <summary>
        /// Check if 2FA is required for user type (Admin/Owner)
        /// </summary>
        bool IsRequiredForUserType(Domain.Entities.Enum.UserType userType);

        /// <summary>
        /// Check if user has 2FA enabled
        /// </summary>
        Task<bool> IsEnabledAsync(int personnelId);
    }
}
