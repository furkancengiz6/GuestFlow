using GuestFlow.Api.Models;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Domain.Entities.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Misafir yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Misafirler")]
    public class GuestsController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _guestService: Misafirlerle ilgili işlemleri yapmak için kullanıyorum.
        private readonly IGuestService _guestService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public GuestsController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        /// <summary>
        /// Yeni bir misafir ekler
        /// </summary>
        /// <param name="request">Misafir bilgileri</param>
        /// <returns>Oluşturulan misafir bilgileri</returns>
        /// <response code="200">Misafir başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <example>
        /// <code>
        /// POST /api/v1/guests
        /// {
        ///   "fullName": "John Doe",
        ///   "email": "john.doe@example.com",
        ///   "phoneNumber": "+90 555 123 4567",
        ///   "nationality": "US",
        ///   "isSpecialGuest": false
        /// }
        /// </code>
        /// </example>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetGuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddGuest(AddGuestRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var addGuestDto = new AddGuestDto
            {
                FullName = request.FullName,
                Email = request.Email,
                Nationality = request.Nationality,
                PhoneNumber = request.PhoneNumber,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri eklemek için servisi çağırıyorum.
            var result = await _guestService.AddGuest(addGuestDto);
            // Standart API yanıt formatını kullan
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm misafirleri getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="searchTerm">Arama terimi (isim, email, telefon)</param>
        /// <param name="nationality">Uyruk filtresi</param>
        /// <param name="isSpecialGuest">Özel misafir filtresi</param>
        /// <param name="email">Email filtresi</param>
        /// <param name="phoneNumber">Telefon numarası filtresi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış misafir listesi</returns>
        /// <response code="200">Misafir listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<GetGuestDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? nationality = null,
            [FromQuery] bool? isSpecialGuest = null,
            [FromQuery] string? email = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new GuestFlow.Application.Models.GuestFilterParameters
            {
                SearchTerm = searchTerm,
                Nationality = nationality,
                IsSpecialGuest = isSpecialGuest,
                Email = email,
                PhoneNumber = phoneNumber
            };

            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış misafirleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _guestService.GetGuestsPaged(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetGuestDto>(result, "Misafirler başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir misafiri ID'sine göre getirir
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir bilgileri</returns>
        /// <response code="200">Misafir başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetGuestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestById(int id)
        {
            // Servisten misafiri ID'sine göre alıyorum.
            var result = await _guestService.GetGuestById(id);
            // Standart API yanıt formatını kullan
            return result == null ? NotFound("Misafir bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Mevcut bir misafiri günceller
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <param name="request">Güncellenecek misafir bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Misafir başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateGuestRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateGuestDto = new UpdateGuestDto
            {
                Id = id,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Nationality = request.Nationality,
                IsSpecialGuest = request.IsSpecialGuest
            };

            // Misafiri güncellemek için servisi çağırıyorum.
            var result = await _guestService.UpdateGuest(updateGuestDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir misafiri siler (soft delete)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Misafir başarıyla silindi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Misafiri silmek için servisi çağırıyorum.
            var result = await _guestService.DeleteGuest(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Misafir detayını getirir (geçmiş ile - transferler, turlar, faturalar)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir detay bilgileri</returns>
        /// <response code="200">Misafir detayı başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestDetail(int id)
        {
            try
            {
                var result = await _guestService.GetGuestDetailAsync(id);
                return Success(result, "Misafir detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir faturalarını getirir
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir faturaları listesi</returns>
        /// <response code="200">Faturalar başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/invoices")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestInvoices(int id)
        {
            try
            {
                var result = await _guestService.GetGuestInvoicesAsync(id);
                return Success(result, "Misafir faturaları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir faturaları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir zaman çizelgesini getirir (transferler, turlar kronolojik sırada)
        /// </summary>
        /// <param name="id">Misafir ID'si</param>
        /// <returns>Misafir zaman çizelgesi</returns>
        /// <response code="200">Zaman çizelgesi başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestTimeline(int id)
        {
            try
            {
                var result = await _guestService.GetGuestTimelineAsync(id);
                return Success(result, "Misafir zaman çizelgesi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir zaman çizelgesi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}