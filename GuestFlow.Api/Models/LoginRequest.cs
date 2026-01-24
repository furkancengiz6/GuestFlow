using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models
{
    public class LoginRequest
    {
     
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 2FA code (required if user has 2FA enabled)
        /// </summary>
        public string? TwoFactorCode { get; set; }

        /// <summary>
        /// Recovery code (alternative to 2FA code)
        /// </summary>
        public string? RecoveryCode { get; set; }
    }
}
