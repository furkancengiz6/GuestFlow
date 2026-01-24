// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Map.Dtos;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Map
{
    /// <summary>
    /// Harita servisi - Operasyonel harita görünümü için
    /// </summary>
    public interface IMapService
    {
        /// <summary>
        /// Belirli bir tarih için harita görünümü getirir
        /// </summary>
        Task<MapViewDto> GetMapViewAsync(MapFilterDto? filter = null);

        /// <summary>
        /// Belirli bir servis için detaylı lokasyon bilgisi getirir
        /// </summary>
        Task<MapServiceLocationDto?> GetServiceLocationAsync(int serviceId, string serviceType);

        /// <summary>
        /// Adres için koordinat bilgisi getirir (geocoding)
        /// </summary>
        Task<MapLocationDto?> GeocodeAddressAsync(string address, string? cityName = null);
    }
}
