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
        private readonly IOTAChannelManagerService _channelManagerService;
        private readonly IOTAReservationMappingService _mappingService;

        public OTAController(
            IOTAIntegrationService otaService,
            IOTAChannelManagerService channelManagerService,
            IOTAReservationMappingService mappingService)
        {
            _otaService = otaService;
            _channelManagerService = channelManagerService;
            _mappingService = mappingService;
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
        public async Task<IActionResult> ProcessWebhook(
            string providerCode, 
            [FromBody] object payload,
            [FromHeader(Name = "X-OTA-Signature")] string? signature = null,
            [FromHeader(Name = "X-Booking-Signature")] string? bookingSignature = null,
            [FromHeader(Name = "X-Expedia-Signature")] string? expediaSignature = null,
            [FromHeader(Name = "X-Idempotency-Key")] string? idempotencyKey = null,
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKeyAlt = null)
        {
            if (payload is null)
                return BadRequest(new ApiResponse<bool> { Success = false, Message = "Webhook payload is required." });

            // Provider'a göre signature header'ını belirle
            var webhookSignature = signature ?? bookingSignature ?? expediaSignature;
            
            // Idempotency key - X-Idempotency-Key veya Idempotency-Key header'ından al
            var idempotencyKeyValue = idempotencyKey ?? idempotencyKeyAlt;

            // IP adresi ve User Agent bilgilerini al
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var result = await _otaService.ProcessWebhookAsync(
                providerCode, 
                payload.ToString() ?? string.Empty, 
                webhookSignature,
                idempotencyKeyValue,
                ipAddress,
                userAgent);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Channel Manager Endpoints
        [HttpPost("channel-manager/sync-availability")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncAvailabilityFromPMS([FromQuery] int pmsIntegrationId, [FromQuery] DateTime? date = null)
        {
            var result = await _channelManagerService.SyncAvailabilityFromPMSToOTAsAsync(pmsIntegrationId, date);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Dead-letter queue'daki webhook'ları listele
        /// </summary>
        [HttpGet("webhook/dead-letter")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> GetDeadLetterWebhooks(
            [FromQuery] string? providerCode = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _otaService.GetDeadLetterWebhooksAsync(providerCode, pageNumber, pageSize);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Dead-letter queue'daki webhook'u manuel olarak retry et
        /// </summary>
        [HttpPost("webhook/dead-letter/{webhookLogId}/retry")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> RetryDeadLetterWebhook(int webhookLogId)
        {
            var result = await _otaService.RetryDeadLetterWebhookAsync(webhookLogId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("channel-manager/sync-rates")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncRatesFromPMS([FromQuery] int pmsIntegrationId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _channelManagerService.SyncRatesFromPMSToOTAsAsync(pmsIntegrationId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("channel-manager/sync-all")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> SyncAllActiveIntegrations()
        {
            var result = await _channelManagerService.SyncAllActiveIntegrationsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("channel-manager/stop-sell")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> BroadcastStopSell([FromQuery] int hotelId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _channelManagerService.BroadcastStopSellAsync(hotelId, startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // Reservation Mapping & Conflict Resolution Endpoints
        [HttpPost("reservations/{otaReservationId}/map/{guestFlowReservationId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> MapReservation(int otaReservationId, int guestFlowReservationId)
        {
            var result = await _mappingService.MapOTAReservationToGuestFlowAsync(otaReservationId, guestFlowReservationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("reservations/{otaReservationId}/conflict")]
        [ProducesResponseType(typeof(ApiResponse<OTAReservationConflict>), 200)]
        public async Task<IActionResult> CheckConflict(int otaReservationId)
        {
            var result = await _mappingService.CheckConflictAsync(otaReservationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("conflicts/{conflictId}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        public async Task<IActionResult> ResolveConflict(int conflictId, [FromBody] ConflictResolutionStrategy strategy)
        {
            var result = await _mappingService.ResolveConflictAsync(conflictId, strategy);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("conflicts")]
        [ProducesResponseType(typeof(ApiResponse<List<OTAReservationConflict>>), 200)]
        public async Task<IActionResult> GetAllConflicts()
        {
            var result = await _mappingService.GetAllConflictsAsync();
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("mappings")]
        [ProducesResponseType(typeof(ApiResponse<List<OTAReservationMapping>>), 200)]
        public async Task<IActionResult> GetMappings([FromQuery] int? otaIntegrationId = null)
        {
            var result = await _mappingService.GetMappingsAsync(otaIntegrationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}