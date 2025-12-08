using GuestFlow.Api.Models;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Notification.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Bildirim yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    [Tags("Bildirimler")]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Bildirim oluşturur ve gönderir
        /// </summary>
        /// <param name="dto">Bildirim oluşturma bilgileri</param>
        /// <returns>Oluşturulan bildirim bilgileri</returns>
        /// <response code="200">Bildirim başarıyla oluşturuldu ve gönderildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
        {
            try
            {
                var result = await _notificationService.CreateAndSendNotificationAsync(dto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim oluşturulurken hata oluştu.");
                return Error("Bildirim oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim listesini getirir (sayfalama ile)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="notificationType">Bildirim tipi filtresi (opsiyonel)</param>
        /// <param name="status">Durum filtresi (opsiyonel)</param>
        /// <param name="recipientPersonnelId">Alıcı personel ID filtresi (opsiyonel)</param>
        /// <param name="recipientGuestId">Alıcı misafir ID filtresi (opsiyonel)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi filtresi (opsiyonel)</param>
        /// <returns>Sayfalanmış bildirim listesi</returns>
        /// <response code="200">Bildirimler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<object>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? notificationType = null,
            [FromQuery] string? status = null,
            [FromQuery] int? recipientPersonnelId = null,
            [FromQuery] int? recipientGuestId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _notificationService.GetNotificationsPagedAsync(
                    pageNumber, pageSize, notificationType, status, recipientPersonnelId, recipientGuestId, startDate, endDate);
                return PagedResult(result, "Bildirimler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirimler getirilirken hata oluştu.");
                return Error("Bildirimler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Kullanıcının bildirimlerini getirir
        /// </summary>
        /// <param name="unreadOnly">Sadece okunmamış bildirimleri getir (varsayılan: false)</param>
        /// <returns>Kullanıcı bildirimleri listesi</returns>
        /// <response code="200">Kullanıcı bildirimleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("my")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] bool? unreadOnly = false)
        {
            try
            {
                // Kullanıcı ID'sini claim'den al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id");
                var personnelId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int id) ? id : (int?)null;

                var notifications = await _notificationService.GetUserNotificationsAsync(personnelId, null, unreadOnly);
                return Success(notifications, "Bildirimler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bildirimleri getirilirken hata oluştu.");
                return Error("Bildirimler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim detayını getirir
        /// </summary>
        /// <param name="id">Bildirim ID'si</param>
        /// <returns>Bildirim detay bilgileri</returns>
        /// <response code="200">Bildirim başarıyla getirildi</response>
        /// <response code="404">Bildirim bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetNotification(int id)
        {
            try
            {
                var notification = await _notificationService.GetNotificationByIdAsync(id);
                if (notification == null)
                    return Error("Bildirim bulunamadı.", 404);

                return Success(notification, "Bildirim başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim getirilirken hata oluştu: {id}");
                return Error("Bildirim getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim şablonlarını getirir
        /// </summary>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                var templates = await _notificationService.GetTemplatesAsync();
                return Success(templates, "Bildirim şablonları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim şablonları getirilirken hata oluştu.");
                return Error("Bildirim şablonları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim şablonunu getirir
        /// </summary>
        [HttpGet("templates/{templateName}")]
        public async Task<IActionResult> GetTemplate(string templateName)
        {
            try
            {
                var template = await _notificationService.GetTemplateAsync(templateName);
                if (template == null)
                    return Error("Şablon bulunamadı.", 404);

                return Success(template, "Şablon başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şablon getirilirken hata oluştu: {templateName}");
                return Error("Şablon getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Şablon kullanarak bildirim gönderir
        /// </summary>
        [HttpPost("send-with-template")]
        public async Task<IActionResult> SendWithTemplate([FromBody] SendNotificationWithTemplateRequest request)
        {
            try
            {
                var result = await _notificationService.SendNotificationWithTemplateAsync(
                    request.TemplateName,
                    request.RecipientEmail,
                    request.Variables ?? new Dictionary<string, string>(),
                    request.RelatedEntityType,
                    request.RelatedEntityId);

                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şablon ile bildirim gönderilirken hata oluştu.");
                return Error("Bildirim gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Test e-postası gönderir
        /// </summary>
        [HttpPost("test-email")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendTestEmail([FromBody] SendTestEmailDto dto)
        {
            try
            {
                var result = await _notificationService.SendTestEmailAsync(dto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test e-postası gönderilirken hata oluştu.");
                return Error("Test e-postası gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim istatistiklerini getirir
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var statistics = await _notificationService.GetNotificationStatisticsAsync(startDate, endDate);
                return Success(statistics, "Bildirim istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim istatistikleri getirilirken hata oluştu.");
                return Error("Bildirim istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim geçmişini getirir
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? notificationType = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _notificationService.GetNotificationsPagedAsync(
                    pageNumber, pageSize, notificationType, status, null, null, startDate, endDate);
                return PagedResult(result, "Bildirim geçmişi başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim geçmişi getirilirken hata oluştu.");
                return Error("Bildirim geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        /// <param name="id">Bildirim ID'si</param>
        /// <returns>İşaretleme sonucu</returns>
        /// <response code="200">Bildirim başarıyla okundu olarak işaretlendi</response>
        /// <response code="404">Bildirim bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPatch("{id}/read")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var result = await _notificationService.MarkNotificationAsReadAsync(id);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim okundu işaretlenirken hata oluştu: {id}");
                return Error("Bildirim okundu işaretlenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirimi siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var result = await _notificationService.DeleteNotificationAsync(id);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim silinirken hata oluştu: {id}");
                return Error("Bildirim silinirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Şablon ile bildirim gönderme isteği
    /// </summary>
    public class SendNotificationWithTemplateRequest
    {
        public string TemplateName { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public Dictionary<string, string>? Variables { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
}

