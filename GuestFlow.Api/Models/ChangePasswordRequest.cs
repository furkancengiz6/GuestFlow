using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ValidatePasswordRequest
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class ValidatePasswordResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
        public int StrengthScore { get; set; }
        public string StrengthLevel { get; set; } = string.Empty;
    }
}

