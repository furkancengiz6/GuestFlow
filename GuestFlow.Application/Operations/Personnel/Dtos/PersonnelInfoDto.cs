using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Personnel.Dtos
{
    public class PersonnelInfoDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
