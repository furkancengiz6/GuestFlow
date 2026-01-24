using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Login audit and security monitoring API endpoints
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner")] // Only Admin and Owner can access login audit
    [Tags("Güvenlik - Login Audit")]
    public class LoginAuditController : BaseController
    {
        private readonly ILoginAuditService _loginAuditService;
        private readonly ILogger<LoginAuditController> _logger;

        public LoginAuditController(
            ILoginAuditService loginAuditService,
            ILogger<LoginAuditController> logger)
        {
            _loginAuditService = loginAuditService;
            _logger = logger;
        }

        /// <summary>
        /// Get login attempts with filtering
        /// </summary>
        [HttpGet("attempts")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetLoginAttempts(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? email = null,
            [FromQuery] string? ipAddress = null,
            [FromQuery] bool? isSuccessful = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                var result = await _loginAuditService.GetLoginAttemptsAsync(
                    startDate, endDate, email, ipAddress, isSuccessful, personnelId, pageNumber, pageSize);
                return Success(result, "Login denemeleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login denemeleri getirilirken hata oluştu.");
                return Error("Login denemeleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get login audit statistics
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _loginAuditService.GetStatisticsAsync(startDate, endDate);
                return Success(result, "Login audit istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login audit istatistikleri getirilirken hata oluştu.");
                return Error("Login audit istatistikleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get failed login summary (top failed attempts by email)
        /// </summary>
        [HttpGet("failed-summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetFailedLoginSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? topCount = null)
        {
            try
            {
                var result = await _loginAuditService.GetFailedLoginSummaryAsync(startDate, endDate, topCount);
                return Success(result, "Başarısız login özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başarısız login özeti getirilirken hata oluştu.");
                return Error("Başarısız login özeti getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
