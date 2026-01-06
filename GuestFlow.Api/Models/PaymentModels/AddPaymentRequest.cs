using System;
using System.ComponentModel.DataAnnotations;

namespace GuestFlow.Api.Models.PaymentModels
{
    /// <summary>
    /// Yeni ödeme/tahsilat oluşturma request modeli
    /// </summary>
    public class AddPaymentRequest
    {
        /// <summary>
        /// Fatura ID (opsiyonel - ödeme fatura olmadan da kaydedilebilir)
        /// </summary>
        public int? InvoiceId { get; set; }

        /// <summary>
        /// Misafir ID (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Misafir ID gereklidir.")]
        public int GuestId { get; set; }

        /// <summary>
        /// Ödemeyi tahsil eden personel ID (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Tahsil eden personel ID gereklidir.")]
        public int CollectedByPersonnelId { get; set; }

        /// <summary>
        /// Transfer ID (opsiyonel - doğrudan servise bağlı ödeme için)
        /// </summary>
        public int? TransferId { get; set; }

        /// <summary>
        /// Şehir Turu ID (opsiyonel)
        /// </summary>
        public int? CityTourId { get; set; }

        /// <summary>
        /// Yat Turu ID (opsiyonel)
        /// </summary>
        public int? YachtTourId { get; set; }

        /// <summary>
        /// Ödeme tutarı (zorunlu)
        /// </summary>
        [Required(ErrorMessage = "Ödeme tutarı gereklidir.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Para birimi
        /// </summary>
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Para birimi 3 karakter olmalıdır.")]
        public string Currency { get; set; } = "TRY";

        /// <summary>
        /// Ödeme yöntemi (CreditCard, BankTransfer, Cash, RoomCharge, Other)
        /// </summary>
        [Required(ErrorMessage = "Ödeme yöntemi gereklidir.")]
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Ödeme tarihi (tahsilat anı)
        /// </summary>
        [Required(ErrorMessage = "Ödeme tarihi gereklidir.")]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Notlar (opsiyonel)
        /// </summary>
        public string? Notes { get; set; }
    }
}

