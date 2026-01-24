using GuestFlow.Application.Models.Responses.FeatureFlags;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.FeatureFlags
{
    /// <summary>
    /// Feature flag service implementation
    /// Manages feature flags for gradual rollout and A/B testing
    /// </summary>
    public class FeatureFlagService : IFeatureFlagService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FeatureFlagService> _logger;

        public FeatureFlagService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<FeatureFlagService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> IsFeatureEnabledAsync(string featureName, int? userId = null, string? userRole = null, string? environment = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var featureFlag = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == featureName && f.Environment == environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (featureFlag == null)
                {
                    _logger.LogDebug($"Feature flag '{featureName}' not found in environment '{environment}'");
                    return false;
                }

                // Global enable/disable check
                if (!featureFlag.IsEnabled)
                {
                    return false;
                }

                // Admin override: if enabled for admins and user is admin
                if (featureFlag.IsEnabledForAdmins && (userRole == "Admin" || userRole == "Owner"))
                {
                    return true;
                }

                // Role-based targeting
                if (!string.IsNullOrEmpty(featureFlag.TargetRoles))
                {
                    var targetRoles = featureFlag.TargetRoles.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim())
                        .ToList();

                    if (!targetRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                }

                // User ID-based targeting
                if (!string.IsNullOrEmpty(featureFlag.TargetUserIds) && userId.HasValue)
                {
                    var targetUserIds = featureFlag.TargetUserIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => int.TryParse(id.Trim(), out var uid) ? uid : -1)
                        .ToList();

                    if (targetUserIds.Contains(userId.Value))
                    {
                        return true; // Explicitly enabled for this user
                    }
                }

                // Rollout percentage check (deterministic based on user ID)
                if (featureFlag.RolloutPercentage > 0 && userId.HasValue)
                {
                    // Deterministic hash-based rollout (same user always gets same result)
                    var hash = Math.Abs(userId.Value.GetHashCode());
                    var userPercentage = (hash % 100) + 1; // 1-100
                    
                    return userPercentage <= featureFlag.RolloutPercentage;
                }

                // If rollout percentage is 0 and no specific targeting, return false
                if (featureFlag.RolloutPercentage == 0 && 
                    string.IsNullOrEmpty(featureFlag.TargetRoles) && 
                    string.IsNullOrEmpty(featureFlag.TargetUserIds))
                {
                    return false;
                }

                // Default: enabled if global flag is on
                return featureFlag.IsEnabled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking feature flag '{featureName}': {ex.Message}");
                // Fail closed: return false on error
                return false;
            }
        }

        public async Task<List<FeatureFlagDto>> GetAllFeatureFlagsAsync(string? environment = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var flags = await _unitOfWork.FeatureFlags
                    .GetAll(f => (environment == null || f.Environment == environment) && !f.IsDeleted)
                    .OrderBy(f => f.Name)
                    .ToListAsync();

                return flags.Select(f => MapToDto(f)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feature flags");
                throw;
            }
        }

        public async Task<FeatureFlagDto?> GetFeatureFlagAsync(string featureName, string? environment = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var flag = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == featureName && f.Environment == environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                return flag != null ? MapToDto(flag) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting feature flag '{featureName}'");
                throw;
            }
        }

        public async Task<FeatureFlagDto> UpsertFeatureFlagAsync(CreateOrUpdateFeatureFlagRequest request)
        {
            try
            {
                var existing = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == request.Name && f.Environment == request.Environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                FeatureFlagEntity flag;
                if (existing != null)
                {
                    // Update existing
                    existing.Description = request.Description;
                    existing.IsEnabled = request.IsEnabled;
                    existing.RolloutPercentage = request.RolloutPercentage;
                    existing.IsEnabledForAdmins = request.IsEnabledForAdmins;
                    existing.TargetRoles = request.TargetRoles;
                    existing.TargetUserIds = request.TargetUserIds;
                    existing.Notes = request.Notes;
                    
                    if (request.IsEnabled && !existing.IsEnabled)
                    {
                        existing.EnabledDate = DateTime.UtcNow;
                    }
                    else if (!request.IsEnabled && existing.IsEnabled)
                    {
                        existing.DisabledDate = DateTime.UtcNow;
                    }

                    existing.MarkAsUpdated();
                    await _unitOfWork.FeatureFlags.UpdateAsync(existing);
                    flag = existing;
                }
                else
                {
                    // Create new
                    flag = new FeatureFlagEntity
                    {
                        Name = request.Name,
                        Description = request.Description,
                        IsEnabled = request.IsEnabled,
                        Environment = request.Environment,
                        RolloutPercentage = request.RolloutPercentage,
                        IsEnabledForAdmins = request.IsEnabledForAdmins,
                        TargetRoles = request.TargetRoles,
                        TargetUserIds = request.TargetUserIds,
                        Notes = request.Notes,
                        EnabledDate = request.IsEnabled ? DateTime.UtcNow : null
                    };

                    await _unitOfWork.FeatureFlags.AddAsync(flag);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Feature flag '{request.Name}' upserted in environment '{request.Environment}'");
                return MapToDto(flag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error upserting feature flag '{request.Name}': {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EnableFeatureFlagAsync(string featureName, string? environment = null, string? enabledBy = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var flag = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == featureName && f.Environment == environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (flag == null)
                {
                    _logger.LogWarning($"Feature flag '{featureName}' not found in environment '{environment}'");
                    return false;
                }

                flag.IsEnabled = true;
                flag.EnabledDate = DateTime.UtcNow;
                flag.EnabledBy = enabledBy;
                flag.DisabledDate = null;
                flag.MarkAsUpdated();

                await _unitOfWork.FeatureFlags.UpdateAsync(flag);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Feature flag '{featureName}' enabled in environment '{environment}' by {enabledBy}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enabling feature flag '{featureName}': {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisableFeatureFlagAsync(string featureName, string? environment = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var flag = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == featureName && f.Environment == environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (flag == null)
                {
                    _logger.LogWarning($"Feature flag '{featureName}' not found in environment '{environment}'");
                    return false;
                }

                flag.IsEnabled = false;
                flag.DisabledDate = DateTime.UtcNow;
                flag.MarkAsUpdated();

                await _unitOfWork.FeatureFlags.UpdateAsync(flag);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Feature flag '{featureName}' disabled in environment '{environment}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disabling feature flag '{featureName}': {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteFeatureFlagAsync(string featureName, string? environment = null)
        {
            try
            {
                environment = environment ?? _configuration["Environment"] ?? "Production";

                var flag = await _unitOfWork.FeatureFlags
                    .GetAll(f => f.Name == featureName && f.Environment == environment && !f.IsDeleted)
                    .FirstOrDefaultAsync();

                if (flag == null)
                {
                    _logger.LogWarning($"Feature flag '{featureName}' not found in environment '{environment}'");
                    return false;
                }

                flag.IsDeleted = true;
                flag.MarkAsUpdated();

                await _unitOfWork.FeatureFlags.UpdateAsync(flag);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Feature flag '{featureName}' deleted in environment '{environment}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting feature flag '{featureName}': {ex.Message}");
                return false;
            }
        }

        private FeatureFlagDto MapToDto(FeatureFlagEntity flag)
        {
            return new FeatureFlagDto
            {
                Id = flag.Id,
                Name = flag.Name,
                Description = flag.Description,
                IsEnabled = flag.IsEnabled,
                Environment = flag.Environment,
                RolloutPercentage = flag.RolloutPercentage,
                IsEnabledForAdmins = flag.IsEnabledForAdmins,
                TargetRoles = flag.TargetRoles,
                TargetUserIds = flag.TargetUserIds,
                EnabledDate = flag.EnabledDate,
                DisabledDate = flag.DisabledDate,
                EnabledBy = flag.EnabledBy,
                Notes = flag.Notes,
                CreatedDate = flag.CreatedDate,
                UpdatedDate = flag.UpdatedDate
            };
        }
    }
}
