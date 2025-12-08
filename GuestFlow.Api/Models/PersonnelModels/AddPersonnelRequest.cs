using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PersonnelModels
{
    public class AddPersonnelRequest
    {
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;
    }
}

