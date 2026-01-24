namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Role-Permission mapping entity
    /// Defines which permissions are granted to which roles
    /// </summary>
    public class RolePermissionEntity : BaseEntity
    {
        public string RoleName { get; set; } = string.Empty; // "Admin", "Owner", "Manager", etc.
        public int PermissionId { get; set; }
        
        // Navigation
        public virtual PermissionEntity? Permission { get; set; }
    }
}
