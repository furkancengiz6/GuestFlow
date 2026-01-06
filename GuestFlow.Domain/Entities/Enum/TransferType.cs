namespace GuestFlow.Domain.Entities.Enum
{
    /// <summary>
    /// Transfer tipleri
    /// </summary>
    public enum TransferType
    {
        /// <summary>
        /// Havalimanı → Otel
        /// </summary>
        AirportToHotel = 1,

        /// <summary>
        /// Otel → Havalimanı
        /// </summary>
        HotelToAirport = 2,

        /// <summary>
        /// Otel → Restoran
        /// </summary>
        HotelToRestaurant = 3,

        /// <summary>
        /// Restoran → Otel
        /// </summary>
        RestaurantToHotel = 4,

        /// <summary>
        /// Otel → Şehir (genel)
        /// </summary>
        HotelToCity = 5,

        /// <summary>
        /// Şehir → Otel
        /// </summary>
        CityToHotel = 6,

        /// <summary>
        /// Otel → Otel
        /// </summary>
        HotelToHotel = 7,

        /// <summary>
        /// Özel transfer
        /// </summary>
        Custom = 8
    }

    /// <summary>
    /// Transfer tipi yardımcı sınıfı
    /// </summary>
    public static class TransferTypeHelper
    {
        /// <summary>
        /// Transfer tipini string'e çevirir
        /// </summary>
        public static string ToString(TransferType type)
        {
            return type switch
            {
                TransferType.AirportToHotel => "AirportToHotel",
                TransferType.HotelToAirport => "HotelToAirport",
                TransferType.HotelToRestaurant => "HotelToRestaurant",
                TransferType.RestaurantToHotel => "RestaurantToHotel",
                TransferType.HotelToCity => "HotelToCity",
                TransferType.CityToHotel => "CityToHotel",
                TransferType.HotelToHotel => "HotelToHotel",
                TransferType.Custom => "Custom",
                _ => "Custom"
            };
        }

        /// <summary>
        /// String'i TransferType enum'una çevirir
        /// </summary>
        public static TransferType FromString(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return TransferType.Custom;

            return type.ToLower() switch
            {
                "airporttohotel" or "airport_to_hotel" or "airport-hotel" => TransferType.AirportToHotel,
                "hoteltoairport" or "hotel_to_airport" or "hotel-airport" => TransferType.HotelToAirport,
                "hoteltorestaurant" or "hotel_to_restaurant" or "hotel-restaurant" => TransferType.HotelToRestaurant,
                "restauranttohotel" or "restaurant_to_hotel" or "restaurant-hotel" => TransferType.RestaurantToHotel,
                "hoteltocity" or "hotel_to_city" or "hotel-city" => TransferType.HotelToCity,
                "citytohotel" or "city_to_hotel" or "city-hotel" => TransferType.CityToHotel,
                "hoteltohotel" or "hotel_to_hotel" or "hotel-hotel" => TransferType.HotelToHotel,
                "custom" or "özel" => TransferType.Custom,
                _ => TransferType.Custom
            };
        }

        /// <summary>
        /// Transfer tipinin Türkçe karşılığını döndürür
        /// </summary>
        public static string GetTurkishName(TransferType type)
        {
            return type switch
            {
                TransferType.AirportToHotel => "Havalimanı → Otel",
                TransferType.HotelToAirport => "Otel → Havalimanı",
                TransferType.HotelToRestaurant => "Otel → Restoran",
                TransferType.RestaurantToHotel => "Restoran → Otel",
                TransferType.HotelToCity => "Otel → Şehir",
                TransferType.CityToHotel => "Şehir → Otel",
                TransferType.HotelToHotel => "Otel → Otel",
                TransferType.Custom => "Özel Transfer",
                _ => "Özel Transfer"
            };
        }

        /// <summary>
        /// Tüm transfer tiplerini listeler
        /// </summary>
        public static List<string> GetAllTypes()
        {
            return new List<string> 
            { 
                "AirportToHotel", 
                "HotelToAirport", 
                "HotelToRestaurant", 
                "RestaurantToHotel", 
                "HotelToCity", 
                "CityToHotel", 
                "HotelToHotel", 
                "Custom" 
            };
        }
    }
}

