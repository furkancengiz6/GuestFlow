namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Yeni ödeme oluşturma DTO'su
    /// </summary>
    public class AddPaymentDto
    {
        /// <summary>
        /// Fatura ID
        /// </summary>
        public int InvoiceId { get; set; }

        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Ödeme yöntemi (CreditCard, BankTransfer, Cash, Other)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Ödeme tarihi
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }
    }
}

