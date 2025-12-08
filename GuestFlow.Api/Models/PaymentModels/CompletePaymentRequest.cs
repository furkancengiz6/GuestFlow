using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Ödeme tamamlama request modeli
    /// </summary>
    public class CompletePaymentRequest
    {
        [Required(ErrorMessage = "Transaction ID gereklidir.")]
        public string TransactionId { get; set; }

        public string? GatewayResponse { get; set; }
    }
}

