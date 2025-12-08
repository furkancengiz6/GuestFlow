using GuestFlow.Api.Models;
using GuestFlow.Api.Models.YachtTourModels;
using GuestFlow.Application.Operations.YachtTour.Dtos;
using GuestFlow.Application.Operations.YachtTour;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Yat turu yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Yat Turları")]
    public class YachtToursController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _yachtTourService: Yat turlarıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IYachtTourService _yachtTourService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public YachtToursController(IYachtTourService yachtTourService)
        {
            _yachtTourService = yachtTourService;
        }

        /// <summary>
        /// Yeni bir yat turu ekler
        /// </summary>
        /// <param name="request">Yat turu bilgileri</param>
        /// <returns>Oluşturulan yat turu bilgileri</returns>
        /// <response code="200">Yat turu başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetYachtTourDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add(AddYachtTourRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddYachtTourDto
            {
                TourDate = request.TourDate,
                NumberOfPeople = request.NumberOfPeople,
                Price = request.Price,
                SpecialRequest = request.SpecialRequest,
                YachtName = request.YachtName,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId,
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription,
                Currency = request.Currency
            };

            // Yat turunu eklemek için servisi çağırıyorum.
            var result = await _yachtTourService.AddYachtTour(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm yat turlarını getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="cityId">Şehir ID filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="personnelId">Personel ID filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış yat turu listesi</returns>
        /// <response code="200">Yat turu listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetYachtTourDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYachtTours(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? cityId = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new GuestFlow.Application.Models.YachtTourFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                CityId = cityId,
                GuestId = guestId,
                PersonnelId = personnelId,
                SearchTerm = searchTerm
            };

            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış yat turlarını alıyorum ve JSON formatında döndürüyorum.
            var result = await _yachtTourService.GetYachtToursPaged(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetYachtTourDto>(result, "Yat turları başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir yat turunu ID'sine göre getirir
        /// </summary>
        /// <param name="id">Yat turu ID'si</param>
        /// <returns>Yat turu bilgileri</returns>
        /// <response code="200">Yat turu başarıyla getirildi</response>
        /// <response code="404">Yat turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetYachtTourDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            // Test için bir hata fırlatıyorum.
            if (id == 999)
                throw new ArgumentException("Geçersiz bir ID değeri: 999");
            
            // Servisten yat turunu ID'sine göre alıyorum.
            var result = await _yachtTourService.GetYachtTourById(id);
            if (result == null)
                return NotFound("Yat turu bulunamadı.");
            
            return Success(result, "Yat turu başarıyla getirildi.");
        }

        /// <summary>
        /// Mevcut bir yat turunu günceller
        /// </summary>
        /// <param name="id">Yat turu ID'si</param>
        /// <param name="request">Güncellenecek yat turu bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Yat turu başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Yat turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateYachtTourRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateYachtTourDto = new UpdateYachtTourDto
            {
                Id = id,
                TourDate = request.TourDate,
                NumberOfPeople = request.NumberOfPeople,
                Price = request.Price,
                SpecialRequest = request.SpecialRequest,
                YachtName = request.YachtName,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId
            };

            // Yat turunu güncellemek için servisi çağırıyorum.
            var result = await _yachtTourService.UpdateYachtTour(updateYachtTourDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir yat turunu siler (soft delete)
        /// </summary>
        /// <param name="id">Yat turu ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Yat turu başarıyla silindi</response>
        /// <response code="404">Yat turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Yat turunu silmek için servisi çağırıyorum.
            var result = await _yachtTourService.DeleteYachtTour(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Yat turu detayını getirir (ilgili veriler ile)
        /// </summary>
        /// <param name="id">Yat turu ID'si</param>
        /// <returns>Yat turu detay bilgileri</returns>
        /// <response code="200">Yat turu detayı başarıyla getirildi</response>
        /// <response code="404">Yat turu bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYachtTourDetail(int id)
        {
            try
            {
                var result = await _yachtTourService.GetYachtTourDetailAsync(id);
                return Success(result, "Yat turu detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Yat turu detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}