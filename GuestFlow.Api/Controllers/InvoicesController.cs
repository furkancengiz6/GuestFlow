using GuestFlow.Api.Filters;
using GuestFlow.Api.Models;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Invoice;
using GuestFlow.Application.Operations.Invoice.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Fatura yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")] // Bu controller'a sadece Staff ve Admin rolleri erişebilir.
    [TypeFilter(typeof(LoggingFilter))] // Bu controller'daki tüm işlemler için loglama filtresi uyguluyorum.
    [Tags("Faturalar")]
    public class InvoicesController : BaseController
    {
        // Burada kullanacağım değişkeni tanımlıyorum.
        // _invoiceService: Faturalarla ilgili işlemleri yapmak için kullanıyorum.
        private readonly IInvoiceService _invoiceService;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Belirli bir faturayı ID'sine göre getirir
        /// </summary>
        /// <param name="id">Fatura ID'si</param>
        /// <returns>Fatura bilgileri</returns>
        /// <response code="200">Fatura başarıyla getirildi</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetInvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            // Servisten faturayı ID'sine göre alıyorum.
            var result = await _invoiceService.GetInvoiceById(id);
            // Eğer fatura bulunamazsa, 404 Not Found ile hata mesajı döndürüyorum; bulunursa sonucu JSON formatında döndürüyorum.
            return result == null ? NotFound("Fatura bulunamadı.") : Success(result);
        }

        /// <summary>
        /// Tüm faturaları getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="personnelId">Personel ID filtresi</param>
        /// <param name="currency">Para birimi filtresi</param>
        /// <param name="hasPdf">PDF var mı filtresi</param>
        /// <param name="serviceType">Servis tipi filtresi</param>
        /// <param name="serviceId">Servis ID filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="minAmount">Minimum tutar filtresi</param>
        /// <param name="maxAmount">Maksimum tutar filtresi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış fatura listesi</returns>
        /// <response code="200">Fatura listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetInvoiceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? personnelId = null,
            [FromQuery] string? currency = null,
            [FromQuery] bool? hasPdf = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new InvoiceFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                GuestId = guestId,
                PersonnelId = personnelId,
                Currency = currency,
                HasPdf = hasPdf,
                ServiceType = serviceType,
                ServiceId = serviceId,
                SearchTerm = searchTerm,
                MinAmount = minAmount,
                MaxAmount = maxAmount
            };

            // Sıralama parametrelerini oluştur
            var sorting = new SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Servisten sayfalanmış, filtrelenmiş ve sıralanmış faturaları alıyorum ve JSON formatında döndürüyorum.
            var result = await _invoiceService.GetInvoicesPagedAsync(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetInvoiceDto>(result, "Faturalar başarıyla getirildi.");
        }

        /// <summary>
        /// Belirli bir misafire ait faturaları getirir
        /// </summary>
        /// <param name="guestId">Misafir ID'si</param>
        /// <returns>Misafir faturaları listesi</returns>
        /// <response code="200">Faturalar başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoicesByGuestId(int guestId)
        {
            // Servisten misafire ait faturaları alıyorum ve JSON formatında döndürüyorum.
            var result = await _invoiceService.GetInvoicesByGuestId(guestId);
            return Success(result, "Misafir faturaları başarıyla getirildi.");
        }

        /// <summary>
        /// Fatura detayını getirir (ilgili veriler ile)
        /// </summary>
        /// <param name="id">Fatura ID'si</param>
        /// <returns>Fatura detay bilgileri</returns>
        /// <response code="200">Fatura detayı başarıyla getirildi</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoiceDetail(int id)
        {
            try
            {
                var result = await _invoiceService.GetInvoiceDetailAsync(id);
                return Success(result, "Fatura detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Fatura detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Fatura istatistiklerini getirir
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Fatura istatistikleri</returns>
        /// <response code="200">Fatura istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoiceStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _invoiceService.GetInvoiceStatisticsAsync(startDate, endDate);
                return Success(result, "Fatura istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Fatura istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir fatura için PDF oluşturur veya yeniden oluşturur
        /// </summary>
        /// <param name="id">Fatura ID'si</param>
        /// <returns>PDF URL bilgisi</returns>
        /// <response code="200">PDF başarıyla oluşturuldu</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <example>
        /// <code>
        /// POST /api/v1/invoices/1/generate-pdf
        /// 
        /// Response:
        /// {
        ///   "isSuccess": true,
        ///   "message": "PDF başarıyla oluşturuldu.",
        ///   "data": {
        ///     "pdfUrl": "https://localhost:5146/invoices/invoice_1_20241215.pdf"
        ///   }
        /// }
        /// </code>
        /// </example>
        [HttpPost("{id}/generate-pdf")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            try
            {
                var pdfUrl = await _invoiceService.GeneratePdfForInvoiceAsync(id);
                return Success(new { PdfUrl = pdfUrl }, "PDF başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return Error($"PDF oluşturulurken hata: {ex.Message}", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Faturayı e-posta ile gönderir
        /// </summary>
        /// <param name="id">Fatura ID'si</param>
        /// <param name="request">E-posta gönderim bilgileri (opsiyonel)</param>
        /// <returns>Gönderim sonucu</returns>
        /// <response code="200">Fatura e-postası başarıyla gönderildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/send-email")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendInvoiceByEmail(int id, [FromBody] SendInvoiceEmailRequest? request = null)
        {
            try
            {
                var recipientEmail = request?.RecipientEmail;
                var result = await _invoiceService.SendInvoiceByEmailAsync(id, recipientEmail);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                return Error("Fatura e-postası gönderilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Fatura e-posta gönderme isteği
    /// </summary>
    public class SendInvoiceEmailRequest
    {
        public string? RecipientEmail { get; set; }
    }
}