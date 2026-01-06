namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// İtinerary durumları
    /// </summary>
    public enum ItineraryStatus
    {
        /// <summary>
        /// Taslak
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Onaylandı
        /// </summary>
        Confirmed = 2,

        /// <summary>
        /// Devam ediyor
        /// </summary>
        InProgress = 3,

        /// <summary>
        /// Tamamlandı
        /// </summary>
        Completed = 4,

        /// <summary>
        /// İptal edildi
        /// </summary>
        Cancelled = 5
    }

    /// <summary>
    /// İtinerary durumu yardımcı sınıfı
    /// </summary>
    public static class ItineraryStatusHelper
    {
        /// <summary>
        /// Durumu string'e çevirir
        /// </summary>
        public static string ToString(ItineraryStatus status)
        {
            return status switch
            {
                ItineraryStatus.Draft => "Draft",
                ItineraryStatus.Confirmed => "Confirmed",
                ItineraryStatus.InProgress => "InProgress",
                ItineraryStatus.Completed => "Completed",
                ItineraryStatus.Cancelled => "Cancelled",
                _ => "Draft"
            };
        }

        /// <summary>
        /// String'i ItineraryStatus enum'una çevirir
        /// </summary>
        public static ItineraryStatus FromString(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return ItineraryStatus.Draft;

            return status.ToLower() switch
            {
                "draft" or "taslak" => ItineraryStatus.Draft,
                "confirmed" or "onaylandı" => ItineraryStatus.Confirmed,
                "inprogress" or "in_progress" or "devam ediyor" => ItineraryStatus.InProgress,
                "completed" or "tamamlandı" => ItineraryStatus.Completed,
                "cancelled" or "iptal" => ItineraryStatus.Cancelled,
                _ => ItineraryStatus.Draft
            };
        }

        /// <summary>
        /// Durumun Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(ItineraryStatus status)
        {
            return status switch
            {
                ItineraryStatus.Draft => "Taslak",
                ItineraryStatus.Confirmed => "Onaylandı",
                ItineraryStatus.InProgress => "Devam Ediyor",
                ItineraryStatus.Completed => "Tamamlandı",
                ItineraryStatus.Cancelled => "İptal Edildi",
                _ => "Taslak"
            };
        }
    }
}

