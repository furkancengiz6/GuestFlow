using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses.FeatureFlags;
using GuestFlow.Application.Operations.FeatureFlags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Feature flags API endpoints for gradual rollout and A/B testing
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner")] // Only Admin and Owner can manage feature flags
    [Tags("Platform - Feature Flags")]
    public class FeatureFlagsController : BaseController
    {
        private readonly IFeatureFlagService _featureFlagService;
        private readonly ILogger<FeatureFlagsController> _logger;

        public FeatureFlagsController(
            IFeatureFlagService featureFlagService,
            ILogger<FeatureFlagsController> logger)
        {
            _featureFlagService = featureFlagService;
            _logger = logger;
        }

        /// <summary>
        /// Check if a feature is enabled for current user
        /// </summary>
        [HttpGet("check/{featureName}")]
        [AllowAnonymous] // Allow checking without auth (for public features)
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckFeature(
            [FromRoute] string featureName,
            [FromQuery] string? environment = null)
        {
            try
            {
                int? userId = null;
                string? userRole = null;

                // Get user info from claims if authenticated
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int uid))
                    {
                        userId = uid;
                    }

                    userRole = User.Claims.FirstOrDefault(c => c.Type == "UserType" || c.Type == "role")?.Value;
                }

                var isEnabled = await _featureFlagService.IsFeatureEnabledAsync(featureName, userId, userRole, environment);
                return Success(isEnabled, $"Feature flag '{featureName}' check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feature flag check failed for '{featureName}'");
                return Error("Feature flag kontrolü sırasında bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all feature flags
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllFeatureFlags([FromQuery] string? environment = null)
        {
            try
            {
                var result = await _featureFlagService.GetAllFeatureFlagsAsync(environment);
                return Success(result, "Feature flags başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feature flags getirilirken hata oluştu.");
                return Error("Feature flags getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get feature flag by name
        /// </summary>
        [HttpGet("{featureName}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFeatureFlag(
            [FromRoute] string featureName,
            [FromQuery] string? environment = null)
        {
            try
            {
                var result = await _featureFlagService.GetFeatureFlagAsync(featureName, environment);
                if (result == null)
                {
                    return NotFound(new { message = $"Feature flag '{featureName}' bulunamadı." });
                }
                return Success(result, "Feature flag başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feature flag '{featureName}' getirilirken hata oluştu.");
                return Error("Feature flag getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Create or update feature flag
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpsertFeatureFlag([FromBody] CreateOrUpdateFeatureFlagRequest request)
        {
            try
            {
                var userName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "System";
                var result = await _featureFlagService.UpsertFeatureFlagAsync(request);
                return Success(result, "Feature flag başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feature flag kaydedilirken hata oluştu.");
                return Error("Feature flag kaydedilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Enable feature flag
        /// </summary>
        [HttpPost("{featureName}/enable")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableFeatureFlag(
            [FromRoute] string featureName,
            [FromQuery] string? environment = null)
        {
            try
            {
                var userName = User.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "System";
                var result = await _featureFlagService.EnableFeatureFlagAsync(featureName, environment, userName);
                if (!result)
                {
                    return BadRequest(new { message = $"Feature flag '{featureName}' etkinleştirilemedi." });
                }
                return Success(true, $"Feature flag '{featureName}' başarıyla etkinleştirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feature flag '{featureName}' etkinleştirilirken hata oluştu.");
                return Error("Feature flag etkinleştirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Disable feature flag
        /// </summary>
        [HttpPost("{featureName}/disable")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableFeatureFlag(
            [FromRoute] string featureName,
            [FromQuery] string? environment = null)
        {
            try
            {
                var result = await _featureFlagService.DisableFeatureFlagAsync(featureName, environment);
                if (!result)
                {
                    return BadRequest(new { message = $"Feature flag '{featureName}' devre dışı bırakılamadı." });
                }
                return Success(true, $"Feature flag '{featureName}' başarıyla devre dışı bırakıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feature flag '{featureName}' devre dışı bırakılırken hata oluştu.");
                return Error("Feature flag devre dışı bırakılırken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete feature flag
        /// </summary>
        [HttpDelete("{featureName}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteFeatureFlag(
            [FromRoute] string featureName,
            [FromQuery] string? environment = null)
        {
            try
            {
                var result = await _featureFlagService.DeleteFeatureFlagAsync(featureName, environment);
                if (!result)
                {
                    return BadRequest(new { message = $"Feature flag '{featureName}' silinemedi." });
                }
                return Success(true, $"Feature flag '{featureName}' başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Feature flag '{featureName}' silinirken hata oluştu.");
                return Error("Feature flag silinirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
