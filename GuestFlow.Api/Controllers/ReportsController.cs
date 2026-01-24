using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Raporlar ve istatistikler için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Admin ve Staff erişebilir
    [Tags("Raporlar")]
    public class ReportsController : BaseController
    {
        private readonly IReportsService _reportsService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IReportsService reportsService, ILogger<ReportsController> logger)
        {
            _reportsService = reportsService;
            _logger = logger;
        }

        /// <summary>
        /// Gelir özeti (tarih aralığına göre)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Gelir özet bilgileri</returns>
        /// <response code="200">Gelir özeti başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("revenue-summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRevenueSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? personnelId = null)
        {
            try
            {
                var result = await _reportsService.GetRevenueSummaryAsync(startDate, endDate, serviceType, personnelId);
                return Success(result, "Gelir özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelir özeti getirilirken hata oluştu.");
                return Error("Gelir özeti getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Misafir istatistikleri
        /// </summary>
        /// <returns>Misafir istatistik verileri</returns>
        /// <response code="200">Misafir istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("guest-statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestStatistics()
        {
            try
            {
                var result = await _reportsService.GetGuestStatisticsAsync();
                return Success(result, "Misafir istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Misafir istatistikleri getirilirken hata oluştu.");
                return Error("Misafir istatistikleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Tur istatistikleri
        /// </summary>
        /// <param name="tourType">Tur tipi filtresi (opsiyonel)</param>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Tur istatistik verileri</returns>
        /// <response code="200">Tur istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("tour-statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTourStatistics(
            [FromQuery] string? tourType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _reportsService.GetTourStatisticsAsync(tourType, startDate, endDate);
                return Success(result, "Tur istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tur istatistikleri getirilirken hata oluştu.");
                return Error("Tur istatistikleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Transfer istatistikleri
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Transfer istatistik verileri</returns>
        /// <response code="200">Transfer istatistikleri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("transfer-statistics")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTransferStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? personnelId = null)
        {
            try
            {
                var result = await _reportsService.GetTransferStatisticsAsync(startDate, endDate, personnelId);
                return Success(result, "Transfer istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transfer istatistikleri getirilirken hata oluştu.");
                return Error("Transfer istatistikleri getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Aylık gelir dağılımı
        /// </summary>
        /// <param name="year">Yıl (opsiyonel, varsayılan: mevcut yıl)</param>
        /// <returns>Aylık gelir dağılımı verileri</returns>
        /// <response code="200">Aylık gelir başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("monthly-revenue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int? year = null)
        {
            try
            {
                var result = await _reportsService.GetMonthlyRevenueAsync(year);
                return Success(result, "Aylık gelir başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Aylık gelir getirilirken hata oluştu.");
                return Error("Aylık gelir getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// En popüler destinasyonlar
        /// </summary>
        /// <param name="limit">Kayıt limiti (varsayılan: 10)</param>
        /// <returns>Popüler destinasyonlar listesi</returns>
        /// <response code="200">Popüler destinasyonlar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("popular-destinations")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPopularDestinations([FromQuery] int? limit = 10)
        {
            try
            {
                var result = await _reportsService.GetPopularDestinationsAsync(limit);
                return Success(result, "Popüler destinasyonlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Popüler destinasyonlar getirilirken hata oluştu.");
                return Error("Popüler destinasyonlar getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Dashboard özeti
        /// </summary>
        /// <returns>Dashboard özet verileri</returns>
        /// <response code="200">Dashboard özeti başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("dashboard-summary")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var result = await _reportsService.GetDashboardSummaryAsync();
                return Success(result, "Dashboard özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard özeti getirilirken hata oluştu.");
                return Error("Dashboard özeti getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Günlük gelir raporu
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Günlük gelir raporu verileri</returns>
        /// <response code="200">Günlük gelir raporu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("daily-revenue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDailyRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _reportsService.GetDailyRevenueAsync(startDate, endDate);
                return Success(result, "Günlük gelir raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Günlük gelir raporu getirilirken hata oluştu.");
                return Error("Günlük gelir raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Haftalık gelir raporu
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Haftalık gelir raporu verileri</returns>
        /// <response code="200">Haftalık gelir raporu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("weekly-revenue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWeeklyRevenue(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _reportsService.GetWeeklyRevenueAsync(startDate, endDate);
                return Success(result, "Haftalık gelir raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Haftalık gelir raporu getirilirken hata oluştu.");
                return Error("Haftalık gelir raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Yıllık gelir raporu
        /// </summary>
        /// <param name="startYear">Başlangıç yılı (opsiyonel)</param>
        /// <param name="endYear">Bitiş yılı (opsiyonel)</param>
        /// <returns>Yıllık gelir raporu verileri</returns>
        /// <response code="200">Yıllık gelir raporu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("yearly-revenue")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYearlyRevenue(
            [FromQuery] int? startYear = null,
            [FromQuery] int? endYear = null)
        {
            try
            {
                var result = await _reportsService.GetYearlyRevenueAsync(startYear, endYear);
                return Success(result, "Yıllık gelir raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yıllık gelir raporu getirilirken hata oluştu.");
                return Error("Yıllık gelir raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Tur popülerlik analizi (en çok tercih edilen turlar)
        /// </summary>
        /// <param name="tourType">Tur tipi filtresi (opsiyonel)</param>
        /// <param name="limit">Kayıt limiti (varsayılan: 10)</param>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Popüler turlar listesi</returns>
        /// <response code="200">Popüler turlar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("popular-tours")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPopularTours(
            [FromQuery] string? tourType = null,
            [FromQuery] int? limit = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _reportsService.GetPopularToursAsync(tourType, limit, startDate, endDate);
                return Success(result, "Popüler turlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Popüler turlar getirilirken hata oluştu.");
                return Error("Popüler turlar getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Personel performans raporu
        /// </summary>
        [HttpGet("personnel-performance")]
        public async Task<IActionResult> GetPersonnelPerformance(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? personnelId = null)
        {
            try
            {
                var result = await _reportsService.GetPersonnelPerformanceAsync(startDate, endDate, serviceType, personnelId);
                return Success(result, "Personel performans raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Personel performans raporu getirilirken hata oluştu.");
                return Error("Personel performans raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// VAT tahakkuk raporu (391 hesabına göre) - Dönem bazlı KDV raporu
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <param name="currency">Para birimi filtresi (opsiyonel)</param>
        /// <returns>VAT tahakkuk raporu</returns>
        /// <response code="200">VAT tahakkuk raporu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("vat-accrual")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetVatAccrualReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? currency = null)
        {
            try
            {
                var result = await _reportsService.GetVatAccrualReportAsync(startDate, endDate, currency);
                return Success(result, "VAT tahakkuk raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "VAT tahakkuk raporu getirilirken hata oluştu.");
                return Error("VAT tahakkuk raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// Dönem bazlı KDV detay raporu (aylık/haftalık/günlük breakdown)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <param name="periodType">Dönem tipi: daily, weekly, monthly (varsayılan: monthly)</param>
        /// <param name="currency">Para birimi filtresi (opsiyonel)</param>
        /// <returns>Dönem bazlı KDV raporu</returns>
        /// <response code="200">Dönem bazlı KDV raporu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("vat-period")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetVatPeriodReport(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? periodType = null,
            [FromQuery] string? currency = null)
        {
            try
            {
                var result = await _reportsService.GetVatPeriodReportAsync(startDate, endDate, periodType, currency);
                return Success(result, "Dönem bazlı KDV raporu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dönem bazlı KDV raporu getirilirken hata oluştu.");
                return Error("Dönem bazlı KDV raporu getirilirken bir hata oluştu.", (int)HttpStatusCode.InternalServerError);
            }
        }
    }
}

