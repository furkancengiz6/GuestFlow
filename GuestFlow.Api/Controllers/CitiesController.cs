using GuestFlow.Api.Models;
using GuestFlow.Api.Models.CityModels;
using GuestFlow.Application.Operations.City;
using GuestFlow.Application.Operations.City.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Şehir yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Şehirler")]
    public class CitiesController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _cityService: Şehirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly ICityService _cityService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        /// <summary>
        /// Yeni bir şehir ekler
        /// </summary>
        /// <param name="request">Şehir bilgileri</param>
        /// <returns>Oluşturulan şehir bilgileri</returns>
        /// <response code="200">Şehir başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetCityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add(AddCityRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddCityDto
            {
                CityName = request.CityName,
                Country = request.Country
            };

            // Şehri eklemek için servisi çağırıyorum.
            var result = await _cityService.AddCity(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Mevcut bir şehri günceller
        /// </summary>
        /// <param name="id">Şehir ID'si</param>
        /// <param name="request">Güncellenecek şehir bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Şehir başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Şehir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateCityRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum.
            var dto = new UpdateCityDto
            {
                Id = id,
                CityName = request.CityName,
                Country = request.Country
            };

            // Şehri güncellemek için servisi çağırıyorum.
            var result = await _cityService.UpdateCity(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir şehri siler (soft delete)
        /// </summary>
        /// <param name="id">Şehir ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Şehir başarıyla silindi</response>
        /// <response code="404">Şehir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Şehri silmek için servisi çağırıyorum.
            var result = await _cityService.DeleteCity(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Belirli bir şehri ID'sine göre getirir
        /// </summary>
        /// <param name="id">Şehir ID'si</param>
        /// <returns>Şehir bilgileri</returns>
        /// <response code="200">Şehir başarıyla getirildi</response>
        /// <response code="404">Şehir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetCityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                // Servisten şehri ID'sine göre alıyorum.
                var result = await _cityService.GetCityById(id);
                return Success(result, "Şehir başarıyla getirildi.");
            }
            catch (Exception)
            {
                return NotFound("Şehir bulunamadı.");
            }
        }

        /// <summary>
        /// Tüm şehirleri getirir (sayfalanmış ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış şehir listesi</returns>
        /// <response code="200">Şehir listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetCityDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCities(
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

            // Servisten sayfalanmış ve sıralanmış şehirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _cityService.GetCitiesPaged(pageNumber, pageSize, sorting);
            return PagedResult<GetCityDto>(result, "Şehirler başarıyla getirildi.");
        }
    }
}