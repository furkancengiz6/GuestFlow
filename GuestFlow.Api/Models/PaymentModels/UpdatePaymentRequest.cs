using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Ödeme güncelleme request modeli
    /// </summary>
    public class UpdatePaymentRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
        public decimal? Amount { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi 3 karakter olmalıdır.")]
        public string? Currency { get; set; }

        public string? PaymentMethod { get; set; }

        public string? Status { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? TransactionId { get; set; }

        public string? Notes { get; set; }
    }
}

