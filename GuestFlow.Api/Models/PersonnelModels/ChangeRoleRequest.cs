using GuestFlow.Domain.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PersonnelModels
{
    public class ChangeRoleRequest
    {
        [Required]
        public UserType UserType { get; set; }
    }
}

