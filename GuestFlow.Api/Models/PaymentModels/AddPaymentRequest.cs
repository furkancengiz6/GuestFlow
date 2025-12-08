using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Yeni ödeme oluşturma request modeli
    /// </summary>
    public class AddPaymentRequest
    {
        [Required(ErrorMessage = "Fatura ID gereklidir.")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        [Required(ErrorMessage = "Ödeme tutarı gereklidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi 3 karakter olmalıdır.")]
        public string Currency { get; set; } = "TRY";

        [Required(ErrorMessage = "Ödeme yöntemi gereklidir.")]
        public string PaymentMethod { get; set; }

        [Required(ErrorMessage = "Ödeme tarihi gereklidir.")]
        public DateTime PaymentDate { get; set; }

        public string? Notes { get; set; }
    }
}

