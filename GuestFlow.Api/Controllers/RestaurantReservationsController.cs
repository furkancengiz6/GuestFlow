using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.RestaurantReservation;
using GuestFlow.Application.Operations.RestaurantReservation.Dtos;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Restoran rezervasyon yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Restoran Rezervasyonları")]
    public class RestaurantReservationsController : BaseController
    {
        private readonly IRestaurantReservationService _reservationService;

        public RestaurantReservationsController(IRestaurantReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        /// <summary>
        /// Yeni bir restoran rezervasyonu oluşturur
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetRestaurantReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddReservation([FromBody] AddRestaurantReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Giriş yapmış kullanıcının ID'sini al
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int personnelId))
            {
                dto.PersonnelId = personnelId;
            }

            var result = await _reservationService.AddRestaurantReservation(dto);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(new { Message = result.Message });
        }

        /// <summary>
        /// Tüm restoran rezervasyonlarını getirir (sayfalanmış)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetRestaurantReservationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _reservationService.GetRestaurantReservationsPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetRestaurantReservationDto>(result, "Restoran rezervasyonları başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir rezervasyonu ID'sine göre getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetRestaurantReservationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _reservationService.GetRestaurantReservationById(id);
            return result == null ? NotFound("Rezervasyon bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Misafir ID'sine göre rezervasyonları getirir
        /// </summary>
        [HttpGet("guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetRestaurantReservationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByGuest(int guestId)
        {
            var result = await _reservationService.GetRestaurantReservationsByGuestId(guestId);
            return Success(result, "Rezervasyonlar başarıyla getirildi.");
        }

        /// <summary>
        /// Restoran ID'sine göre rezervasyonları getirir
        /// </summary>
        [HttpGet("restaurant/{restaurantId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetRestaurantReservationDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReservationsByRestaurant(int restaurantId)
        {
            var result = await _reservationService.GetRestaurantReservationsByRestaurantId(restaurantId);
            return Success(result, "Rezervasyonlar başarıyla getirildi.");
        }

        /// <summary>
        /// Rezervasyonu günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRestaurantReservationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var result = await _reservationService.UpdateRestaurantReservation(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu siler
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _reservationService.DeleteRestaurantReservation(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyon durumunu günceller
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReservationStatusRequest request)
        {
            var result = await _reservationService.UpdateRestaurantReservationStatus(id, request.Status);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu onaylar
        /// </summary>
        [HttpPost("{id}/confirm")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Confirm(int id)
        {
            var result = await _reservationService.ConfirmRestaurantReservation(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyonu iptal eder
        /// </summary>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelReservationRequest? request = null)
        {
            var result = await _reservationService.CancelRestaurantReservation(id, request?.Reason);
            return FromServiceMessage(result);
        }
    }

    public class UpdateReservationStatusRequest
    {
        public ReservationStatus Status { get; set; }
    }

    public class CancelReservationRequest
    {
        public string? Reason { get; set; }
    }
}

