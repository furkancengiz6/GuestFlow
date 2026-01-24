// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Map.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Map
{
    /// <summary>
    /// Geocoding servisi implementasyonu - Google Maps API kullanarak
    /// </summary>
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeocodingService> _logger;
        private readonly Dictionary<string, MapLocationDto> _cache = new Dictionary<string, MapLocationDto>();

        // Türkiye'deki popüler şehirler için sabit koordinatlar (fallback)
        private static readonly Dictionary<string, MapLocationDto> CityCoordinates = new Dictionary<string, MapLocationDto>
        {
            { "İstanbul", new MapLocationDto { Latitude = 41.0082, Longitude = 28.9784, CityName = "İstanbul" } },
            { "Ankara", new MapLocationDto { Latitude = 39.9334, Longitude = 32.8597, CityName = "Ankara" } },
            { "İzmir", new MapLocationDto { Latitude = 38.4237, Longitude = 27.1428, CityName = "İzmir" } },
            { "Antalya", new MapLocationDto { Latitude = 36.8969, Longitude = 30.7133, CityName = "Antalya" } },
            { "Bodrum", new MapLocationDto { Latitude = 37.0344, Longitude = 27.4305, CityName = "Bodrum" } },
            { "Kapadokya", new MapLocationDto { Latitude = 38.6431, Longitude = 34.8331, CityName = "Kapadokya" } },
            { "Pamukkale", new MapLocationDto { Latitude = 37.9200, Longitude = 29.1200, CityName = "Pamukkale" } },
            { "Fethiye", new MapLocationDto { Latitude = 36.6214, Longitude = 29.1164, CityName = "Fethiye" } },
            { "Marmaris", new MapLocationDto { Latitude = 36.8550, Longitude = 28.2742, CityName = "Marmaris" } },
            { "Alanya", new MapLocationDto { Latitude = 36.5448, Longitude = 31.9958, CityName = "Alanya" } }
        };

        public GeocodingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeocodingService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<MapLocationDto?> GeocodeAsync(string address, string? cityName = null)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;

            try
            {
                // Check cache first
                var cacheKey = $"{address}|{cityName}";
                if (_cache.TryGetValue(cacheKey, out var cached))
                    return cached;

                // Try Google Maps API if configured
                var apiKey = _configuration["GoogleMaps:ApiKey"];
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var geocoded = await GeocodeWithGoogleMapsAsync(address, cityName, apiKey);
                    if (geocoded != null)
                    {
                        _cache[cacheKey] = geocoded;
                        return geocoded;
                    }
                }

                // Fallback: Use city-based coordinates
                var cityBased = GetCityBasedCoordinates(address, cityName);
                if (cityBased != null)
                {
                    _cache[cacheKey] = cityBased;
                    return cityBased;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geocoding failed for address: {Address}", address);
                
                // Fallback to city coordinates
                return GetCityBasedCoordinates(address, cityName);
            }
        }

        private async Task<MapLocationDto?> GeocodeWithGoogleMapsAsync(string address, string? cityName, string apiKey)
        {
            try
            {
                var fullAddress = string.IsNullOrEmpty(cityName) 
                    ? address 
                    : $"{address}, {cityName}, Turkey";

                var encodedAddress = Uri.EscapeDataString(fullAddress);
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={encodedAddress}&key={apiKey}";

                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GoogleGeocodingResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Status == "OK" && result.Results?.Length > 0)
                {
                    var location = result.Results[0].Geometry?.Location;
                    if (location != null)
                    {
                        return new MapLocationDto
                        {
                            Latitude = location.Lat,
                            Longitude = location.Lng,
                            Address = fullAddress,
                            CityName = cityName
                        };
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Google Maps geocoding failed for: {Address}", address);
                return null;
            }
        }

        private MapLocationDto? GetCityBasedCoordinates(string address, string? cityName)
        {
            // Try to match city name
            if (!string.IsNullOrEmpty(cityName))
            {
                foreach (var city in CityCoordinates.Keys)
                {
                    if (cityName.Contains(city, StringComparison.OrdinalIgnoreCase) ||
                        city.Contains(cityName, StringComparison.OrdinalIgnoreCase))
                    {
                        return new MapLocationDto
                        {
                            Latitude = CityCoordinates[city].Latitude,
                            Longitude = CityCoordinates[city].Longitude,
                            Address = address,
                            CityName = cityName
                        };
                    }
                }
            }

            // Try to match address with known cities
            foreach (var city in CityCoordinates.Keys)
            {
                if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                {
                    return new MapLocationDto
                    {
                        Latitude = CityCoordinates[city].Latitude,
                        Longitude = CityCoordinates[city].Longitude,
                        Address = address,
                        CityName = city
                    };
                }
            }

            // Default to Istanbul if nothing matches
            return new MapLocationDto
            {
                Latitude = CityCoordinates["İstanbul"].Latitude,
                Longitude = CityCoordinates["İstanbul"].Longitude,
                Address = address,
                CityName = cityName ?? "İstanbul"
            };
        }

        // Google Maps API response models
        private class GoogleGeocodingResponse
        {
            public string? Status { get; set; }
            public GoogleGeocodingResult[]? Results { get; set; }
        }

        private class GoogleGeocodingResult
        {
            public GoogleGeometry? Geometry { get; set; }
        }

        private class GoogleGeometry
        {
            public GoogleLocation? Location { get; set; }
        }

        private class GoogleLocation
        {
            public double Lat { get; set; }
            public double Lng { get; set; }
        }
    }
}
