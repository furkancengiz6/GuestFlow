// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Analytics.Dtos;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Analytics
{
    /// <summary>
    /// Gelişmiş Analytics servisi - KPI'lar, trend analizi, segmentasyon, tahminler
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Gerçek zamanlı KPI'ları getirir
        /// </summary>
        Task<RealTimeKpiDto> GetRealTimeKpisAsync(DateTime? date = null);

        /// <summary>
        /// Gelir trend analizini getirir
        /// </summary>
        Task<RevenueTrendDto> GetRevenueTrendAsync(
            string period = "daily", 
            DateTime? startDate = null, 
            DateTime? endDate = null,
            bool includeComparison = false);

        /// <summary>
        /// Sezon bazlı karşılaştırma getirir
        /// </summary>
        Task<SeasonalComparisonDto> GetSeasonalComparisonAsync(int? year = null);

        /// <summary>
        /// Yıl bazlı büyüme oranlarını getirir
        /// </summary>
        Task<YearlyGrowthDto> GetYearlyGrowthAsync(int? year = null);

        /// <summary>
        /// Misafir segmentasyonunu getirir
        /// </summary>
        Task<GuestSegmentationDto> GetGuestSegmentationAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Hizmet tipi dağılımını getirir
        /// </summary>
        Task<ServiceDistributionDto> GetServiceDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Şehir bazlı performansı getirir
        /// </summary>
        Task<CityPerformanceDto> GetCityPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Tedarikçi bazlı kârlılığı getirir
        /// </summary>
        Task<SupplierProfitabilityDto> GetSupplierProfitabilityAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Gelir tahmini yapar
        /// </summary>
        Task<RevenueForecastDto> GetRevenueForecastAsync(int monthsAhead = 1);

        /// <summary>
        /// Talep tahmini yapar
        /// </summary>
        Task<DemandForecastDto> GetDemandForecastAsync(string serviceType, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Optimal fiyat önerileri getirir
        /// </summary>
        Task<OptimalPriceDto> GetOptimalPriceSuggestionsAsync(string serviceType, DateTime? targetDate = null);
    }
}
