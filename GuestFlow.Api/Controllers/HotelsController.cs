using GuestFlow.Api.Models;
using GuestFlow.Api.Models.HotelModels;
using GuestFlow.Application.Operations.Hotel.Dtos;
using GuestFlow.Application.Operations.Hotel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Otel yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Oteller")]
    public class HotelsController : BaseController
    {
        private readonly IHotelService _hotelService;

        public HotelsController(IHotelService hotelService)
        {
            _hotelService = hotelService;
        }

        /// <summary>
        /// Yeni bir otel ekler
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddHotel(AddHotelRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddHotelDto
            {
                HotelName = request.HotelName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                CityId = request.CityId,
                StarRating = request.StarRating,
                CheckInTime = request.CheckInTime,
                CheckOutTime = request.CheckOutTime,
                RoomTypes = request.RoomTypes,
                Amenities = request.Amenities,
                Notes = request.Notes,
                IsActive = request.IsActive
            };

            var result = await _hotelService.AddHotel(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm otelleri getirir (sayfalanmış ve sıralanmış)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetHotelDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHotels(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _hotelService.GetHotelsPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetHotelDto>(result, "Oteller başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir oteli ID'sine göre getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetHotelDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _hotelService.GetHotelById(id);
            return result == null ? NotFound("Otel bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Mevcut bir oteli günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateHotelRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new UpdateHotelDto
            {
                Id = id,
                HotelName = request.HotelName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                CityId = request.CityId,
                StarRating = request.StarRating,
                CheckInTime = request.CheckInTime,
                CheckOutTime = request.CheckOutTime,
                RoomTypes = request.RoomTypes,
                Amenities = request.Amenities,
                Notes = request.Notes,
                IsActive = request.IsActive
            };

            var result = await _hotelService.UpdateHotel(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir oteli siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _hotelService.DeleteHotel(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Şehir ID'sine göre otelleri getirir
        /// </summary>
        [HttpGet("city/{cityId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetHotelDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetHotelsByCity(int cityId)
        {
            var result = await _hotelService.GetHotelsByCityId(cityId);
            return Success(result, "Oteller başarıyla getirildi.");
        }
    }
}

