using GuestFlow.Api.Models;
using GuestFlow.Api.Models.CityTourModels;
using GuestFlow.Application.Operations.CityTour;
using GuestFlow.Application.Operations.CityTour.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Şehir turu yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Reception,Concierge,Manager,Admin,Owner")] // Şehir turu işlemleri için gerekli roller
    [Tags("Şehir Turları")]
    public class CityToursController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _cityTourService: Şehir turlarıyla ilgili işlemleri yapmak için kullanıyorum.
        private readonly ICityTourService _cityTourService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public CityToursController(ICityTourService cityTourService)
        {
            _cityTourService = cityTourService;
        }

        /// <summary>
        /// Yeni bir şehir turu ekler
        /// </summary>
        /// <param name="request">Şehir turu bilgileri</param>
        /// <returns>Oluşturulan şehir turu bilgileri</returns>
        /// <response code="200">Şehir turu başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AddCityTourResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddCityTour(AddCityTourRequest request)
        {
            // Giriş yapmış kullanıcının ID'sini al (otomatik personel atama için)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
            int? currentPersonnelId = null;
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int personnelId))
            {
                currentPersonnelId = personnelId;
            }

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddCityTourDto
            {
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId ?? currentPersonnelId, // Otomatik doldurulacak
                CityId = request.CityId,
                TourId = request.TourId,
                CreateInvoice = request.CreateInvoice,
                DiscountPercentage = request.DiscountPercentage,
                InvoiceDescription = request.InvoiceDescription,
                Currency = request.Currency,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                VehicleId = request.VehicleId,
                DriverName = request.DriverName,
                GuideName = request.GuideName,
                GuidePhone = request.GuidePhone,
                ExternalVehiclePlate = request.ExternalVehiclePlate,
                ExternalDriverName = request.ExternalDriverName,
                ExternalDriverPhone = request.ExternalDriverPhone,
                SupplierName = request.SupplierName,
                SupplierCost = request.SupplierCost,
                SupplierCurrency = request.SupplierCurrency,
                SupplierInvoiceNumber = request.SupplierInvoiceNumber
            };

            // Şehir turunu eklemek için servisi çağırıyorum.
            var result = await _cityTourService.AddCityTour(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Belirli bir şehir turunu ID'sine göre getirir
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <returns>Şehir turu bilgileri</returns>
        /// <response code="200">Şehir turu başarıyla getirildi</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetCityTourDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten şehir turunu ID'sine göre alıyorum.
            var result = await _cityTourService.GetCityTourById(id);
            if (result == null)
                return NotFound("Şehir turu bulunamadı.");
            
            return Success(result, "Şehir turu başarıyla getirildi.");
        }

        /// <summary>
        /// Tüm şehir turlarını getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
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
        /// <returns>Sayfalanmış şehir turu listesi</returns>
        /// <response code="200">Şehir turu listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetCityTourDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCityTours(
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
            var filters = new GuestFlow.Application.Models.CityTourFilterParameters
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

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış şehir turlarını alıyorum ve JSON formatında döndürüyorum.
            var result = await _cityTourService.GetCityToursPaged(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetCityTourDto>(result, "Şehir turları başarıyla getirildi.");
        }

        /// <summary>
        /// Mevcut bir şehir turunu günceller
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <param name="request">Güncellenecek şehir turu bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Şehir turu başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateCityTourRequest request)
        {
            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateCityTourDto = new UpdateCityTourDto
            {
                Id = id,
                TourDate = request.TourDate,
                Language = request.Language,
                DurationHours = request.DurationHours,
                Price = request.Price,
                OwnerGuestId = request.OwnerGuestId,
                PersonnelId = request.PersonnelId,
                CityId = request.CityId,
                TourId = request.TourId ?? 0,
                DiscountPercentage = request.DiscountPercentage,
                Currency = request.Currency,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                VehicleId = request.VehicleId,
                DriverName = request.DriverName,
                GuideName = request.GuideName,
                GuidePhone = request.GuidePhone,
                ExternalVehiclePlate = request.ExternalVehiclePlate,
                ExternalDriverName = request.ExternalDriverName,
                ExternalDriverPhone = request.ExternalDriverPhone
            };

            // Şehir turunu güncellemek için servisi çağırıyorum.
            var result = await _cityTourService.UpdateCityTour(updateCityTourDto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bir şehir turunu siler (soft delete)
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Şehir turu başarıyla silindi</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Manager,Admin,Owner")] // Sadece yönetim rolleri silebilir
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Şehir turunu silmek için servisi çağırıyorum.
            var result = await _cityTourService.DeleteCityTour(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Şehir turu detayını getirir (ilgili veriler ile)
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <returns>Şehir turu detay bilgileri</returns>
        /// <response code="200">Şehir turu detayı başarıyla getirildi</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCityTourDetail(int id)
        {
            try
            {
                var result = await _cityTourService.GetCityTourDetailAsync(id);
                return Success(result, "Şehir turu detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Şehir turu detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Şehir turu için fatura oluşturur
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <returns>Fatura oluşturma sonucu</returns>
        /// <response code="200">Fatura başarıyla oluşturuldu</response>
        /// <response code="400">Fatura oluşturulamadı</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/invoice")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateCityTourInvoice(int id)
        {
            try
            {
                var result = await _cityTourService.CreateCityTourInvoiceAsync(id);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Fatura oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Şehir turu onay maili gönderir
        /// </summary>
        /// <param name="id">Şehir turu ID'si</param>
        /// <returns>Onay maili gönderme sonucu</returns>
        /// <response code="200">Onay maili başarıyla gönderildi</response>
        /// <response code="400">Mail gönderilemedi</response>
        /// <response code="404">Şehir turu bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/send-confirmation")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendCityTourConfirmation(int id)
        {
            try
            {
                var result = await _cityTourService.SendCityTourConfirmationAsync(id);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Onay maili gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Update city tour status (mark completed/cancelled)
        /// </summary>
        /// <param name="id">City tour ID</param>
        /// <param name="request">Status update request</param>
        /// <returns>Operation result</returns>
        /// <response code="200">Status updated successfully</response>
        /// <response code="400">Invalid request</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Concierge,Manager,Admin,Owner")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCityTourStatus(int id, [FromBody] UpdateCityTourStatusRequest request)
        {
            try
            {
                var result = await _cityTourService.UpdateCityTourStatusAsync(id, request.Status);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Şehir turu durumu güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}