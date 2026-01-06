using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Itinerary;
using GuestFlow.Application.Operations.Itinerary.Dtos;
using GuestFlow.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// İtinerary (Seyahat Planı) yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("İtineraryler")]
    public class ItinerariesController : BaseController
    {
        private readonly IItineraryService _itineraryService;

        public ItinerariesController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        /// <summary>
        /// Yeni bir itinerary oluşturur
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetItineraryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItinerary([FromBody] AddItineraryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Giriş yapmış kullanıcının ID'sini al
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int personnelId))
            {
                dto.PersonnelId = personnelId;
            }

            var result = await _itineraryService.AddItinerary(dto);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(new { Message = result.Message });
        }

        /// <summary>
        /// Tüm itinerary'leri getirir (sayfalanmış)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetItineraryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetItineraries(
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

            var result = await _itineraryService.GetItinerariesPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetItineraryDto>(result, "İtineraryler başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir itinerary'yi ID'sine göre getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetItineraryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _itineraryService.GetItineraryById(id);
            return result == null ? NotFound("İtinerary bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Misafir ID'sine göre itinerary'leri getirir
        /// </summary>
        [HttpGet("guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetItineraryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetItinerariesByGuest(int guestId)
        {
            var result = await _itineraryService.GetItinerariesByGuestId(guestId);
            return Success(result, "İtineraryler başarıyla getirildi.");
        }

        /// <summary>
        /// İtinerary timeline görünümünü getirir
        /// </summary>
        [HttpGet("{id}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<ItineraryTimelineDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTimeline(int id)
        {
            var result = await _itineraryService.GetItineraryTimeline(id);
            return result == null ? NotFound("İtinerary bulunamadı.") : Success(result, "Timeline başarıyla getirildi.");
        }

        /// <summary>
        /// İtinerary'ye item ekler
        /// </summary>
        [HttpPost("{id}/items")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddItem(int id, [FromBody] AddItineraryItemDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itineraryService.AddItineraryItem(id, item);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary item'ını günceller
        /// </summary>
        [HttpPut("{id}/items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateItem(int id, int itemId, [FromBody] AddItineraryItemDto item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _itineraryService.UpdateItineraryItem(id, itemId, item);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary item'ını siler
        /// </summary>
        [HttpDelete("{id}/items/{itemId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteItem(int id, int itemId)
        {
            var result = await _itineraryService.DeleteItineraryItem(id, itemId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary durumunu günceller
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateItineraryStatusRequest request)
        {
            var result = await _itineraryService.UpdateItineraryStatus(id, request.Status);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary'yi günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateItineraryDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var result = await _itineraryService.UpdateItinerary(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary'yi siler
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _itineraryService.DeleteItinerary(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// İtinerary toplam maliyetini hesaplar
        /// </summary>
        [HttpGet("{id}/total-cost")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CalculateTotalCost(int id)
        {
            var result = await _itineraryService.CalculateItineraryTotalCost(id);
            return Success(result, "Toplam maliyet hesaplandı.");
        }
    }

    public class UpdateItineraryStatusRequest
    {
        public ItineraryStatus Status { get; set; }
    }
}

