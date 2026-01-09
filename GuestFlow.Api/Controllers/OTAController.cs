using GuestFlow.Application.Models.Requests.OTA;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Application.Models;
using GuestFlow.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OTAController : BaseController
    {
        private readonly IOTAIntegrationService _otaService;

        public OTAController(IOTAIntegrationService otaService)
        {
            _otaService = otaService;
        }

        [HttpGet("integrations")]
        [ProducesResponseType(typeof(ApiResponse<List<OTAIntegration>>), 200)]
        public async Task<IActionResult> GetAllIntegrations()
        {
            var result = await _otaService.GetAllOTAIntegrationsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("integrations")]
        [ProducesResponseType(typeof(ApiResponse<OTAIntegration>), 201)]
        public async Task<IActionResult> CreateIntegration([FromBody] CreateOTAIntegrationRequest request)
        {
            var result = await _otaService.CreateOTAIntegrationAsync(request);
            return result.Success ? CreatedAtAction(nameof(GetAllIntegrations), result) : BadRequest(result);
        }

        [HttpPost("integrations/{integrationId}/test-connection")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> TestConnection(int integrationId)
        {
            var result = await _otaService.TestOTAConnectionAsync(integrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("integrations/{integrationId}/sync-reservations")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncReservations(int integrationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _otaService.SyncReservationsAsync(integrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("integrations/{integrationId}/hotels/{hotelId}/prices")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> UpdateRoomPrices(int integrationId, int hotelId, [FromBody] List<PriceUpdateRequest> prices)
        {
            var result = await _otaService.UpdateRoomPricesAsync(integrationId, hotelId, prices);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("integrations/{integrationId}/pending-reservations")]
        [ProducesResponseType(typeof(ApiResponse<List<OTAReservation>>), 200)]
        public async Task<IActionResult> GetPendingReservations(int integrationId)
        {
            var result = await _otaService.GetPendingReservationsAsync(integrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("webhook/{providerCode}")]
        [AllowAnonymous] // Webhooks come from external services
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ProcessWebhook(string providerCode, [FromBody] object payload)
        {
            var result = await _otaService.ProcessWebhookAsync(providerCode, payload.ToString());
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}