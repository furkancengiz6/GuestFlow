using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Dashboard ve istatistikler için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Admin ve Staff erişebilir
    [Tags("Dashboard")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly DailyOperationsService _dailyOperationsService;

        public DashboardController(
            IDashboardService dashboardService,
            DailyOperationsService dailyOperationsService)
        {
            _dashboardService = dashboardService;
            _dailyOperationsService = dailyOperationsService;
        }

        /// <summary>
        /// Dashboard genel bakış bilgilerini getirir
        /// </summary>
        /// <returns>Dashboard özet bilgileri</returns>
        /// <response code="200">Dashboard özeti başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboardOverview()
        {
            try
            {
                var result = await _dashboardService.GetDashboardOverviewAsync();
                return Success(result, "Dashboard özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Dashboard özeti getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Hızlı istatistikleri getirir
        /// </summary>
        /// <returns>Hızlı istatistik verileri</returns>
        /// <response code="200">Hızlı istatistikler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("quick-stats")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetQuickStats()
        {
            try
            {
                var result = await _dashboardService.GetQuickStatsAsync();
                return Success(result, "Hızlı istatistikler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Hızlı istatistikler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Son aktiviteleri getirir
        /// </summary>
        /// <param name="limit">Kayıt limiti (varsayılan: 10)</param>
        /// <returns>Son aktiviteler listesi</returns>
        /// <response code="200">Son aktiviteler başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("recent-activities")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int? limit = 10)
        {
            try
            {
                var result = await _dashboardService.GetRecentActivitiesAsync(limit);
                return Success(result, "Son aktiviteler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Son aktiviteler getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Gelir grafik verilerini getirir
        /// </summary>
        /// <param name="period">Periyot (daily, weekly, monthly, varsayılan: daily)</param>
        /// <param name="days">Gün sayısı (opsiyonel)</param>
        /// <returns>Gelir grafik verileri</returns>
        /// <response code="200">Gelir grafik verileri başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("revenue-chart")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] string period = "daily",
            [FromQuery] int? days = null)
        {
            try
            {
                var result = await _dashboardService.GetRevenueChartDataAsync(period, days);
                return Success(result, "Gelir grafik verileri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Gelir grafik verileri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yaklaşan rezervasyonları getirir (takvim için)
        /// </summary>
        /// <param name="startDate">Başlangıç tarihi (opsiyonel)</param>
        /// <param name="endDate">Bitiş tarihi (opsiyonel)</param>
        /// <returns>Yaklaşan rezervasyonlar listesi</returns>
        /// <response code="200">Yaklaşan rezervasyonlar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("upcoming-bookings")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUpcomingBookings(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _dashboardService.GetUpcomingBookingsAsync(startDate, endDate);
                return Success(result, "Yaklaşan rezervasyonlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Yaklaşan rezervasyonlar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir istatistik kartı verilerini getirir
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
                var result = await _dashboardService.GetGuestStatisticsCardAsync();
                return Success(result, "Misafir istatistikleri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Misafir istatistikleri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Ödemesi alınmamış hizmetler (transfer, şehir turu, yat turu)
        /// </summary>
        [HttpGet("unpaid-services")]
        public async Task<IActionResult> GetUnpaidServices([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _dashboardService.GetUnpaidServicesAsync(startDate, endDate);
            return Success(result, "Ödemesi alınmamış hizmetler başarıyla getirildi.");
        }

        /// <summary>
        /// Yaklaşan hizmetler (transfer, şehir turu, yat turu)
        /// </summary>
        [HttpGet("upcoming-services")]
        public async Task<IActionResult> GetUpcomingServices([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _dashboardService.GetUpcomingServicesAsync(startDate, endDate);
            return Success(result, "Yaklaşan hizmetler başarıyla getirildi.");
        }

        /// <summary>
        /// Bugünkü kritik olaylar (acil müdahale gerekenler)
        /// </summary>
        [HttpGet("critical-events")]
        public async Task<IActionResult> GetCriticalEvents()
        {
            var result = await _dashboardService.GetCriticalEventsAsync();
            return Success(result, "Bugünkü kritik olaylar başarıyla getirildi.");
        }

        /// <summary>
        /// Günlük operasyon özeti (bugün/yaklaşan servisler, risk bayrakları, hızlı istatistikler)
        /// </summary>
        /// <param name="date">Tarih (opsiyonel, varsayılan: bugün)</param>
        /// <returns>Günlük operasyon özeti</returns>
        /// <response code="200">Günlük operasyon özeti başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("daily-operations")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDailyOperations([FromQuery] DateTime? date = null)
        {
            try
            {
                var result = await _dailyOperationsService.GetDailyOperationsAsync(date);
                return Success(result, "Günlük operasyon özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error("Günlük operasyon özeti getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}

