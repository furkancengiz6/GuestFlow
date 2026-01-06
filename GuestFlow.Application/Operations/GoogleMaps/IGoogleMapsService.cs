using GuestFlow.Application.Types;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.GoogleMaps
{
    /// <summary>
    /// Google Maps servisi interface'i
    /// </summary>
    public interface IGoogleMapsService
    {
        /// <summary>
        /// İki konum arasındaki mesafeyi ve süreyi hesaplar
        /// </summary>
        Task<ServiceMessage<DistanceMatrixResult>> GetDistanceMatrixAsync(
            string origin, 
            string destination, 
            string? mode = "driving");

        /// <summary>
        /// Adresi koordinatlara çevirir (Geocoding)
        /// </summary>
        Task<ServiceMessage<GeocodingResult>> GeocodeAddressAsync(string address);

        /// <summary>
        /// Koordinatları adrese çevirir (Reverse Geocoding)
        /// </summary>
        Task<ServiceMessage<GeocodingResult>> ReverseGeocodeAsync(double latitude, double longitude);

        /// <summary>
        /// Harita embed URL'i oluşturur
        /// </summary>
        string GetMapEmbedUrl(string address, int width = 600, int height = 450);

        /// <summary>
        /// Harita static image URL'i oluşturur
        /// </summary>
        string GetStaticMapUrl(string address, int width = 600, int height = 400, int zoom = 15);

        /// <summary>
        /// Yol tarifi URL'i oluşturur
        /// </summary>
        string GetDirectionsUrl(string origin, string destination, string? mode = "driving");
    }

    public class DistanceMatrixResult
    {
        public string Origin { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public double DistanceInMeters { get; set; }
        public double DistanceInKilometers { get; set; }
        public int DurationInSeconds { get; set; }
        public string DurationText { get; set; } = string.Empty;
        public string? Mode { get; set; }
    }

    public class GeocodingResult
    {
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? FormattedAddress { get; set; }
        public string? PlaceId { get; set; }
    }
}

