using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Ödenmemiş servis detayı - PaymentEntity'den hesaplanır
    /// </summary>
    public class UnpaidServiceItemDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public int ServiceId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public int GuestId { get; set; }
        public string? RoomNumber { get; set; }
        public string? CityName { get; set; }
        
        /// <summary>
        /// Servis tutarı (FinalPrice)
        /// </summary>
        public decimal ServiceAmount { get; set; }
        
        /// <summary>
        /// Tahsil edilen tutar (PaymentEntity'den)
        /// </summary>
        public decimal PaidAmount { get; set; }
        
        /// <summary>
        /// Kalan tutar (ServiceAmount - PaidAmount)
        /// </summary>
        public decimal RemainingAmount { get; set; }
        
        public string? Currency { get; set; }
        public string? Status { get; set; }
        
        /// <summary>
        /// Ödeme durumu: Unpaid, PartiallyPaid, Paid
        /// </summary>
        public string PaymentStatus { get; set; } = "Unpaid";

        /// <summary>
        /// Kaç gün geciktiği (ServiceDate'ten itibaren)
        /// </summary>
        public int DaysOverdue { get; set; }

        // Geriye uyumluluk için
        [Obsolete("Use ServiceAmount instead")]
        public decimal Amount { get => ServiceAmount; set => ServiceAmount = value; }
    }

    public class UnpaidServicesDto
    {
        public IList<UnpaidServiceItemDto> Items { get; set; } = new List<UnpaidServiceItemDto>();

        /// <summary>
        /// Toplam kayıt sayısı (pagination için)
        /// </summary>
        public int TotalCount => Items?.Count ?? 0;

        /// <summary>
        /// Currency bazlı toplam kalan tutar
        /// </summary>
        public Dictionary<string, decimal> TotalRemainingByCurrency { get; set; } = new Dictionary<string, decimal>();

        /// <summary>
        /// Toplam ödenmemiş servis sayısı
        /// </summary>
        public int TotalUnpaidCount { get; set; }

        /// <summary>
        /// Kısmi ödemeli servis sayısı
        /// </summary>
        public int PartiallyPaidCount { get; set; }
    }
}
