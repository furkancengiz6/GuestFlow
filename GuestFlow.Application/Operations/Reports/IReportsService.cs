using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Reports
{
    public interface IReportsService
    {
        /// <summary>
        /// Tarih aralığına göre gelir özeti
        /// </summary>
        Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? startDate = null, DateTime? endDate = null, string? serviceType = null, int? personnelId = null);

        /// <summary>
        /// Misafir istatistikleri
        /// </summary>
        Task<GuestStatisticsDto> GetGuestStatisticsAsync();

        /// <summary>
        /// Tur istatistikleri (CityTour veya YachtTour)
        /// </summary>
        Task<TourStatisticsDto> GetTourStatisticsAsync(string? tourType = null, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Transfer istatistikleri
        /// </summary>
        Task<TransferStatisticsDto> GetTransferStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, int? personnelId = null);

        /// <summary>
        /// Aylık gelir dağılımı
        /// </summary>
        Task<List<MonthlyRevenueDto>> GetMonthlyRevenueAsync(int? year = null);

        /// <summary>
        /// En popüler destinasyonlar
        /// </summary>
        Task<List<PopularDestinationDto>> GetPopularDestinationsAsync(int? limit = 10);

        /// <summary>
        /// Dashboard özeti
        /// </summary>
        Task<DashboardSummaryDto> GetDashboardSummaryAsync();
        
        /// <summary>
        /// Günlük gelir raporu
        /// </summary>
        Task<List<DailyRevenueDto>> GetDailyRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Haftalık gelir raporu
        /// </summary>
        Task<List<WeeklyRevenueDto>> GetWeeklyRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Yıllık gelir raporu
        /// </summary>
        Task<List<YearlyRevenueDto>> GetYearlyRevenueAsync(int? startYear = null, int? endYear = null);
        
        /// <summary>
        /// Tur popülerlik analizi (en çok tercih edilen turlar)
        /// </summary>
        Task<List<PopularTourDto>> GetPopularToursAsync(string? tourType = null, int? limit = 10, DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Personel performans raporu
        /// </summary>
        Task<List<PersonnelPerformanceDto>> GetPersonnelPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null, string? serviceType = null, int? personnelId = null);

        /// <summary>
        /// VAT tahakkuk raporu (391 hesabına göre) - Dönem bazlı KDV raporu
        /// </summary>
        Task<VatAccrualReportDto> GetVatAccrualReportAsync(DateTime? startDate = null, DateTime? endDate = null, string? currency = null);

        /// <summary>
        /// Dönem bazlı KDV detay raporu (aylık/haftalık/günlük breakdown)
        /// </summary>
        Task<List<VatPeriodReportDto>> GetVatPeriodReportAsync(DateTime? startDate = null, DateTime? endDate = null, string? periodType = null, string? currency = null);

        /// <summary>
        /// Haftalık operasyonel rapor oluşturur (PDF)
        /// </summary>
        Task<string> GenerateWeeklyOperationalReportAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Rapor verilerini AI ile analiz eder ve içgörü döndürür
        /// </summary>
        Task<string> GetReportInsightsAsync(string reportType, object reportData);
    }
}

