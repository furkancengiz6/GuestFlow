namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Rezervasyon durumları
    /// </summary>
    public enum ReservationStatus
    {
        /// <summary>
        /// Beklemede - Rezervasyon oluşturuldu, onay bekliyor
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Onaylandı - Rezervasyon onaylandı
        /// </summary>
        Confirmed = 2,

        /// <summary>
        /// İptal edildi - Rezervasyon iptal edildi
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// Tamamlandı - Rezervasyon tamamlandı
        /// </summary>
        Completed = 4
    }

    /// <summary>
    /// Rezervasyon durumu yardımcı sınıfı
    /// </summary>
    public static class ReservationStatusHelper
    {
        /// <summary>
        /// Rezervasyon durumunu string'e çevirir
        /// </summary>
        public static string ToString(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Pending => "Pending",
                ReservationStatus.Confirmed => "Confirmed",
                ReservationStatus.Cancelled => "Cancelled",
                ReservationStatus.Completed => "Completed",
                _ => "Pending"
            };
        }

        /// <summary>
        /// String'i ReservationStatus enum'una çevirir
        /// </summary>
        public static ReservationStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return ReservationStatus.Pending;

            return status.ToLower() switch
            {
                "pending" => ReservationStatus.Pending,
                "confirmed" => ReservationStatus.Confirmed,
                "cancelled" or "canceled" => ReservationStatus.Cancelled,
                "completed" => ReservationStatus.Completed,
                _ => ReservationStatus.Pending
            };
        }

        /// <summary>
        /// Rezervasyon durumunun Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(ReservationStatus status)
        {
            return status switch
            {
                ReservationStatus.Pending => "Beklemede",
                ReservationStatus.Confirmed => "Onaylandı",
                ReservationStatus.Cancelled => "İptal Edildi",
                ReservationStatus.Completed => "Tamamlandı",
                _ => "Beklemede"
            };
        }
    }
}

