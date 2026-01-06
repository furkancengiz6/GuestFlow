using GuestFlow.Api.Models;
using GuestFlow.Api.Models.RestaurantModels;
using GuestFlow.Application.Operations.Restaurant.Dtos;
using GuestFlow.Application.Operations.Restaurant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Restoran yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Restoranlar")]
    public class RestaurantsController : BaseController
    {
        private readonly IRestaurantService _restaurantService;

        public RestaurantsController(IRestaurantService restaurantService)
        {
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Yeni bir restoran ekler
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddRestaurant(AddRestaurantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new AddRestaurantDto
            {
                RestaurantName = request.RestaurantName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                CityId = request.CityId,
                CuisineType = request.CuisineType,
                Capacity = request.Capacity,
                OperatingHours = request.OperatingHours,
                ReservationRequired = request.ReservationRequired,
                Notes = request.Notes,
                IsActive = request.IsActive
            };

            var result = await _restaurantService.AddRestaurant(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm restoranları getirir (sayfalanmış ve sıralanmış)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetRestaurantDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRestaurants(
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

            var result = await _restaurantService.GetRestaurantsPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetRestaurantDto>(result, "Restoranlar başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir restoranı ID'sine göre getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetRestaurantDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _restaurantService.GetRestaurantById(id);
            return result == null ? NotFound("Restoran bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Mevcut bir restoranı günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateRestaurantRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var dto = new UpdateRestaurantDto
            {
                Id = id,
                RestaurantName = request.RestaurantName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                CityId = request.CityId,
                CuisineType = request.CuisineType,
                Capacity = request.Capacity,
                OperatingHours = request.OperatingHours,
                ReservationRequired = request.ReservationRequired,
                Notes = request.Notes,
                IsActive = request.IsActive
            };

            var result = await _restaurantService.UpdateRestaurant(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir restoranı siler (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _restaurantService.DeleteRestaurant(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Şehir ID'sine göre restoranları getirir
        /// </summary>
        [HttpGet("city/{cityId}")]
        [ProducesResponseType(typeof(ApiResponse<List<GetRestaurantDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRestaurantsByCity(int cityId)
        {
            var result = await _restaurantService.GetRestaurantsByCityId(cityId);
            return Success(result, "Restoranlar başarıyla getirildi.");
        }
    }
}

