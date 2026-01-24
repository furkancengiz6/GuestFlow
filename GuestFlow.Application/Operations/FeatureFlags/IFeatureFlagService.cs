using GuestFlow.Application.Models.Responses.FeatureFlags;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.FeatureFlags
{
    /// <summary>
    /// Feature flag service interface
    /// Manages feature flags for gradual rollout and A/B testing
    /// </summary>
    public interface IFeatureFlagService
    {
        /// <summary>
        /// Check if a feature is enabled for a user
        /// </summary>
        Task<bool> IsFeatureEnabledAsync(string featureName, int? userId = null, string? userRole = null, string? environment = null);

        /// <summary>
        /// Get all feature flags
        /// </summary>
        Task<List<FeatureFlagDto>> GetAllFeatureFlagsAsync(string? environment = null);

        /// <summary>
        /// Get feature flag by name
        /// </summary>
        Task<FeatureFlagDto?> GetFeatureFlagAsync(string featureName, string? environment = null);

        /// <summary>
        /// Create or update feature flag
        /// </summary>
        Task<FeatureFlagDto> UpsertFeatureFlagAsync(CreateOrUpdateFeatureFlagRequest request);

        /// <summary>
        /// Enable feature flag
        /// </summary>
        Task<bool> EnableFeatureFlagAsync(string featureName, string? environment = null, string? enabledBy = null);

        /// <summary>
        /// Disable feature flag
        /// </summary>
        Task<bool> DisableFeatureFlagAsync(string featureName, string? environment = null);

        /// <summary>
        /// Delete feature flag
        /// </summary>
        Task<bool> DeleteFeatureFlagAsync(string featureName, string? environment = null);
    }
}
