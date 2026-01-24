using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Authorization;
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
    /// Permissions and role-permission matrix API endpoints
    /// Provides centralized permission management for UI and API
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner")] // Only Admin and Owner can manage permissions
    [Tags("Platform - Permissions")]
    public class PermissionsController : BaseController
    {
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IPermissionService permissionService,
            ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _logger = logger;
        }

        /// <summary>
        /// Check if current user has permission
        /// </summary>
        [HttpGet("check/{permissionCode}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckPermission([FromRoute] string permissionCode)
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == "UserType" || c.Type == "role")?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

                var hasPermission = await _permissionService.HasPermissionAsync(userRole, permissionCode);
                return Success(hasPermission, $"Permission '{permissionCode}' check completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Permission check failed for '{permissionCode}'");
                return Error("Permission kontrolü sırasında bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all permissions for current user's role
        /// </summary>
        [HttpGet("my-permissions")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyPermissions()
        {
            try
            {
                var userRole = User.Claims.FirstOrDefault(c => c.Type == "UserType" || c.Type == "role")?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

                var permissions = await _permissionService.GetPermissionsForRoleAsync(userRole);
                return Success(permissions, "Kullanıcı izinleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "User permissions getirilirken hata oluştu.");
                return Error("Kullanıcı izinleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get all permissions (for UI generation)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllPermissions()
        {
            try
            {
                var result = await _permissionService.GetAllPermissionsAsync();
                return Success(result, "Tüm izinler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Permissions getirilirken hata oluştu.");
                return Error("İzinler getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get role-permission matrix (for UI generation)
        /// </summary>
        [HttpGet("matrix")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolePermissionMatrix()
        {
            try
            {
                var result = await _permissionService.GetRolePermissionMatrixAsync();
                return Success(result, "Role-permission matrisi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Role-permission matrisi getirilirken hata oluştu.");
                return Error("Role-permission matrisi getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get permissions for a specific role
        /// </summary>
        [HttpGet("role/{roleName}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPermissionsForRole([FromRoute] string roleName)
        {
            try
            {
                var result = await _permissionService.GetPermissionsForRoleAsync(roleName);
                return Success(result, $"Role '{roleName}' izinleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Role '{roleName}' izinleri getirilirken hata oluştu.");
                return Error("Role izinleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Assign permission to role
        /// </summary>
        [HttpPost("assign")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPermission([FromBody] AssignPermissionRequest request)
        {
            try
            {
                var result = await _permissionService.AssignPermissionToRoleAsync(request.RoleName, request.PermissionCode);
                if (!result)
                {
                    return BadRequest(new { message = "İzin atanamadı." });
                }
                return Success(true, "İzin başarıyla atandı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İzin atanırken hata oluştu.");
                return Error("İzin atanırken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Remove permission from role
        /// </summary>
        [HttpPost("remove")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RemovePermission([FromBody] AssignPermissionRequest request)
        {
            try
            {
                var result = await _permissionService.RemovePermissionFromRoleAsync(request.RoleName, request.PermissionCode);
                if (!result)
                {
                    return BadRequest(new { message = "İzin kaldırılamadı." });
                }
                return Success(true, "İzin başarıyla kaldırıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İzin kaldırılırken hata oluştu.");
                return Error("İzin kaldırılırken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Initialize default permissions (seed data)
        /// </summary>
        [HttpPost("initialize")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> InitializeDefaultPermissions()
        {
            try
            {
                await _permissionService.InitializeDefaultPermissionsAsync();
                return Success(true, "Varsayılan izinler başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan izinler oluşturulurken hata oluştu.");
                return Error("Varsayılan izinler oluşturulurken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }
    }

    /// <summary>
    /// Assign permission request DTO
    /// </summary>
    public class AssignPermissionRequest
    {
        public string RoleName { get; set; } = string.Empty;
        public string PermissionCode { get; set; } = string.Empty;
    }
}
