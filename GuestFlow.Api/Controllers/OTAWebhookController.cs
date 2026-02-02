using GuestFlow.Application.Operations.OTA;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/ota")]
    [ApiVersion("1.0")]
    public class OTAWebhookController : ControllerBase
    {
        private readonly IOTAIntegrationService _otaIntegrationService;

        private readonly ILogger<OTAWebhookController> _logger;

        public OTAWebhookController(
            IOTAIntegrationService otaIntegrationService,
            ILogger<OTAWebhookController> logger)
        {
            _otaIntegrationService = otaIntegrationService;
            _logger = logger;
        }

        [HttpPost("booking/webhook")]
        public async Task<IActionResult> HandleBookingWebhook(
            [FromBody] JsonElement payload,
            [FromHeader(Name = "X-Booking-Signature")] string signature)
        {
            try
            {
                var jsonString = payload.ToString();
                


                // 2. Process generically via IntegrationService (which logs, checks idempotency etc)
                // 'BKG' is the provider code for Booking.com
                var result = await _otaIntegrationService.ProcessWebhookAsync("BKG", jsonString, signature);

                if (result.Success)
                    return Ok(new { status = "processed" });

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Booking webhook");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
