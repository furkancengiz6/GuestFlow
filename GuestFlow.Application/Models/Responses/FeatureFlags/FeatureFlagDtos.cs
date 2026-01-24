using System;

namespace GuestFlow.Application.Models.Responses.FeatureFlags
{
    /// <summary>
    /// Feature flag DTO
    /// </summary>
    public class FeatureFlagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Environment { get; set; } = "Production";
        public decimal RolloutPercentage { get; set; }
        public bool IsEnabledForAdmins { get; set; }
        public string? TargetRoles { get; set; }
        public string? TargetUserIds { get; set; }
        public DateTime? EnabledDate { get; set; }
        public DateTime? DisabledDate { get; set; }
        public string? EnabledBy { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// Create or update feature flag request
    /// </summary>
    public class CreateOrUpdateFeatureFlagRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;
        public string Environment { get; set; } = "Production";
        public decimal RolloutPercentage { get; set; } = 0;
        public bool IsEnabledForAdmins { get; set; } = false;
        public string? TargetRoles { get; set; }
        public string? TargetUserIds { get; set; }
        public string? Notes { get; set; }
    }
}
