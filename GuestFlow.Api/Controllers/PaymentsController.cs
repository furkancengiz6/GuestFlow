using GuestFlow.Api.Models;
using GuestFlow.Api.Models.PaymentModels;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Operations.Payment.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GuestFlow.Api.Filters;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Ödeme yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [TwoFactorRequirement]
    [Tags("Ödemeler")]
    public class PaymentsController : BaseController
    {
        private readonly IPaymentService _paymentService;
        private readonly IPaymentStatusService _paymentStatusService;

        public PaymentsController(IPaymentService paymentService, IPaymentStatusService paymentStatusService)
        {
            _paymentService = paymentService;
            _paymentStatusService = paymentStatusService;
        }

        /// <summary>
        /// Yeni ödeme oluşturur
        /// </summary>
        /// <param name="request">Ödeme bilgileri</param>
        /// <returns>Oluşturulan ödeme bilgileri</returns>
        /// <response code="200">Ödeme başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<GetPaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddPayment(AddPaymentRequest request)
        {
            var dto = new AddPaymentDto
            {
                InvoiceId = request.InvoiceId,
                GuestId = request.GuestId,
                CollectedByPersonnelId = request.CollectedByPersonnelId,
                TransferId = request.TransferId,
                CityTourId = request.CityTourId,
                YachtTourId = request.YachtTourId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                PaymentDate = request.PaymentDate,
                Notes = request.Notes
            };

            var result = await _paymentService.AddPaymentAsync(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi günceller
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <param name="request">Güncellenecek ödeme bilgileri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Ödeme başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePayment(int id, UpdatePaymentRequest request)
        {
            var dto = new UpdatePaymentDto
            {
                Id = id,
                InvoiceId = request.InvoiceId,
                TransferId = request.TransferId,
                CityTourId = request.CityTourId,
                YachtTourId = request.YachtTourId,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = request.PaymentMethod,
                Status = request.Status,
                PaymentDate = request.PaymentDate,
                TransactionId = request.TransactionId,
                Notes = request.Notes
            };

            var result = await _paymentService.UpdatePaymentAsync(dto);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi siler (soft delete)
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Ödeme başarıyla silindi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi ID'ye göre getirir
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <returns>Ödeme bilgileri</returns>
        /// <response code="200">Ödeme başarıyla getirildi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<GetPaymentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            try
            {
                var result = await _paymentService.GetPaymentByIdAsync(id);
                if (result == null)
                    return NotFound("Ödeme bulunamadı.");

                return Success(result, "Ödeme başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Ödeme getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Ödeme detayını getirir
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <returns>Ödeme detay bilgileri</returns>
        /// <response code="200">Ödeme detayı başarıyla getirildi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("{id}/detail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentDetail(int id)
        {
            try
            {
                var result = await _paymentService.GetPaymentDetailAsync(id);
                if (result == null)
                    return NotFound("Ödeme bulunamadı.");

                return Success(result, "Ödeme detayı başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Ödeme detayı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm ödemeleri getirir (sayfalanmış, filtrelenmiş ve sıralanmış)
        /// </summary>
        /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi filtresi</param>
        /// <param name="endDate">Bitiş tarihi filtresi</param>
        /// <param name="guestId">Misafir ID filtresi</param>
        /// <param name="invoiceId">Fatura ID filtresi</param>
        /// <param name="status">Durum filtresi</param>
        /// <param name="paymentMethod">Ödeme yöntemi filtresi</param>
        /// <param name="minAmount">Minimum tutar filtresi</param>
        /// <param name="maxAmount">Maksimum tutar filtresi</param>
        /// <param name="currency">Para birimi filtresi</param>
        /// <param name="searchTerm">Arama terimi</param>
        /// <param name="sortBy">Sıralama alanı</param>
        /// <param name="sortOrder">Sıralama yönü (asc/desc, varsayılan: desc)</param>
        /// <returns>Sayfalanmış ödeme listesi</returns>
        /// <response code="200">Ödeme listesi başarıyla getirildi</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<Models.PagedResult<GetPaymentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? guestId = null,
            [FromQuery] int? invoiceId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? paymentMethod = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] string? currency = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = "desc")
        {
            // Filtreleme parametrelerini oluştur
            var filters = new PaymentFilterParameters
            {
                StartDate = startDate,
                EndDate = endDate,
                GuestId = guestId,
                InvoiceId = invoiceId,
                Status = status,
                PaymentMethod = paymentMethod,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Currency = currency,
                SearchTerm = searchTerm
            };

            // Sıralama parametrelerini oluştur
            var sorting = new SortingParameters
            {
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _paymentService.GetPaymentsPagedAsync(pageNumber, pageSize, filters, sorting);
            return PagedResult<GetPaymentDto>(result, "Ödemeler başarıyla getirildi.");
        }

        /// <summary>
        /// Ödemeyi tamamlar (gateway callback için)
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <param name="request">Ödeme tamamlama bilgileri</param>
        /// <returns>Tamamlama sonucu</returns>
        /// <response code="200">Ödeme başarıyla tamamlandı</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/complete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CompletePayment(int id, CompletePaymentRequest request)
        {
            var result = await _paymentService.CompletePaymentAsync(id, request.TransactionId, request.GatewayResponse);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi başarısız olarak işaretler
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <param name="reason">Başarısızlık nedeni (opsiyonel)</param>
        /// <returns>İşaretleme sonucu</returns>
        /// <response code="200">Ödeme başarısız olarak işaretlendi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/fail")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> FailPayment(int id, [FromBody] string? reason = null)
        {
            var result = await _paymentService.FailPaymentAsync(id, reason);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi iade eder
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <param name="request">İade nedeni (opsiyonel)</param>
        /// <returns>İade sonucu</returns>
        /// <response code="200">Ödeme başarıyla iade edildi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/refund")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefundPayment(int id, RefundPaymentRequest? request = null)
        {
            var refundReason = request?.RefundReason;
            var result = await _paymentService.RefundPaymentAsync(id, refundReason);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Ödemeyi iptal eder
        /// </summary>
        /// <param name="id">Ödeme ID'si</param>
        /// <param name="request">İptal nedeni (opsiyonel)</param>
        /// <returns>İptal sonucu</returns>
        /// <response code="200">Ödeme başarıyla iptal edildi</response>
        /// <response code="404">Ödeme bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelPayment(int id, CancelPaymentRequest? request = null)
        {
            var cancellationReason = request?.CancellationReason;
            var result = await _paymentService.CancelPaymentAsync(id, cancellationReason);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Misafire ait ödemeleri getirir
        /// </summary>
        /// <param name="guestId">Misafir ID'si</param>
        /// <returns>Misafir ödemeleri listesi</returns>
        /// <response code="200">Misafir ödemeleri başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-guest/{guestId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentsByGuestId(int guestId)
        {
            try
            {
                var result = await _paymentService.GetPaymentsByGuestIdAsync(guestId);
                return Success(result, "Misafir ödemeleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir ödemeleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafire ait ödeme özetini getirir
        /// </summary>
        /// <param name="guestId">Misafir ID'si</param>
        /// <returns>Misafir ödeme özeti</returns>
        /// <response code="200">Misafir ödeme özeti başarıyla getirildi</response>
        /// <response code="404">Misafir bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-guest/{guestId}/summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentSummaryByGuestId(int guestId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentsByGuestIdAsync(guestId);
                if (payments == null || payments.Count == 0)
                {
                    return Success(new
                    {
                        totalPaid = 0.0m,
                        currency = "TRY",
                        paymentMethodBreakdown = new Dictionary<string, int>(),
                        recentPayments = new List<GetPaymentDto>()
                    }, "Misafir için ödeme bulunamadı.");
                }

                var totalPaid = payments.Sum(p => p.Amount);
                var currency = payments.First().Currency ?? "TRY";

                // Group by payment method and count
                var paymentMethodBreakdown = payments
                    .GroupBy(p => p.PaymentMethod)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Get recent 5 payments ordered by date desc
                var recentPayments = payments
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToList();

                var summary = new
                {
                    totalPaid = totalPaid,
                    currency = currency,
                    paymentMethodBreakdown = paymentMethodBreakdown,
                    recentPayments = recentPayments
                };

                return Success(summary, "Misafir ödeme özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir ödeme özeti getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Faturaya ait ödemeleri getirir
        /// </summary>
        /// <param name="invoiceId">Fatura ID'si</param>
        /// <returns>Fatura ödemeleri listesi</returns>
        /// <response code="200">Fatura ödemeleri başarıyla getirildi</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("by-invoice/{invoiceId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPaymentsByInvoiceId(int invoiceId)
        {
            try
            {
                var result = await _paymentService.GetPaymentsByInvoiceIdAsync(invoiceId);
                return Success(result, "Fatura ödemeleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Fatura ödemeleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Duruma göre ödemeleri getirir
        /// </summary>
        [HttpGet("by-status/{status}")]
        public async Task<IActionResult> GetPaymentsByStatus(string status)
        {
            try
            {
                var result = await _paymentService.GetPaymentsByStatusAsync(status);
                return Success(result, "Duruma göre ödemeler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Duruma göre ödemeler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Servisin ödeme durumunu getirir (canonical calculation)
        /// </summary>
        /// <param name="serviceId">Servis ID'si</param>
        /// <param name="serviceType">Servis tipi (Transfer/CityTour/YachtTour)</param>
        /// <returns>Servis ödeme durumu</returns>
        /// <response code="200">Servis ödeme durumu başarıyla getirildi</response>
        /// <response code="400">Geçersiz servis tipi</response>
        /// <response code="404">Servis bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("service/{serviceId}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetServicePaymentStatus(int serviceId, [FromQuery] string serviceType)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serviceType))
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Servis tipi gereklidir." });

                var validTypes = new[] { "transfer", "citytour", "yachttour" };
                if (!validTypes.Contains(serviceType.ToLower()))
                    return BadRequest(new ApiResponse<object> { Success = false, Message = $"Geçersiz servis tipi. Geçerli tipler: {string.Join(", ", validTypes)}" });

                var result = await _paymentStatusService.GetServicePaymentStatusAsync(serviceId, serviceType);
                return Success(result, "Servis ödeme durumu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Servis ödeme durumu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Faturanın ödeme durumunu getirir (canonical calculation)
        /// </summary>
        /// <param name="invoiceId">Fatura ID'si</param>
        /// <returns>Fatura ödeme durumu</returns>
        /// <response code="200">Fatura ödeme durumu başarıyla getirildi</response>
        /// <response code="404">Fatura bulunamadı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("invoice/{invoiceId}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetInvoicePaymentStatus(int invoiceId)
        {
            try
            {
                var result = await _paymentStatusService.GetInvoicePaymentStatusAsync(invoiceId);
                return Success(result, "Fatura ödeme durumu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Fatura ödeme durumu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

