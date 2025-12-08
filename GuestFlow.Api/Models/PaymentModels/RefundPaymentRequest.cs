namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Ödeme iade request modeli
    /// </summary>
    public class RefundPaymentRequest
    {
        public string? RefundReason { get; set; }
    }
}

