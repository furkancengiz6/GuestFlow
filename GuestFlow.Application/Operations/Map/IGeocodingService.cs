// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Map.Dtos;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Map
{
    /// <summary>
    /// Geocoding servisi - Adresleri koordinatlara çevirir
    /// </summary>
    public interface IGeocodingService
    {
        /// <summary>
        /// Adresi koordinatlara çevirir (geocoding)
        /// </summary>
        Task<MapLocationDto?> GeocodeAsync(string address, string? cityName = null);
    }
}
