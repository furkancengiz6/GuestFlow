using GuestFlow.Api.Models;
using GuestFlow.Api.Models.TransferModel;
using System;
using GuestFlow.Application.Operations.Transfer.Dtos;
using GuestFlow.Application.Operations.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Transfer yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [Tags("Transferler")]
    public class TransfersController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _transferService: Transfer işlemleriyle ilgili işlemleri yapmak için kullanıyorum.
        private readonly ITransferService _transferService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public TransfersController(ITransferService transferService)
        {
            _transferService = transferService;
        }

        /// <summary>
        /// Yeni bir transfer kaydı ekler
        /// </summary>
        /// <param name="request">Transfer bilgileri</param>
        /// <returns>Oluşturulan transfer bilgileri</returns>
        /// <response code="200">Transfer başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <example>
        /// <code>
        /// POST /api/v1/transfers
        /// {
        ///   "transferDate": "2024-12-15T10:00:00",
        ///   "pickupAddress": "Istanbul Airport",
        ///   "dropoffAddress": "Grand Hotel, Taksim",
        ///   "price": 500.00,
        ///   "guestId": 1,
        ///   "personnelId": 1,
        ///   "airportId": 1,
        ///   "vehicleId": 1,
        ///   "isFromAirport": true,
        ///   "pickupCityId": 34,
        ///   "dropoffCityId": 34,
        ///   "createInvoice": true,
        ///   "currency": "USD",
        ///   "note": "VIP guest"
        /// }
        /// </code>
        /// </example>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Add(AddTransferRequest request)
        {
            // Gelen isteğin doğruluğunu kontrol ediyorum. Eğer model geçersizse, hata döndürüyorum.
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum ki serviste kullanabileyim.
            var dto = new AddTransferDto
            {
                TransferDate = request.TransferDate,
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
                Price = request.Price,
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                AirportId = request.AirportId,
                VehicleId = request.VehicleId,
                Note = request.Note,
                Status = request.Status,
                IsFromAirport = request.IsFromAirport,
                PickupCityId = request.PickupCityId,
                DropoffCityId = request.DropoffCityId,
                    CreateInvoice = request.CreateInvoice,
                    DiscountPercentage = request.DiscountPercentage,
                    InvoiceDescription = request.InvoiceDescription,
                    Currency = request.Currency
                };

            // Transferi eklemek için servisi çağırıyorum.
            var result = await _transferService.AddTransfer(dto);
            // Standart API yanıt formatını kullan
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tüm transferleri getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="status">Durum filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="personnelId">Personel ID filtresi</param>
        /// <param name="vehicleId">Araç ID filtresi</param>
        /// <param name="airportId">Havalimanı ID filtresi</param>
        /// <param name="isFromAirport">Havalimanından mı filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: asc)</param>
        /// <returns>Sayfalanmış transfer listesi</returns>
        /// <response code="200">Transfer listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetTransferDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransfers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] int? airportId = null,
            [FromQuery] bool? isFromAirport = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "asc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new GuestFlow.Application.Models.TransferFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                Status = status,
                GuestId = guestId,
                PersonnelId = personnelId,
                VehicleId = vehicleId,
                AirportId = airportId,
                IsFromAirport = isFromAirport,
                SearchTerm = searchTerm
            };

            // Sıralama parametrelerini oluştur
            var sorting = new GuestFlow.Application.Models.SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış transferleri alıyorum ve JSON formatında döndürüyorum.
            var result = await _transferService.GetTransfersPaged(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetTransferDto>(result, "Transferler başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir transferi ID'sine göre getirir
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <returns>Transfer bilgileri</returns>
        /// <response code="200">Transfer başarıyla getirildi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetTransferDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten transferi ID'sine göre alıyorum.
            var result = await _transferService.GetTransferById(id);
            // Standart API yanıt formatını kullan
            return result == null ? NotFound("Transfer bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Mevcut bir transferi günceller
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <param name="request">Güncellenecek transfer bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Transfer başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, UpdateTransferRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Gelen isteği bir DTO'ya çeviriyorum.
            var updateTransferDto = new UpdateTransferDto
            {
                Id = id,
                TransferDate = request.TransferDate,
                PickupAddress = request.PickupAddress,
                DropoffAddress = request.DropoffAddress,
                Price = request.Price,
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                AirportId = request.AirportId,
                VehicleId = request.VehicleId,
                Note = request.Note,
                Status = request.Status,
                IsFromAirport = request.IsFromAirport,
                PickupCityId = request.PickupCityId,
                DropoffCityId = request.DropoffCityId
            };

            // Transferi güncellemek için servisi çağırıyorum.
            var result = await _transferService.UpdateTransfer(updateTransferDto);
            return result.IsSuccess ? Ok(new { Message = result.Message }) : BadRequest(new { Message = result.Message });
        }

        /// <summary>
        /// Bir transferi siler (soft delete)
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Transfer başarıyla silindi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            // Transferi silmek için servisi çağırıyorum.
            var result = await _transferService.DeleteTransfer(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Transfer detayını getirir (ilgili veriler ile)
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <returns>Transfer detay bilgileri</returns>
        /// <response code="200">Transfer detayı başarıyla getirildi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransferDetail(int id)
        {
            try
            {
                var result = await _transferService.GetTransferDetailAsync(id);
                return Success(result, "Transfer detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Transfer detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer takvim görünümünü getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Transfer takvim verileri</returns>
        /// <response code="200">Transfer takvimi başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("calendar")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransferCalendar(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _transferService.GetTransferCalendarAsync(startDate, endDate);
                return Success(result, "Transfer takvimi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Transfer takvimi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer istatistiklerini getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Transfer istatistikleri</returns>
        /// <response code="200">Transfer istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransferStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _transferService.GetTransferStatisticsAsync(startDate, endDate);
                return Success(result, "Transfer istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Transfer istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer durumunu günceller (iş akışı için)
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <param name="request">Yeni durum bilgisi</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Transfer durumu başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTransferStatus(int id, [FromBody] UpdateTransferStatusRequest request)
        {
            try
            {
                var result = await _transferService.UpdateTransferStatusAsync(id, request.Status);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Transfer durumu güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Transfer'e araç atar
        /// </summary>
        /// <param name="id">Transfer ID'si</param>
        /// <param name="request">Araç ID bilgisi</param>
        /// <returns>Atama sonucu</returns>
        /// <response code="200">Araç başarıyla atandı</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Transfer veya araç bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPatch("{id}/assign-vehicle")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AssignVehicle(int id, [FromBody] AssignVehicleRequest request)
        {
            try
            {
                var result = await _transferService.AssignVehicleAsync(id, request.VehicleId);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Araç atanırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}