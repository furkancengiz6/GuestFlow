namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// İtinerary item tipleri
    /// </summary>
    public enum ItineraryItemType
    {
        /// <summary>
        /// Transfer
        /// </summary>
        Transfer = 1,

        /// <summary>
        /// Şehir turu
        /// </summary>
        CityTour = 2,

        /// <summary>
        /// Yat turu
        /// </summary>
        YachtTour = 3,

        /// <summary>
        /// Restoran rezervasyonu
        /// </summary>
        RestaurantReservation = 4,

        /// <summary>
        /// Diğer
        /// </summary>
        Other = 5
    }

    /// <summary>
    /// İtinerary item tipi yardımcı sınıfı
    /// </summary>
    public static class ItineraryItemTypeHelper
    {
        /// <summary>
        /// Tipi string'e çevirir
        /// </summary>
        public static string ToString(ItineraryItemType type)
        {
            return type switch
            {
                ItineraryItemType.Transfer => "Transfer",
                ItineraryItemType.CityTour => "CityTour",
                ItineraryItemType.YachtTour => "YachtTour",
                ItineraryItemType.RestaurantReservation => "RestaurantReservation",
                ItineraryItemType.Other => "Other",
                _ => "Other"
            };
        }

        /// <summary>
        /// String'i ItineraryItemType enum'una çevirir
        /// </summary>
        public static ItineraryItemType FromString(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return ItineraryItemType.Other;

            return type.ToLower() switch
            {
                "transfer" => ItineraryItemType.Transfer,
                "citytour" or "city_tour" => ItineraryItemType.CityTour,
                "yachttour" or "yacht_tour" => ItineraryItemType.YachtTour,
                "restaurantreservation" or "restaurant_reservation" => ItineraryItemType.RestaurantReservation,
                "other" or "diğer" => ItineraryItemType.Other,
                _ => ItineraryItemType.Other
            };
        }

        /// <summary>
        /// Tipin Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(ItineraryItemType type)
        {
            return type switch
            {
                ItineraryItemType.Transfer => "Transfer",
                ItineraryItemType.CityTour => "Şehir Turu",
                ItineraryItemType.YachtTour => "Yat Turu",
                ItineraryItemType.RestaurantReservation => "Restoran Rezervasyonu",
                ItineraryItemType.Other => "Diğer",
                _ => "Diğer"
            };
        }
    }
}

