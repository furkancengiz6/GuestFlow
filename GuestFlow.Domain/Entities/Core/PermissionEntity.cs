namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Permission entity - defines individual permissions in the system
    /// </summary>
    public class PermissionEntity : BaseEntity
    {
        public string Code { get; set; } = string.Empty; // Unique permission code (e.g., "guest:view", "transfer:create")
        public string Name { get; set; } = string.Empty; // Human-readable name
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // "Guest", "Transfer", "Invoice", etc.
        public bool IsActive { get; set; } = true;
    }
}
