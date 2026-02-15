namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Yeni ödeme oluşturma DTO'su - Tahsilat kaydı için
    /// </summary>
    public class AddPaymentDto
    {
        /// <summary>
        /// Fatura ID (opsiyonel - ödeme fatura olmadan da kaydedilebilir)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Misafir ID
        /// </summary>
        public int GuestId { get; set; }

        /// <summary>
        /// Ödemeyi tahsil eden personel ID (zorunlu)
        /// </summary>
        public int CollectedByPersonnelId { get; set; }

        /// <summary>
        /// Transfer ID (opsiyonel - doğrudan servise bağlı ödeme için)
        /// </summary>
        public int? TransferId { get; set; }

        /// <summary>
        /// Şehir Turu ID (opsiyonel - doğrudan servise bağlı ödeme için)
        /// </summary>
        public int? CityTourId { get; set; }

        /// <summary>
        /// Yat Turu ID (opsiyonel - doğrudan servise bağlı ödeme için)
        /// </summary>
        public int? YachtTourId { get; set; }

        /// <summary>
        /// Ödeme tutarı
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Ödeme yöntemi (CreditCard, BankTransfer, Cash, RoomCharge, Other)
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Ödeme tarihi (tahsilat anı)
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Notlar
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Ödeme durumu (Pending, Completed, etc.) - Opsiyonel, varsayılan Completed (tahsilat için)
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// İşlem ID (Stripe Intent ID, Banka Ref no vb.)
        /// </summary>
        public string? TransactionId { get; set; }
    }
}

