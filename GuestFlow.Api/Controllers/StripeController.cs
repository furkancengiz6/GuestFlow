using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GuestFlow.Application.Operations.Payment;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly IStripePaymentService _stripeService;

        public StripeController(IStripePaymentService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var intent = await _stripeService.CreatePaymentIntentAsync(
                request.Amount, 
                request.Currency, 
                request.PaymentMethodId, 
                request.GuestId, 
                request.InvoiceId);

            return Ok(new { clientSecret = intent.ClientSecret });
        }
    }

    public class CreatePaymentIntentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string PaymentMethodId { get; set; } = string.Empty;
        public int GuestId { get; set; }
        public int? InvoiceId { get; set; }
    }
}
