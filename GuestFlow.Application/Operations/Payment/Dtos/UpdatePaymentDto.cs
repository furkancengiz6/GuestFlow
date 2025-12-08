namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Ödeme güncelleme DTO'su
    /// </summary>
    public class UpdatePaymentDto
    {
        /// <summary>
        /// Ödeme ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Ödeme yöntemi
        /// </summary>
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// Ödeme durumu
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Ödeme tarihi
        /// </summary>
        public DateTime? PaymentDate { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }
    }
}

