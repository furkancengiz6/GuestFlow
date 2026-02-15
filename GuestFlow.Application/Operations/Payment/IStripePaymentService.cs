using System.Threading.Tasks;
using Stripe;

namespace GuestFlow.Application.Operations.Payment
{
    public interface IStripePaymentService
    {
        /// <summary>
        /// Creates a PaymentIntent for a specific amount and currency.
        /// </summary>
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string paymentMethodId, int guestId, int? invoiceId = null);

        /// <summary>
        /// Handles the Stripe webhook event for asynchronous payment updates.
        /// </summary>
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}
