namespace GuestFlow.Application.Operations.Payment.Dtos
{
    /// <summary>
    /// Ödeme listesi DTO'su
    /// </summary>
    public class GetPaymentDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; }
        
        // Fatura bilgisi (opsiyonel)
        public int? InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        
        // Misafir bilgisi
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        
        // Tahsil eden personel bilgisi
        public int CollectedByPersonnelId { get; set; }
        public string CollectedByPersonnelName { get; set; }
        
        // Servis bağlantıları (opsiyonel)
        public int? TransferId { get; set; }
        public int? CityTourId { get; set; }
        public int? YachtTourId { get; set; }
        public string? ServiceType { get; set; } // "Transfer", "CityTour", "YachtTour", "General"
        
        // Ödeme detayları
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? RefundDate { get; set; }
        public string? RefundReason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

