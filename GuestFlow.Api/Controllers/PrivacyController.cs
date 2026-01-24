using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses.Privacy;
using GuestFlow.Application.Operations.Privacy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Privacy and PII management API endpoints (KVKK/GDPR compliance)
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Owner")] // Only Admin and Owner can manage privacy
    [Tags("Güvenlik - PII Yönetimi")]
    public class PrivacyController : BaseController
    {
        private readonly IPIIManagementService _piiManagementService;
        private readonly ILogger<PrivacyController> _logger;

        public PrivacyController(
            IPIIManagementService piiManagementService,
            ILogger<PrivacyController> logger)
        {
            _piiManagementService = piiManagementService;
            _logger = logger;
        }

        /// <summary>
        /// Mask email address for display
        /// </summary>
        [HttpPost("mask/email")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public IActionResult MaskEmail([FromBody] MaskRequest request)
        {
            try
            {
                var masked = _piiManagementService.MaskEmail(request.Value);
                return Success(masked, "Email başarıyla maskelendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email maskelenirken hata oluştu.");
                return Error("Email maskelenirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Mask phone number for display
        /// </summary>
        [HttpPost("mask/phone")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        public IActionResult MaskPhone([FromBody] MaskRequest request)
        {
            try
            {
                var masked = _piiManagementService.MaskPhone(request.Value);
                return Success(masked, "Telefon numarası başarıyla maskelendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Telefon numarası maskelenirken hata oluştu.");
                return Error("Telefon numarası maskelenirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Anonymize guest data (KVKK/GDPR right to be forgotten)
        /// </summary>
        [HttpPost("anonymize-guest")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AnonymizeGuest([FromBody] AnonymizeGuestRequest request)
        {
            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                int? requestedByPersonnelId = null;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    requestedByPersonnelId = userId;
                }

                var result = await _piiManagementService.AnonymizeGuestAsync(
                    request.GuestId, 
                    request.Reason, 
                    requestedByPersonnelId);

                if (!result)
                {
                    return BadRequest(new { message = "Misafir verisi anonymize edilemedi. Misafir bulunamadı veya zaten anonymize edilmiş." });
                }

                _logger.LogWarning($"Guest {request.GuestId} anonymized by user {requestedByPersonnelId}. Reason: {request.Reason}");
                return Success(true, "Misafir verisi başarıyla anonymize edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafir verisi anonymize edilirken hata oluştu.");
                return Error("Misafir verisi anonymize edilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Delete guest data (hard delete - use with caution)
        /// </summary>
        [HttpPost("delete-guest")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteGuest([FromBody] DeleteGuestRequest request)
        {
            try
            {
                if (!request.ConfirmDeletion)
                {
                    return BadRequest(new { message = "Silme işlemini onaylamak için ConfirmDeletion=true gönderilmelidir." });
                }

                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                int? requestedByPersonnelId = null;
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    requestedByPersonnelId = userId;
                }

                var result = await _piiManagementService.DeleteGuestDataAsync(
                    request.GuestId, 
                    request.Reason, 
                    requestedByPersonnelId);

                if (!result)
                {
                    return BadRequest(new { message = "Misafir verisi silinemedi. Misafir bulunamadı." });
                }

                _logger.LogWarning($"Guest {request.GuestId} deleted by user {requestedByPersonnelId}. Reason: {request.Reason}");
                return Success(true, "Misafir verisi başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafir verisi silinirken hata oluştu.");
                return Error("Misafir verisi silinirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Get privacy action history
        /// </summary>
        [HttpGet("history")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrivacyActionHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null)
        {
            try
            {
                var result = await _piiManagementService.GetPrivacyActionHistoryAsync(startDate, endDate, guestId);
                return Success(result, "Privacy action history başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Privacy action history getirilirken hata oluştu.");
                return Error("Privacy action history getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Check if guest is anonymized
        /// </summary>
        [HttpGet("check-anonymized/{guestId:int}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckAnonymized([FromRoute] int guestId)
        {
            try
            {
                var result = await _piiManagementService.IsGuestAnonymizedAsync(guestId);
                return Success(result, "Anonymization durumu başarıyla kontrol edildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Anonymization durumu kontrol edilirken hata oluştu.");
                return Error("Anonymization durumu kontrol edilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }
    }

    /// <summary>
    /// Mask request DTO
    /// </summary>
    public class MaskRequest
    {
        public string Value { get; set; } = string.Empty;
    }
}
