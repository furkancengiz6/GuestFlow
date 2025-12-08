using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models
{
    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}

