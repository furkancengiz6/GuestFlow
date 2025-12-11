using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.GuestModels
{
    public class UpdateGuestRequest
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Nationality { get; set; } = string.Empty;

        [Required]
        public bool IsSpecialGuest { get; set; }
    }
}
