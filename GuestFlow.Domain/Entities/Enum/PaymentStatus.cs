namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Ödeme durumları
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>
        /// Beklemede - Ödeme başlatıldı, işlem bekliyor
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Tamamlandı - Ödeme başarıyla tamamlandı
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Başarısız - Ödeme işlemi başarısız oldu
        /// </summary>
        Failed = 3,

        /// <summary>
        /// İade edildi - Ödeme iade edildi
        /// </summary>
        Refunded = 4,

        /// <summary>
        /// İptal edildi - Ödeme iptal edildi
        /// </summary>
        Cancelled = 5
    }

    /// <summary>
    /// Ödeme durumu yardımcı sınıfı
    /// </summary>
    public static class PaymentStatusHelper
    {
        /// <summary>
        /// Ödeme durumunu string'e çevirir
        /// </summary>
        public static string ToString(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Completed => "Completed",
                PaymentStatus.Failed => "Failed",
                PaymentStatus.Refunded => "Refunded",
                PaymentStatus.Cancelled => "Cancelled",
                _ => "Pending"
            };
        }

        /// <summary>
        /// String'i PaymentStatus enum'una çevirir
        /// </summary>
        public static PaymentStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return PaymentStatus.Pending;

            return status.ToLower() switch
            {
                "pending" => PaymentStatus.Pending,
                "completed" => PaymentStatus.Completed,
                "failed" => PaymentStatus.Failed,
                "refunded" => PaymentStatus.Refunded,
                "cancelled" or "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };
        }

        /// <summary>
        /// Ödeme durumunun Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Pending => "Beklemede",
                PaymentStatus.Completed => "Tamamlandı",
                PaymentStatus.Failed => "Başarısız",
                PaymentStatus.Refunded => "İade Edildi",
                PaymentStatus.Cancelled => "İptal Edildi",
                _ => "Beklemede"
            };
        }

        /// <summary>
        /// Geçerli bir ödeme durumu olup olmadığını kontrol eder
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var validStatuses = new[] { "pending", "completed", "failed", "refunded", "cancelled", "canceled" };
            return validStatuses.Contains(status.ToLower());
        }

        /// <summary>
        /// Tüm ödeme durumlarını listeler
        /// </summary>
        public static List<string> GetAllStatuses()
        {
            return new List<string> { "Pending", "Completed", "Failed", "Refunded", "Cancelled" };
        }
    }
}

