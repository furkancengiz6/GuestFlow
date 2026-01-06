using GuestFlow.Application.Types;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.GoogleMaps
{
    /// <summary>
    /// Google Maps servisi implementasyonu
    /// </summary>
    public class GoogleMapsService : IGoogleMapsService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GoogleMapsService> _logger;
        private readonly string? _apiKey;

        public GoogleMapsService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<GoogleMapsService> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiKey = _configuration["GoogleMaps:ApiKey"];
        }

        public async Task<ServiceMessage<DistanceMatrixResult>> GetDistanceMatrixAsync(
            string origin, 
            string destination, 
            string? mode = "driving")
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("Google Maps API key yapılandırılmamış. Varsayılan değerler döndürülüyor.");
                    // API key yoksa tahmini değerler döndür
                    return new ServiceMessage<DistanceMatrixResult>
                    {
                        IsSuccess = true,
                        Message = "API key yapılandırılmamış, tahmini değerler döndürüldü.",
                        Data = new DistanceMatrixResult
                        {
                            Origin = origin,
                            Destination = destination,
                            DistanceInKilometers = 10, // Tahmini
                            DistanceInMeters = 10000,
                            DurationInSeconds = 900, // 15 dakika tahmini
                            DurationText = "15 dakika",
                            Mode = mode
                        }
                    };
                }

                var client = _httpClientFactory.CreateClient();
                var url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
                    $"?origins={Uri.EscapeDataString(origin)}" +
                    $"&destinations={Uri.EscapeDataString(destination)}" +
                    $"&mode={mode}" +
                    $"&key={_apiKey}" +
                    $"&language=tr";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Google Maps API hatası: {response.StatusCode} - {content}");
                    return new ServiceMessage<DistanceMatrixResult>
                    {
                        IsSuccess = false,
                        Message = $"Google Maps API hatası: {response.StatusCode}"
                    };
                }

                // JSON parse işlemi
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Status kontrolü
                if (root.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    if (status != "OK")
                    {
                        _logger.LogWarning($"Google Maps API status: {status}");
                        return new ServiceMessage<DistanceMatrixResult>
                        {
                            IsSuccess = false,
                            Message = $"Google Maps API hatası: {status}"
                        };
                    }
                }

                // Distance Matrix response parse
                var result = new DistanceMatrixResult
                {
                    Origin = origin,
                    Destination = destination,
                    Mode = mode
                };

                if (root.TryGetProperty("rows", out var rows) && rows.GetArrayLength() > 0)
                {
                    var firstRow = rows[0];
                    if (firstRow.TryGetProperty("elements", out var elements) && elements.GetArrayLength() > 0)
                    {
                        var element = elements[0];
                        
                        if (element.TryGetProperty("distance", out var distance))
                        {
                            if (distance.TryGetProperty("value", out var distanceValue))
                            {
                                result.DistanceInMeters = distanceValue.GetInt32();
                                result.DistanceInKilometers = result.DistanceInMeters / 1000.0;
                            }
                        }

                        if (element.TryGetProperty("duration", out var duration))
                        {
                            if (duration.TryGetProperty("value", out var durationValue))
                            {
                                result.DurationInSeconds = durationValue.GetInt32();
                            }
                            if (duration.TryGetProperty("text", out var durationText))
                            {
                                result.DurationText = durationText.GetString() ?? string.Empty;
                            }
                        }
                    }
                }

                return new ServiceMessage<DistanceMatrixResult>
                {
                    IsSuccess = true,
                    Message = "Mesafe bilgisi başarıyla alındı.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Mesafe hesaplanırken hata: {ex.Message}");
                return new ServiceMessage<DistanceMatrixResult>
                {
                    IsSuccess = false,
                    Message = $"Mesafe hesaplanırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GeocodingResult>> GeocodeAddressAsync(string address)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return new ServiceMessage<GeocodingResult>
                    {
                        IsSuccess = false,
                        Message = "Google Maps API key yapılandırılmamış."
                    };
                }

                var client = _httpClientFactory.CreateClient();
                var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                    $"?address={Uri.EscapeDataString(address)}" +
                    $"&key={_apiKey}" +
                    $"&language=tr";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Google Maps Geocoding API hatası: {response.StatusCode} - {content}");
                    return new ServiceMessage<GeocodingResult>
                    {
                        IsSuccess = false,
                        Message = $"Google Maps API hatası: {response.StatusCode}"
                    };
                }

                // JSON parse işlemi
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Status kontrolü
                if (root.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    if (status != "OK" && status != "ZERO_RESULTS")
                    {
                        _logger.LogWarning($"Google Maps Geocoding API status: {status}");
                        return new ServiceMessage<GeocodingResult>
                        {
                            IsSuccess = false,
                            Message = $"Google Maps API hatası: {status}"
                        };
                    }
                }

                var result = new GeocodingResult
                {
                    Address = address
                };

                if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    
                    if (firstResult.TryGetProperty("formatted_address", out var formattedAddress))
                    {
                        result.FormattedAddress = formattedAddress.GetString();
                    }
                    else
                    {
                        result.FormattedAddress = address;
                    }

                    if (firstResult.TryGetProperty("place_id", out var placeId))
                    {
                        result.PlaceId = placeId.GetString();
                    }

                    if (firstResult.TryGetProperty("geometry", out var geometry))
                    {
                        if (geometry.TryGetProperty("location", out var location))
                        {
                            if (location.TryGetProperty("lat", out var lat))
                            {
                                result.Latitude = lat.GetDouble();
                            }
                            if (location.TryGetProperty("lng", out var lng))
                            {
                                result.Longitude = lng.GetDouble();
                            }
                        }
                    }
                }
                else
                {
                    // Sonuç bulunamadı
                    result.FormattedAddress = address;
                }

                return new ServiceMessage<GeocodingResult>
                {
                    IsSuccess = true,
                    Message = "Adres başarıyla koordinatlara çevrildi.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Geocoding yapılırken hata: {ex.Message}");
                return new ServiceMessage<GeocodingResult>
                {
                    IsSuccess = false,
                    Message = $"Geocoding yapılırken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<GeocodingResult>> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return new ServiceMessage<GeocodingResult>
                    {
                        IsSuccess = false,
                        Message = "Google Maps API key yapılandırılmamış."
                    };
                }

                var client = _httpClientFactory.CreateClient();
                var url = $"https://maps.googleapis.com/maps/api/geocode/json" +
                    $"?latlng={latitude},{longitude}" +
                    $"&key={_apiKey}" +
                    $"&language=tr";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Google Maps Reverse Geocoding API hatası: {response.StatusCode} - {content}");
                    return new ServiceMessage<GeocodingResult>
                    {
                        IsSuccess = false,
                        Message = $"Google Maps API hatası: {response.StatusCode}"
                    };
                }

                // JSON parse işlemi
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // Status kontrolü
                if (root.TryGetProperty("status", out var statusElement))
                {
                    var status = statusElement.GetString();
                    if (status != "OK" && status != "ZERO_RESULTS")
                    {
                        _logger.LogWarning($"Google Maps Reverse Geocoding API status: {status}");
                        return new ServiceMessage<GeocodingResult>
                        {
                            IsSuccess = false,
                            Message = $"Google Maps API hatası: {status}"
                        };
                    }
                }

                var result = new GeocodingResult
                {
                    Latitude = latitude,
                    Longitude = longitude
                };

                if (root.TryGetProperty("results", out var results) && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];
                    
                    if (firstResult.TryGetProperty("formatted_address", out var formattedAddress))
                    {
                        result.FormattedAddress = formattedAddress.GetString();
                        result.Address = formattedAddress.GetString() ?? string.Empty;
                    }
                    else
                    {
                        result.Address = "Adres bulunamadı";
                        result.FormattedAddress = "Adres bulunamadı";
                    }

                    if (firstResult.TryGetProperty("place_id", out var placeId))
                    {
                        result.PlaceId = placeId.GetString();
                    }
                }
                else
                {
                    result.Address = "Adres bulunamadı";
                    result.FormattedAddress = "Adres bulunamadı";
                }

                return new ServiceMessage<GeocodingResult>
                {
                    IsSuccess = true,
                    Message = "Koordinatlar başarıyla adrese çevrildi.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Reverse geocoding yapılırken hata: {ex.Message}");
                return new ServiceMessage<GeocodingResult>
                {
                    IsSuccess = false,
                    Message = $"Reverse geocoding yapılırken hata: {ex.Message}"
                };
            }
        }

        public string GetMapEmbedUrl(string address, int width = 600, int height = 450)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            return $"https://www.google.com/maps/embed/v1/place?key={_apiKey}&q={encodedAddress}";
        }

        public string GetStaticMapUrl(string address, int width = 600, int height = 400, int zoom = 15)
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var keyParam = string.IsNullOrEmpty(_apiKey) ? "" : $"&key={_apiKey}";
            return $"https://maps.googleapis.com/maps/api/staticmap?center={encodedAddress}&zoom={zoom}&size={width}x{height}&markers={encodedAddress}{keyParam}";
        }

        public string GetDirectionsUrl(string origin, string destination, string? mode = "driving")
        {
            var encodedOrigin = Uri.EscapeDataString(origin);
            var encodedDestination = Uri.EscapeDataString(destination);
            return $"https://www.google.com/maps/dir/?api=1&origin={encodedOrigin}&destination={encodedDestination}&travelmode={mode}";
        }
    }
}

