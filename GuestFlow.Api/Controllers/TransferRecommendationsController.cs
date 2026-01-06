using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.TransferRecommendation;
using GuestFlow.Application.Operations.TransferRecommendation.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Otomatik transfer önerileri için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Transfer Önerileri")]
    public class TransferRecommendationsController : BaseController
    {
        private readonly ITransferRecommendationService _recommendationService;

        public TransferRecommendationsController(ITransferRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        /// <summary>
        /// Misafir için tüm transfer önerilerini getirir
        /// </summary>
        [HttpGet("guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<List<TransferRecommendationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRecommendationsForGuest(int guestId)
        {
            var result = await _recommendationService.GetRecommendationsForGuest(guestId);
            return Success<List<TransferRecommendationDto>>(result, "Transfer önerileri başarıyla getirildi.");
        }

        /// <summary>
        /// Check-in tarihine göre havalimanı→otel transfer önerisi
        /// </summary>
        [HttpGet("guest/{guestId}/airport-to-hotel")]
        [ProducesResponseType(typeof(ApiResponse<TransferRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAirportToHotelRecommendation(int guestId)
        {
            var result = await _recommendationService.RecommendAirportToHotelTransfer(guestId);
            return result == null 
                ? Success<TransferRecommendationDto>(null, "Bu misafir için havalimanı→otel transfer önerisi bulunamadı.") 
                : Success<TransferRecommendationDto>(result, "Transfer önerisi başarıyla getirildi.");
        }

        /// <summary>
        /// Check-out tarihine göre otel→havalimanı transfer önerisi
        /// </summary>
        [HttpGet("guest/{guestId}/hotel-to-airport")]
        [ProducesResponseType(typeof(ApiResponse<TransferRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHotelToAirportRecommendation(int guestId)
        {
            var result = await _recommendationService.RecommendHotelToAirportTransfer(guestId);
            return result == null 
                ? Success<TransferRecommendationDto>(null, "Bu misafir için otel→havalimanı transfer önerisi bulunamadı.") 
                : Success<TransferRecommendationDto>(result, "Transfer önerisi başarıyla getirildi.");
        }

        /// <summary>
        /// Şehir turu için transfer önerisi
        /// </summary>
        [HttpGet("guest/{guestId}/city-tour/{cityTourId}")]
        [ProducesResponseType(typeof(ApiResponse<TransferRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCityTourRecommendation(int guestId, int cityTourId)
        {
            var result = await _recommendationService.RecommendTransferForCityTour(guestId, cityTourId);
            return result == null 
                ? Success<TransferRecommendationDto>(null, "Bu şehir turu için transfer önerisi bulunamadı.") 
                : Success<TransferRecommendationDto>(result, "Transfer önerisi başarıyla getirildi.");
        }

        /// <summary>
        /// Yat turu için transfer önerisi
        /// </summary>
        [HttpGet("guest/{guestId}/yacht-tour/{yachtTourId}")]
        [ProducesResponseType(typeof(ApiResponse<TransferRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYachtTourRecommendation(int guestId, int yachtTourId)
        {
            var result = await _recommendationService.RecommendTransferForYachtTour(guestId, yachtTourId);
            return result == null 
                ? Success<TransferRecommendationDto>(null, "Bu yat turu için transfer önerisi bulunamadı.") 
                : Success<TransferRecommendationDto>(result, "Transfer önerisi başarıyla getirildi.");
        }

        /// <summary>
        /// Restoran rezervasyonu için transfer önerisi
        /// </summary>
        [HttpGet("guest/{guestId}/restaurant-reservation/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<TransferRecommendationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRestaurantReservationRecommendation(int guestId, int reservationId)
        {
            var result = await _recommendationService.RecommendTransferForRestaurantReservation(guestId, reservationId);
            return result == null 
                ? Success<TransferRecommendationDto>(null, "Bu restoran rezervasyonu için transfer önerisi bulunamadı.") 
                : Success<TransferRecommendationDto>(result, "Transfer önerisi başarıyla getirildi.");
        }
    }
}

