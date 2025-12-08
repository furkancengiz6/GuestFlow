using GuestFlow.Api.Models;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Email.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// E-posta yönetimi için API endpoint'leri (kuyruk, şablon, geçmiş, istatistikler)
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    [Tags("E-postalar")]
    public class EmailsController : BaseController
    {
        private readonly IEmailQueueService _emailQueueService;
        private readonly IEmailTemplateService _emailTemplateService;
        private readonly IEmailHistoryService _emailHistoryService;
        private readonly IEmailStatisticsService _emailStatisticsService;
        private readonly IEmailService _emailService;

        public EmailsController(
            IEmailQueueService emailQueueService,
            IEmailTemplateService emailTemplateService,
            IEmailHistoryService emailHistoryService,
            IEmailStatisticsService emailStatisticsService,
            IEmailService emailService)
        {
            _emailQueueService = emailQueueService;
            _emailTemplateService = emailTemplateService;
            _emailHistoryService = emailHistoryService;
            _emailStatisticsService = emailStatisticsService;
            _emailService = emailService;
        }

        #region Queue Management

        /// <summary>
        /// E-posta kuyruğa ekler
        /// </summary>
        /// <param name="request">E-posta kuyruk bilgileri</param>
        /// <returns>Kuyruğa eklenen e-posta bilgileri</returns>
        /// <response code="200">E-posta başarıyla kuyruğa eklendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("queue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddToQueue([FromBody] CreateEmailQueueDto request)
        {
            try
            {
                var result = await _emailQueueService.AddToQueueAsync(request);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta kuyruğa eklenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta kuyruğunu getirir
        /// </summary>
        /// <param name="status">Durum filtresi (opsiyonel)</param>
        /// <param name="priority">Öncelik filtresi (opsiyonel)</param>
        /// <returns>E-posta kuyruğu listesi</returns>
        /// <response code="200">E-posta kuyruğu başarıyla getirildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("queue")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQueue([FromQuery] string? status = null, [FromQuery] int? priority = null)
        {
            try
            {
                var result = await _emailQueueService.GetQueueAsync(status, priority);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta kuyruğu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Başarısız e-postaları tekrar dener
        /// </summary>
        /// <param name="maxRetryCount">Maksimum deneme sayısı (varsayılan: 3)</param>
        /// <returns>Yeniden deneme sonucu</returns>
        /// <response code="200">Başarısız e-postalar başarıyla yeniden denendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("queue/retry")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RetryFailedEmails([FromQuery] int maxRetryCount = 3)
        {
            try
            {
                var result = await _emailQueueService.RetryFailedEmailsAsync(maxRetryCount);
                if (result.IsSuccess)
                {
                    return Success(result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Başarısız e-postalar tekrar denenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Eski kuyruk kayıtlarını temizler
        /// </summary>
        /// <param name="daysOld">Temizlenecek kayıtların yaşı (gün, varsayılan: 30)</param>
        /// <returns>Temizleme sonucu</returns>
        /// <response code="200">Eski kuyruk kayıtları başarıyla temizlendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("queue/clear")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClearOldQueueItems([FromQuery] int daysOld = 30)
        {
            try
            {
                var result = await _emailQueueService.ClearOldQueueItemsAsync(daysOld);
                if (result.IsSuccess)
                {
                    return Success(result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Eski kuyruk kayıtları temizlenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        #endregion

        #region Template Management

        /// <summary>
        /// E-posta şablonu oluşturur
        /// </summary>
        /// <param name="request">E-posta şablon bilgileri</param>
        /// <returns>Oluşturulan şablon bilgileri</returns>
        /// <response code="200">E-posta şablonu başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("templates")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateEmailTemplateDto request)
        {
            try
            {
                var result = await _emailTemplateService.CreateTemplateAsync(request);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta şablonu oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta şablonlarını getirir
        /// </summary>
        /// <param name="category">Kategori filtresi (opsiyonel)</param>
        /// <param name="isActive">Aktif durum filtresi (opsiyonel)</param>
        /// <returns>E-posta şablonları listesi</returns>
        /// <response code="200">E-posta şablonları başarıyla getirildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("templates")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTemplates([FromQuery] string? category = null, [FromQuery] bool? isActive = null)
        {
            try
            {
                var result = await _emailTemplateService.GetTemplatesAsync(category, isActive);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta şablonları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta şablonunu getirir
        /// </summary>
        /// <param name="id">Şablon ID'si</param>
        /// <returns>E-posta şablon bilgileri</returns>
        /// <response code="200">E-posta şablonu başarıyla getirildi</response>
        /// <response code="404">Şablon bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("templates/{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTemplate(int id)
        {
            try
            {
                var result = await _emailTemplateService.GetTemplateAsync(id);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 404);
            }
            catch (Exception ex)
            {
                return Error("E-posta şablonu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta şablonunu günceller
        /// </summary>
        /// <param name="id">Şablon ID'si</param>
        /// <param name="request">Güncellenecek şablon bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">E-posta şablonu başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Şablon bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("templates/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] CreateEmailTemplateDto request)
        {
            try
            {
                var result = await _emailTemplateService.UpdateTemplateAsync(id, request);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta şablonu güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta şablonunu siler
        /// </summary>
        /// <param name="id">Şablon ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">E-posta şablonu başarıyla silindi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("templates/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            try
            {
                var result = await _emailTemplateService.DeleteTemplateAsync(id);
                if (result.IsSuccess)
                {
                    return Success(result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta şablonu silinirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Şablonu render eder (test için)
        /// </summary>
        /// <param name="id">Şablon ID'si</param>
        /// <param name="variables">Şablon değişkenleri</param>
        /// <returns>Render edilmiş şablon içeriği</returns>
        /// <response code="200">Şablon başarıyla render edildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("templates/{id}/render")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RenderTemplate(int id, [FromBody] Dictionary<string, string> variables)
        {
            try
            {
                var result = await _emailTemplateService.RenderTemplateBodyAsync(id, variables);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(new { RenderedBody = result.Data }, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Şablon render edilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        #endregion

        #region History

        /// <summary>
        /// E-posta geçmişini getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <param name="status">Durum filtresi (opsiyonel)</param>
        /// <param name="to">Alıcı e-posta filtresi (opsiyonel)</param>
        /// <param name="templateName">Şablon adı filtresi (opsiyonel)</param>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 50)</param>
        /// <returns>Sayfalanmış e-posta geçmişi listesi</returns>
        /// <response code="200">E-posta geçmişi başarıyla getirildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("history")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEmailHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null,
            [FromQuery] string? to = null,
            [FromQuery] string? templateName = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _emailHistoryService.GetEmailHistoryAsync(startDate, endDate, status, to, templateName, pageNumber, pageSize);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta geçmişini getirir (ID ile)
        /// </summary>
        /// <param name="id">E-posta geçmişi ID'si</param>
        /// <returns>E-posta geçmişi bilgileri</returns>
        /// <response code="200">E-posta geçmişi başarıyla getirildi</response>
        /// <response code="404">E-posta geçmişi bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("history/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetEmailHistoryById(int id)
        {
            try
            {
                var result = await _emailHistoryService.GetEmailHistoryByIdAsync(id);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 404);
            }
            catch (Exception ex)
            {
                return Error("E-posta geçmişi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta açıldı olarak işaretler (tracking için)
        /// </summary>
        /// <param name="id">E-posta geçmişi ID'si</param>
        /// <returns>1x1 transparent pixel (tracking için)</returns>
        /// <response code="200">E-posta açıldı olarak işaretlendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("history/{id}/opened")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> MarkEmailAsOpened(int id)
        {
            try
            {
                var result = await _emailHistoryService.MarkEmailAsOpenedAsync(id);
                if (result.IsSuccess)
                {
                    // 1x1 transparent pixel döndür
                    var pixel = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
                    return File(pixel, "image/gif");
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta açıldı işaretlenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// E-posta tıklama sayısını artırır (tracking için)
        /// </summary>
        /// <param name="id">E-posta geçmişi ID'si</param>
        /// <returns>Artırma sonucu</returns>
        /// <response code="200">E-posta tıklama sayısı başarıyla artırıldı</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        [HttpPost("history/{id}/click")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IncrementClickCount(int id)
        {
            try
            {
                var result = await _emailHistoryService.IncrementClickCountAsync(id);
                if (result.IsSuccess)
                {
                    return Success(result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta tıklama sayısı artırılırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// E-posta istatistiklerini getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>E-posta istatistik verileri</returns>
        /// <response code="200">E-posta istatistikleri başarıyla getirildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _emailStatisticsService.GetStatisticsAsync(startDate, endDate);
                if (result.IsSuccess && result.Data != null)
                {
                    return Success(result.Data, result.Message);
                }
                return Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("E-posta istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        #endregion

        #region Bulk Email

        /// <summary>
        /// Toplu e-posta gönderir
        /// </summary>
        /// <param name="request">Toplu e-posta gönderim bilgileri</param>
        /// <returns>Toplu gönderim sonucu (başarılı/başarısız sayıları)</returns>
        /// <response code="200">Toplu e-posta gönderimi tamamlandı</response>
        /// <response code="400">Geçersiz istek verisi (alıcı listesi boş olamaz)</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendBulkEmail([FromBody] BulkEmailRequestDto request)
        {
            try
            {
                if (request.Recipients == null || request.Recipients.Count == 0)
                {
                    return Error("Alıcı listesi boş olamaz.", 400);
                }

                var results = new List<object>();
                var errors = new List<string>();

                foreach (var recipient in request.Recipients)
                {
                    var queueRequest = new CreateEmailQueueDto
                    {
                        To = recipient,
                        Subject = request.Subject,
                        Body = request.Body,
                        IsHtml = request.IsHtml,
                        TemplateName = request.TemplateName,
                        TemplateVariables = request.TemplateVariables,
                        Attachments = request.Attachments,
                        Priority = request.Priority,
                        ScheduledDate = request.ScheduledDate
                    };

                    var result = await _emailQueueService.AddToQueueAsync(queueRequest);
                    if (result.IsSuccess)
                    {
                        results.Add(new { Recipient = recipient, Status = "Queued" });
                    }
                    else
                    {
                        errors.Add($"{recipient}: {result.Message}");
                    }
                }

                return Success(new
                {
                    Successful = results,
                    Failed = errors,
                    Total = request.Recipients.Count,
                    Queued = results.Count,
                    FailedCount = errors.Count
                }, $"{results.Count} e-posta kuyruğa eklendi, {errors.Count} e-posta başarısız oldu.");
            }
            catch (Exception ex)
            {
                return Error("Toplu e-posta gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        #endregion
    }
}

