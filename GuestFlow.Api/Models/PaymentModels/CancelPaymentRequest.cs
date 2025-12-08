namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Ödeme iptal request modeli
    /// </summary>
    public class CancelPaymentRequest
    {
        public string? CancellationReason { get; set; }
    }
}

