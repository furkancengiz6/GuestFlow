using System.Collections.Generic;

namespace GuestFlow.Application.Models.Responses.Authorization
{
    /// <summary>
    /// Permission DTO
    /// </summary>
    public class PermissionDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Role-Permission matrix DTO (for UI generation)
    /// </summary>
    public class RolePermissionMatrixDto
    {
        public List<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
        public List<string> Roles { get; set; } = new List<string>();
        public Dictionary<string, List<string>> RolePermissions { get; set; } = new Dictionary<string, List<string>>(); // Role -> List of Permission Codes
    }
}
