using GuestFlow.Domain.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PersonnelModels
{
    public class UpdatePersonnelRequest
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        public UserType? UserType { get; set; }

        [MinLength(8)]
        public string? NewPassword { get; set; } // Şifre değiştirmek için (opsiyonel)
    }
}

