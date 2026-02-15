using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;

namespace GuestFlow.Application.Operations.Payment
{
    public class StripeSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
    }

    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;

        public StripePaymentService(
            IOptions<StripeSettings> settings,
            ILogger<StripePaymentService> logger,
            IPaymentService paymentService,
            IUnitOfWork unitOfWork)
        {
            _settings = settings.Value;
            _logger = logger;
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
            
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string paymentMethodId, int guestId, int? invoiceId = null)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Stripe uses cents
                    Currency = currency.ToLower(),
                    PaymentMethod = paymentMethodId,
                    ConfirmationMethod = "manual",
                    Confirm = true,
                    Metadata = new Dictionary<string, string>
                    {
                        { "guestId", guestId.ToString() },
                        { "invoiceId", invoiceId?.ToString() ?? "" }
                    }
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                // Create a pending payment record in our system
                var paymentDto = new GuestFlow.Application.Operations.Payment.Dtos.AddPaymentDto
                {
                    GuestId = guestId,
                    InvoiceId = invoiceId,
                    Amount = amount,
                    Currency = currency,
                    PaymentMethod = "CreditCard",
                    Status = "Pending",
                    TransactionId = intent.Id, // Link to Stripe Intent ID
                    Notes = $"Stripe Payment Intent Created: {intent.Id}",
                    PaymentDate = DateTime.UtcNow,
                    CollectedByPersonnelId = 1 // System/Admin for online payments
                };

                var result = await _paymentService.AddPaymentAsync(paymentDto);
                if (!result.IsSuccess)
                {
                    _logger.LogError("Failed to create internal payment record for Stripe Intent {IntentId}: {Error}", intent.Id, result.Message);
                }

                return intent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating PaymentIntent");
                throw;
            }
        }

        public async Task HandleWebhookAsync(string json, string stripeSignature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, _settings.WebhookSecret);

                if (stripeEvent.Type == Stripe.EventTypes.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogInformation("PaymentIntent succeeded: {Id}", paymentIntent.Id);
                        
                        // Find the payment record by transaction ID
                        var internalPayment = await _paymentService.GetPaymentByTransactionIdAsync(paymentIntent.Id);
                        if (internalPayment != null)
                        {
                            await _paymentService.CompletePaymentAsync(internalPayment.Id, paymentIntent.Id, json);
                        }
                        else
                        {
                            _logger.LogWarning("Received Stripe success webhook for unknown TransactionId: {Id}", paymentIntent.Id);
                        }
                    }
                }
                else if (stripeEvent.Type == Stripe.EventTypes.PaymentIntentPaymentFailed)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        _logger.LogWarning("PaymentIntent failed: {Id}", paymentIntent.Id);

                        var internalPayment = await _paymentService.GetPaymentByTransactionIdAsync(paymentIntent.Id);
                        if (internalPayment != null)
                        {
                            await _paymentService.FailPaymentAsync(internalPayment.Id, paymentIntent.LastPaymentError?.Message ?? "Stripe payment failed");
                        }
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling Stripe webhook");
                throw;
            }
        }
    }
}
