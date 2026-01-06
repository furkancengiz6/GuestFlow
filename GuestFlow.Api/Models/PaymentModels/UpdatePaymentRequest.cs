using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Ödeme güncelleme request modeli
    /// </summary>
    public class UpdatePaymentRequest
    {
        /// <summary>
        /// Fatura ID (sonradan bağlanabilir)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Transfer ID (sonradan bağlanabilir)
        /// </summary>
        public int? TransferId { get; set; }

        /// <summary>
        /// Şehir Turu ID (sonradan bağlanabilir)
        /// </summary>
        public int? CityTourId { get; set; }

        /// <summary>
        /// Yat Turu ID (sonradan bağlanabilir)
        /// </summary>
        public int? YachtTourId { get; set; }

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

