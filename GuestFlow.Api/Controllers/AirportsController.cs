using GuestFlow.Api.Models;
using GuestFlow.Api.Models.AirportModels;
using GuestFlow.Application.Operations.Airport.Dtos;
using GuestFlow.Application.Operations.Airport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Havalimanı yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Tags("Havalimanları")]
    public class AirportsController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _airportService: Havalimanıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IAirportService _airportService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public AirportsController(IAirportService airportService)
        {
            _airportService = airportService;
        }

        /// <summary>
        /// Yeni bir havalimanı ekler
        /// </summary>
        /// <param name="request">Havalimanı bilgileri</param>
        /// <returns>Oluşturulan havalimanı bilgileri</returns>
        /// <response code="200">Havalimanı başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetAirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAirport(AddAirportRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addAirportDto = new AddAirportDto
            {
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            // Havalimanını eklemek için servisi çağırıyorum.
            var result = await _airportService.AddAirport(addAirportDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm havalimanlarını getirir (sayfalanmış ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış havalimanı listesi</returns>
        /// <response code="200">Havalimanı listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [Authorize(Roles = "Staff,Admin")] // Bu endpoint'e sadece Staff ve Admin rolleri erişebilir.
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetAirportDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAirports(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış ve sıralanmış havalimanlarını alıyorum ve JSON formatında döndürüyorum.
            var result = await _airportService.GetAirportsPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetAirportDto>(result, "Havalimanları başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir havalimanını ID'sine göre getirir
        /// </summary>
        /// <param name="id">Havalimanı ID'si</param>
        /// <returns>Havalimanı bilgileri</returns>
        /// <response code="200">Havalimanı başarıyla getirildi</response>
        /// <response code="404">Havalimanı bulunamadı</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetAirportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Servisten havalimanını ID'sine göre alıyorum.
                var result = await _airportService.GetAirportById(id);
                return Success(result, "Havalimanı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return NotFound("Havalimanı bulunamadı.");
            }
        }

        /// <summary>
        /// Mevcut bir havalimanını günceller
        /// </summary>
        /// <param name="id">Havalimanı ID'si</param>
        /// <param name="request">Güncellenecek havalimanı bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Havalimanı başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Havalimanı bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Staff,Admin")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateAirportRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateAirportDto = new UpdateAirportDto
            {
                Id = id,
                Name = request.Name,
                Code = request.Code,
                CityId = request.CityId
            };

            // Havalimanını güncellemek için servisi çağırıyorum.
            var result = await _airportService.UpdateAirport(updateAirportDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir havalimanını siler (soft delete)
        /// </summary>
        /// <param name="id">Havalimanı ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Havalimanı başarıyla silindi</response>
        /// <response code="404">Havalimanı bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")] 
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Havalimanını silmek için servisi çağırıyorum.
            var result = await _airportService.DeleteAirport(id);
            return FromServiceMessage(result);
        }
    }
}