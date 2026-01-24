namespace GuestFlow.Application.Models.Responses.Auth
{
    /// <summary>
    /// 2FA setup response (secret + QR code)
    /// </summary>
    public class TwoFactorSetupResponse
    {
        public string Secret { get; set; } = string.Empty; // Base32 encoded secret
        public string QrCodeDataUri { get; set; } = string.Empty; // Data URI for QR code image
        public string ManualEntryKey { get; set; } = string.Empty; // Formatted secret for manual entry
        public List<string> RecoveryCodes { get; set; } = new List<string>(); // One-time recovery codes
    }

    /// <summary>
    /// 2FA verification request
    /// </summary>
    public class TwoFactorVerifyRequest
    {
        public string Code { get; set; } = string.Empty; // 6-digit TOTP code
    }

    /// <summary>
    /// 2FA verification response
    /// </summary>
    public class TwoFactorVerifyResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> RemainingRecoveryCodes { get; set; } = new List<string>(); // If recovery code was used
    }
}
