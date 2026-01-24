// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Map;
using GuestFlow.Application.Operations.Map.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff,Concierge")]
    [Tags("Harita")]
    public class MapController : BaseController
    {
        private readonly IMapService _mapService;

        public MapController(IMapService mapService)
        {
            _mapService = mapService;
        }

        /// <summary>
        /// Harita görünümü getirir (tüm servisler için lokasyonlar)
        /// </summary>
        /// <param name="filter">Filtreleme parametreleri</param>
        /// <returns>Harita görünümü DTO</returns>
        [HttpPost("view")]
        [ProducesResponseType(typeof(ApiResponse<MapViewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMapView([FromBody] MapFilterDto? filter = null)
        {
            try
            {
                var result = await _mapService.GetMapViewAsync(filter);
                return Ok(ApiResponse<MapViewDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("Harita görünümü getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir servis için lokasyon bilgisi getirir
        /// </summary>
        /// <param name="serviceId">Servis ID</param>
        /// <param name="serviceType">Servis tipi (Transfer, CityTour, YachtTour)</param>
        /// <returns>Servis lokasyon DTO</returns>
        [HttpGet("service/{serviceId}/{serviceType}")]
        [ProducesResponseType(typeof(ApiResponse<MapServiceLocationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetServiceLocation(int serviceId, string serviceType)
        {
            try
            {
                var result = await _mapService.GetServiceLocationAsync(serviceId, serviceType);
                if (result == null)
                    return NotFound(new ApiResponse<MapServiceLocationDto> { Success = false, Message = "Servis bulunamadı" });

                return Ok(ApiResponse<MapServiceLocationDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("Servis lokasyonu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Adres için koordinat bilgisi getirir (geocoding)
        /// </summary>
        /// <param name="address">Adres</param>
        /// <param name="cityName">Şehir adı (opsiyonel)</param>
        /// <returns>Lokasyon DTO</returns>
        [HttpGet("geocode")]
        [ProducesResponseType(typeof(ApiResponse<MapLocationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GeocodeAddress([FromQuery] string address, [FromQuery] string? cityName = null)
        {
            try
            {
                var result = await _mapService.GeocodeAddressAsync(address, cityName);
                if (result == null)
                    return Ok(new ApiResponse<MapLocationDto> { Success = false, Message = "Adres geocoding yapılamadı" });

                return Ok(ApiResponse<MapLocationDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("Adres geocoding yapılırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}
