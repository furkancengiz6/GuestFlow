namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// SMS durumları
    /// </summary>
    public enum SmsStatus
    {
        /// <summary>
        /// Beklemede - SMS gönderilmeyi bekliyor
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Gönderildi - SMS başarıyla gönderildi
        /// </summary>
        Sent = 2,

        /// <summary>
        /// Başarısız - SMS gönderilemedi
        /// </summary>
        Failed = 3,

        /// <summary>
        /// Teslim edildi - SMS alıcıya ulaştı
        /// </summary>
        Delivered = 4
    }

    /// <summary>
    /// SMS durumu yardımcı sınıfı
    /// </summary>
    public static class SmsStatusHelper
    {
        /// <summary>
        /// SMS durumunu string'e çevirir
        /// </summary>
        public static string ToString(SmsStatus status)
        {
            return status switch
            {
                SmsStatus.Pending => "Pending",
                SmsStatus.Sent => "Sent",
                SmsStatus.Failed => "Failed",
                SmsStatus.Delivered => "Delivered",
                _ => "Pending"
            };
        }

        /// <summary>
        /// String'i SmsStatus enum'una çevirir
        /// </summary>
        public static SmsStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return SmsStatus.Pending;

            return status.ToLower() switch
            {
                "pending" => SmsStatus.Pending,
                "sent" => SmsStatus.Sent,
                "failed" => SmsStatus.Failed,
                "delivered" => SmsStatus.Delivered,
                _ => SmsStatus.Pending
            };
        }

        /// <summary>
        /// SMS durumunun Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(SmsStatus status)
        {
            return status switch
            {
                SmsStatus.Pending => "Beklemede",
                SmsStatus.Sent => "Gönderildi",
                SmsStatus.Failed => "Başarısız",
                SmsStatus.Delivered => "Teslim Edildi",
                _ => "Beklemede"
            };
        }

        /// <summary>
        /// Geçerli bir SMS durumu olup olmadığını kontrol eder
        /// </summary>
        public static bool IsValidStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var validStatuses = new[] { "pending", "sent", "failed", "delivered" };
            return validStatuses.Contains(status.ToLower());
        }

        /// <summary>
        /// Tüm SMS durumlarını listeler
        /// </summary>
        public static List<string> GetAllStatuses()
        {
            return new List<string> { "Pending", "Sent", "Failed", "Delivered" };
        }
    }
}

