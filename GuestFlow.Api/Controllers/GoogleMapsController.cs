using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.GoogleMaps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Google Maps entegrasyonu için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Google Maps")]
    public class GoogleMapsController : BaseController
    {
        private readonly IGoogleMapsService _googleMapsService;

        public GoogleMapsController(IGoogleMapsService googleMapsService)
        {
            _googleMapsService = googleMapsService;
        }

        /// <summary>
        /// İki konum arasındaki mesafeyi ve süreyi hesaplar
        /// </summary>
        [HttpGet("distance")]
        [ProducesResponseType(typeof(ApiResponse<DistanceMatrixResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDistance(
            [FromQuery] string origin,
            [FromQuery] string destination,
            [FromQuery] string? mode = "driving")
        {
            var result = await _googleMapsService.GetDistanceMatrixAsync(origin, destination, mode);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(result.Message);
        }

        /// <summary>
        /// Adresi koordinatlara çevirir
        /// </summary>
        [HttpGet("geocode")]
        [ProducesResponseType(typeof(ApiResponse<GeocodingResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GeocodeAddress([FromQuery] string address)
        {
            var result = await _googleMapsService.GeocodeAddressAsync(address);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(result.Message);
        }

        /// <summary>
        /// Koordinatları adrese çevirir
        /// </summary>
        [HttpGet("reverse-geocode")]
        [ProducesResponseType(typeof(ApiResponse<GeocodingResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReverseGeocode(
            [FromQuery] double latitude,
            [FromQuery] double longitude)
        {
            var result = await _googleMapsService.ReverseGeocodeAsync(latitude, longitude);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(result.Message);
        }

        /// <summary>
        /// Harita embed URL'i döndürür
        /// </summary>
        [HttpGet("embed-url")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMapEmbedUrl(
            [FromQuery] string address,
            [FromQuery] int width = 600,
            [FromQuery] int height = 450)
        {
            var url = _googleMapsService.GetMapEmbedUrl(address, width, height);
            return Success(url, "Harita embed URL'i başarıyla oluşturuldu.");
        }

        /// <summary>
        /// Harita static image URL'i döndürür
        /// </summary>
        [HttpGet("static-map-url")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetStaticMapUrl(
            [FromQuery] string address,
            [FromQuery] int width = 600,
            [FromQuery] int height = 400,
            [FromQuery] int zoom = 15)
        {
            var url = _googleMapsService.GetStaticMapUrl(address, width, height, zoom);
            return Success(url, "Harita static image URL'i başarıyla oluşturuldu.");
        }

        /// <summary>
        /// Yol tarifi URL'i döndürür
        /// </summary>
        [HttpGet("directions-url")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetDirectionsUrl(
            [FromQuery] string origin,
            [FromQuery] string destination,
            [FromQuery] string? mode = "driving")
        {
            var url = _googleMapsService.GetDirectionsUrl(origin, destination, mode);
            return Success(url, "Yol tarifi URL'i başarıyla oluşturuldu.");
        }
    }
}

