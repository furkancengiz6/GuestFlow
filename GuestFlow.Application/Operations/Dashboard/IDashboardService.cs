using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Dashboard ve istatistikler için özel servis
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Dashboard özet bilgilerini getirir
        /// </summary>
        Task<DashboardOverviewDto> GetDashboardOverviewAsync();

        /// <summary>
        /// Hızlı istatistikleri getirir (toplam sayılar)
        /// </summary>
        Task<QuickStatsDto> GetQuickStatsAsync();

        /// <summary>
        /// Son aktiviteleri getirir
        /// </summary>
        Task<RecentActivityDto> GetRecentActivitiesAsync(int? limit = 10);

        /// <summary>
        /// Gelir grafik verilerini getirir (günlük, haftalık, aylık)
        /// </summary>
        Task<RevenueChartDataDto> GetRevenueChartDataAsync(string period = "daily", int? days = null);

        /// <summary>
        /// Yaklaşan rezervasyonları getirir (takvim için)
        /// </summary>
        Task<UpcomingBookingsDto> GetUpcomingBookingsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Misafir istatistik kartı verilerini getirir
        /// </summary>
        Task<GuestStatisticsCardDto> GetGuestStatisticsCardAsync();
    }
}

