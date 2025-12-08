using GuestFlow.Api.Models;
using GuestFlow.Api.Models.SmsModels;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Sms;
using GuestFlow.Application.Operations.Sms.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// SMS yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("SMS")]
    public class SmsController : BaseController
    {
        private readonly ISmsService _smsService;

        public SmsController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        /// <summary>
        /// SMS gönderir
        /// </summary>
        /// <param name="request">SMS gönderim bilgileri</param>
        /// <returns>SMS gönderim sonucu</returns>
        /// <response code="200">SMS başarıyla gönderildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendSms(SendSmsRequest request)
        {
            var dto = new SendSmsDto
            {
                PhoneNumber = request.PhoneNumber,
                Message = request.Message,
                GuestId = request.GuestId,
                PersonnelId = request.PersonnelId,
                RelatedEntityType = request.RelatedEntityType,
                RelatedEntityId = request.RelatedEntityId,
                SmsType = request.SmsType,
                TemplateName = request.TemplateName
            };

            var result = await _smsService.SendSmsAsync(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Transfer hatırlatma SMS'i gönderir
        /// </summary>
        /// <param name="transferId">Transfer ID'si</param>
        /// <param name="hoursBefore">Kaç saat önce hatırlatılacak (varsayılan: 24)</param>
        /// <returns>SMS gönderim sonucu</returns>
        /// <response code="200">Transfer hatırlatma SMS'i başarıyla gönderildi</response>
        /// <response code="404">Transfer bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("transfer-reminder/{transferId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTransferReminder(int transferId, [FromQuery] int hoursBefore = 24)
        {
            var result = await _smsService.SendTransferReminderAsync(transferId, hoursBefore);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Tur hatırlatma SMS'i gönderir
        /// </summary>
        /// <param name="tourType">Tur tipi</param>
        /// <param name="tourId">Tur ID'si</param>
        /// <param name="hoursBefore">Kaç saat önce hatırlatılacak (varsayılan: 24)</param>
        /// <returns>SMS gönderim sonucu</returns>
        /// <response code="200">Tur hatırlatma SMS'i başarıyla gönderildi</response>
        /// <response code="404">Tur bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("tour-reminder/{tourType}/{tourId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendTourReminder(string tourType, int tourId, [FromQuery] int hoursBefore = 24)
        {
            var result = await _smsService.SendTourReminderAsync(tourType, tourId, hoursBefore);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Rezervasyon onay SMS'i gönderir
        /// </summary>
        /// <param name="reservationId">Rezervasyon ID'si</param>
        /// <returns>SMS gönderim sonucu</returns>
        /// <response code="200">Rezervasyon onay SMS'i başarıyla gönderildi</response>
        /// <response code="404">Rezervasyon bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("reservation-confirmation/{reservationId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendReservationConfirmation(int reservationId)
        {
            var result = await _smsService.SendReservationConfirmationAsync(reservationId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// SMS geçmişini ID'ye göre getirir
        /// </summary>
        /// <param name="id">SMS geçmişi ID'si</param>
        /// <returns>SMS geçmişi bilgileri</returns>
        /// <response code="200">SMS geçmişi başarıyla getirildi</response>
        /// <response code="404">SMS kaydı bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetSmsHistoryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSmsHistoryById(int id)
        {
            try
            {
                var result = await _smsService.GetSmsHistoryByIdAsync(id);
                if (result == null)
                    return NotFound("SMS kaydı bulunamadı.");

                return Success(result, "SMS kaydı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("SMS kaydı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm SMS geçmişini getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="personnelId">Personel ID filtresi</param>
        /// <param name="status">Durum filtresi</param>
        /// <param name="smsType">SMS tipi filtresi</param>
        /// <param name="relatedEntityType">İlişkili entity tipi filtresi</param>
        /// <param name="relatedEntityId">İlişkili entity ID filtresi</param>
        /// <param name="provider">SMS sağlayıcı filtresi</param>
        /// <param name="phoneNumber">Telefon numarası filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış SMS geçmişi listesi</returns>
        /// <response code="200">SMS geçmişi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetSmsHistoryDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSmsHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? smsType = null,
            [FromQuery] string? relatedEntityType = null,
            [FromQuery] int? relatedEntityId = null,
            [FromQuery] string? provider = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new SmsFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                GuestId = guestId,
                PersonnelId = personnelId,
                Status = status,
                SmsType = smsType,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                Provider = provider,
                PhoneNumber = phoneNumber,
                SearchTerm = searchTerm
            };

            // Sıralama parametrelerini oluştur
            var sorting = new SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _smsService.GetSmsHistoryPagedAsync(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetSmsHistoryDto>(result, "SMS geçmişi başarıyla getirildi.");
        }

        /// <summary>
        /// Misafire gönderilen SMS'leri getirir
        /// </summary>
        /// <param name="guestId">Misafir ID'si</param>
        /// <returns>Misafir SMS geçmişi listesi</returns>
        /// <response code="200">Misafir SMS geçmişi başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSmsHistoryByGuestId(int guestId)
        {
            try
            {
                var result = await _smsService.GetSmsHistoryByGuestIdAsync(guestId);
                return Success(result, "Misafir SMS geçmişi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir SMS geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Duruma göre SMS'leri getirir
        /// </summary>
        /// <param name="status">SMS durumu</param>
        /// <returns>Duruma göre SMS geçmişi listesi</returns>
        /// <response code="200">Duruma göre SMS geçmişi başarıyla getirildi</response>
        /// <response code="400">Geçersiz durum</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSmsHistoryByStatus(string status)
        {
            try
            {
                var result = await _smsService.GetSmsHistoryByStatusAsync(status);
                return Success(result, "Duruma göre SMS geçmişi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Duruma göre SMS geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// SMS durumunu günceller (gateway callback için)
        /// </summary>
        /// <param name="id">SMS geçmişi ID'si</param>
        /// <param name="request">SMS durum güncelleme bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">SMS durumu başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">SMS kaydı bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSmsStatus(int id, [FromBody] UpdateSmsStatusRequest request)
        {
            var result = await _smsService.UpdateSmsStatusAsync(id, request.Status, request.MessageId, request.GatewayResponse);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// SMS istatistiklerini getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>SMS istatistik verileri</returns>
        /// <response code="200">SMS istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSmsStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _smsService.GetSmsStatisticsAsync(startDate, endDate);
                return Success(result, "SMS istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("SMS istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// SMS durumu güncelleme request modeli
    /// </summary>
    public class UpdateSmsStatusRequest
    {
        public string Status { get; set; }
        public string? MessageId { get; set; }
        public string? GatewayResponse { get; set; }
    }
}

