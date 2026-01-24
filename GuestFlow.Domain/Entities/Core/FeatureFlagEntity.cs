using System;

namespace GuestFlow.Domain.Entities.Core
{
    /// <summary>
    /// Feature flag entity for gradual rollout and A/B testing
    /// </summary>
    public class FeatureFlagEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty; // Unique feature flag name (e.g., "NewDashboard", "AdvancedReports")
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false; // Global enable/disable
        public string Environment { get; set; } = "Production"; // "Development", "Staging", "Production"
        
        // Rollout configuration
        public decimal RolloutPercentage { get; set; } = 0; // 0-100: percentage of users who see this feature
        public bool IsEnabledForAdmins { get; set; } = false; // Always enable for admins (for testing)
        
        // Target configuration
        public string? TargetRoles { get; set; } // Comma-separated roles (e.g., "Admin,Owner")
        public string? TargetUserIds { get; set; } // Comma-separated user IDs for specific user targeting
        
        // Metadata
        public DateTime? EnabledDate { get; set; }
        public DateTime? DisabledDate { get; set; }
        public string? EnabledBy { get; set; } // Personnel name who enabled it
        public string? Notes { get; set; }
    }
}
