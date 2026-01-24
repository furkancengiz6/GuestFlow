using GuestFlow.Application.Models.Responses.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Authorization
{
    /// <summary>
    /// Permission service interface
    /// Manages role-permission matrix for centralized authorization
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Check if user has permission
        /// </summary>
        Task<bool> HasPermissionAsync(string userRole, string permissionCode);

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        Task<List<PermissionDto>> GetPermissionsForRoleAsync(string roleName);

        /// <summary>
        /// Get all permissions (for UI generation)
        /// </summary>
        Task<List<PermissionDto>> GetAllPermissionsAsync();

        /// <summary>
        /// Get role-permission matrix (for UI generation)
        /// </summary>
        Task<RolePermissionMatrixDto> GetRolePermissionMatrixAsync();

        /// <summary>
        /// Assign permission to role
        /// </summary>
        Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionCode);

        /// <summary>
        /// Remove permission from role
        /// </summary>
        Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionCode);

        /// <summary>
        /// Initialize default permissions and role mappings (seed data)
        /// </summary>
        Task InitializeDefaultPermissionsAsync();
    }
}
