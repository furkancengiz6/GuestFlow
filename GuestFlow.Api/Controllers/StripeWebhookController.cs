using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GuestFlow.Application.Operations.Payment;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IStripePaymentService _stripeService;

        public StripeWebhookController(IStripePaymentService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var stripeSignature = Request.Headers["Stripe-Signature"].ToString() ?? string.Empty;

            try
            {
                await _stripeService.HandleWebhookAsync(json, stripeSignature);
                return Ok();
            }
            catch (System.Exception)
            {
                return BadRequest();
            }
        }
    }
}
