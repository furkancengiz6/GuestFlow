using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.ServicePackage;
using GuestFlow.Application.Operations.ServicePackage.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Servis paketi yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Servis Paketleri")]
    public class ServicePackagesController : BaseController
    {
        private readonly IServicePackageService _packageService;

        public ServicePackagesController(IServicePackageService packageService)
        {
            _packageService = packageService;
        }

        /// <summary>
        /// Yeni bir servis paketi oluşturur
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetServicePackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddPackage([FromBody] AddServicePackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _packageService.AddServicePackage(dto);
            return result.IsSuccess 
                ? Success(result.Data, result.Message) 
                : BadRequest(new { Message = result.Message });
        }

        /// <summary>
        /// Tüm servis paketlerini getirir (sayfalanmış)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetServicePackageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPackages(
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

            var result = await _packageService.GetServicePackagesPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetServicePackageDto>(result, "Servis paketleri başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir paketi ID'sine göre getirir
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetServicePackageDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _packageService.GetServicePackageById(id);
            return result == null ? NotFound("Paket bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Paketi günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServicePackageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Id = id;
            var result = await _packageService.UpdateServicePackage(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paketi siler
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _packageService.DeleteServicePackage(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Pakete transfer ekler
        /// </summary>
        [HttpPost("{id}/transfers/{transferId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddTransfer(int id, int transferId)
        {
            var result = await _packageService.AddTransferToPackage(id, transferId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Pakete şehir turu ekler
        /// </summary>
        [HttpPost("{id}/city-tours/{cityTourId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddCityTour(int id, int cityTourId)
        {
            var result = await _packageService.AddCityTourToPackage(id, cityTourId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Pakete yat turu ekler
        /// </summary>
        [HttpPost("{id}/yacht-tours/{yachtTourId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddYachtTour(int id, int yachtTourId)
        {
            var result = await _packageService.AddYachtTourToPackage(id, yachtTourId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Pakete restoran rezervasyonu ekler
        /// </summary>
        [HttpPost("{id}/restaurant-reservations/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddRestaurantReservation(int id, int reservationId)
        {
            var result = await _packageService.AddRestaurantReservationToPackage(id, reservationId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paketten transfer kaldırır
        /// </summary>
        [HttpDelete("{id}/transfers/{transferId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveTransfer(int id, int transferId)
        {
            var result = await _packageService.RemoveTransferFromPackage(id, transferId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paketten şehir turu kaldırır
        /// </summary>
        [HttpDelete("{id}/city-tours/{cityTourId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveCityTour(int id, int cityTourId)
        {
            var result = await _packageService.RemoveCityTourFromPackage(id, cityTourId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paketten yat turu kaldırır
        /// </summary>
        [HttpDelete("{id}/yacht-tours/{yachtTourId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveYachtTour(int id, int yachtTourId)
        {
            var result = await _packageService.RemoveYachtTourFromPackage(id, yachtTourId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paketten restoran rezervasyonu kaldırır
        /// </summary>
        [HttpDelete("{id}/restaurant-reservations/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveRestaurantReservation(int id, int reservationId)
        {
            var result = await _packageService.RemoveRestaurantReservationFromPackage(id, reservationId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Paket toplam maliyetini hesaplar
        /// </summary>
        [HttpGet("{id}/total-cost")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CalculateTotalCost(int id)
        {
            var result = await _packageService.CalculatePackageTotalCost(id);
            return Success(result, "Toplam maliyet hesaplandı.");
        }
    }
}

