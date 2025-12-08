namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Ödeme detay DTO'su
    /// </summary>
    public class PaymentDetailDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        public int InvoiceId { get; set; }
        public int InvoiceNumber { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string InvoiceCurrency { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string GuestEmail { get; set; }
        public string GuestPhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayResponse { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? RefundReason { get; set; }
        public string? CancellationReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}

