using GuestFlow.Api.Models;
using GuestFlow.Api.Models.VehicleModels;
using GuestFlow.Application.Operations.Vehicle;
using GuestFlow.Application.Operations.Vehicle.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Araç yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Araçlar")]
    public class VehiclesController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _vehicleService: Araçlarla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IVehicleService _vehicleService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Yeni bir araç ekler
        /// </summary>
        /// <param name="request">Araç bilgileri</param>
        /// <returns>Oluşturulan araç bilgileri</returns>
        /// <response code="200">Araç başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetVehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddVehicle(AddVehicleRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addVehicleDto = new AddVehicleDto
            {
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            // Aracı eklemek için servisi çağırıyorum.
            var result = await _vehicleService.AddVehicle(addVehicleDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm araçları getirir (sayfalanmış ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış araç listesi</returns>
        /// <response code="200">Araç listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetVehicleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetVehicles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış ve sıralanmış araçları alıyorum ve JSON formatında döndürüyorum.
            var result = await _vehicleService.GetVehiclesPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetVehicleDto>(result, "Araçlar başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir aracı ID'sine göre getirir
        /// </summary>
        /// <param name="id">Araç ID'si</param>
        /// <returns>Araç bilgileri</returns>
        /// <response code="200">Araç başarıyla getirildi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetVehicleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Servisten aracı ID'sine göre alıyorum.
                var result = await _vehicleService.GetVehicleById(id);
                return Success(result, "Araç başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return NotFound("Araç bulunamadı.");
            }
        }

        /// <summary>
        /// Mevcut bir aracı günceller
        /// </summary>
        /// <param name="id">Araç ID'si</param>
        /// <param name="request">Güncellenecek araç bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Araç başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateVehicleRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateVehicleDto = new UpdateVehicleDto
            {
                Id = id,
                Type = request.Type,
                PlateNumber = request.PlateNumber,
                Capacity = request.Capacity,
                DailyPrice = request.DailyPrice
            };

            // Aracı güncellemek için servisi çağırıyorum.
            var result = await _vehicleService.UpdateVehicle(updateVehicleDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir aracı siler (soft delete)
        /// </summary>
        /// <param name="id">Araç ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Araç başarıyla silindi</response>
        /// <response code="404">Araç bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Aracı silmek için servisi çağırıyorum.
            var result = await _vehicleService.DeleteVehicle(id);
            return FromServiceMessage(result);
        }
    }
}