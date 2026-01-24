// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Analytics.Dtos
{
    /// <summary>
    /// Gerçek zamanlı KPI'lar
    /// </summary>
    public class RealTimeKpiDto
    {
        public decimal TodayRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal ThisMonthNetProfit { get; set; }
        public decimal AverageRevenuePerService { get; set; }
        public int TodayServiceCount { get; set; }
        public int ThisMonthServiceCount { get; set; }
        public List<MostProfitableServiceDto> MostProfitableServices { get; set; } = new List<MostProfitableServiceDto>();
        public decimal RevenueGrowthRate { get; set; } // Bu ay vs geçen ay
        public decimal ProfitMargin { get; set; } // Kâr marjı yüzdesi
    }

    public class MostProfitableServiceDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ServiceCount { get; set; }
    }

    /// <summary>
    /// Gelir trend analizi
    /// </summary>
    public class RevenueTrendDto
    {
        public string Period { get; set; } = string.Empty; // daily, weekly, monthly
        public List<RevenueTrendPointDto> DataPoints { get; set; } = new List<RevenueTrendPointDto>();
        public List<RevenueTrendPointDto>? ComparisonDataPoints { get; set; } // Önceki dönem karşılaştırması
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenue { get; set; }
        public decimal GrowthRate { get; set; } // Önceki döneme göre büyüme
    }

    public class RevenueTrendPointDto
    {
        public string Label { get; set; } = string.Empty; // Tarih veya dönem etiketi
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal NetProfit { get; set; }
        public int ServiceCount { get; set; }
    }

    /// <summary>
    /// Sezon bazlı karşılaştırma
    /// </summary>
    public class SeasonalComparisonDto
    {
        public int Year { get; set; }
        public List<SeasonalDataDto> SeasonalData { get; set; } = new List<SeasonalDataDto>();
        public string PeakSeason { get; set; } = string.Empty;
        public string LowSeason { get; set; } = string.Empty;
    }

    public class SeasonalDataDto
    {
        public string Season { get; set; } = string.Empty; // Spring, Summer, Fall, Winter
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal NetProfit { get; set; }
        public int ServiceCount { get; set; }
        public decimal AverageRevenuePerService { get; set; }
    }

    /// <summary>
    /// Yıl bazlı büyüme
    /// </summary>
    public class YearlyGrowthDto
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenueGrowthRate { get; set; } // Önceki yıla göre
        public decimal ServiceCountGrowthRate { get; set; }
        public decimal ProfitGrowthRate { get; set; }
        public List<YearlyGrowthPointDto> YearlyData { get; set; } = new List<YearlyGrowthPointDto>();
    }

    public class YearlyGrowthPointDto
    {
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal NetProfit { get; set; }
        public int ServiceCount { get; set; }
        public decimal? GrowthRate { get; set; } // Önceki yıla göre
    }

    /// <summary>
    /// Misafir segmentasyonu
    /// </summary>
    public class GuestSegmentationDto
    {
        public List<GuestSegmentDto> Segments { get; set; } = new List<GuestSegmentDto>();
        public int TotalGuests { get; set; }
    }

    public class GuestSegmentDto
    {
        public string SegmentType { get; set; } = string.Empty; // VIP, Returning, New, Regular
        public int GuestCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerGuest { get; set; }
        public decimal Percentage { get; set; } // Toplam misafir içindeki yüzdesi
    }

    /// <summary>
    /// Hizmet tipi dağılımı
    /// </summary>
    public class ServiceDistributionDto
    {
        public List<ServiceDistributionItemDto> Services { get; set; } = new List<ServiceDistributionItemDto>();
        public int TotalServices { get; set; }
    }

    public class ServiceDistributionItemDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public int Count { get; set; }
        public decimal Revenue { get; set; }
        public decimal Percentage { get; set; } // Toplam içindeki yüzdesi
        public decimal AverageRevenue { get; set; }
    }

    /// <summary>
    /// Şehir bazlı performans
    /// </summary>
    public class CityPerformanceDto
    {
        public List<CityPerformanceItemDto> Cities { get; set; } = new List<CityPerformanceItemDto>();
    }

    public class CityPerformanceItemDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal AverageRevenuePerService { get; set; }
    }

    /// <summary>
    /// Tedarikçi bazlı kârlılık
    /// </summary>
    public class SupplierProfitabilityDto
    {
        public List<SupplierProfitabilityItemDto> Suppliers { get; set; } = new List<SupplierProfitabilityItemDto>();
    }

    public class SupplierProfitabilityItemDto
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal NetProfit { get; set; }
        public decimal ProfitMargin { get; set; }
        public decimal AverageCostPerService { get; set; }
    }

    /// <summary>
    /// Gelir tahmini
    /// </summary>
    public class RevenueForecastDto
    {
        public DateTime ForecastDate { get; set; }
        public int MonthsAhead { get; set; }
        public List<ForecastPointDto> ForecastPoints { get; set; } = new List<ForecastPointDto>();
        public decimal PredictedTotalRevenue { get; set; }
        public decimal ConfidenceLevel { get; set; } // 0-100 arası güven seviyesi
        public string Method { get; set; } = string.Empty; // Linear, MovingAverage, etc.
    }

    public class ForecastPointDto
    {
        public DateTime Date { get; set; }
        public decimal PredictedRevenue { get; set; }
        public decimal? MinRevenue { get; set; } // Güven aralığı alt
        public decimal? MaxRevenue { get; set; } // Güven aralığı üst
    }

    /// <summary>
    /// Talep tahmini
    /// </summary>
    public class DemandForecastDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public List<DemandForecastPointDto> ForecastPoints { get; set; } = new List<DemandForecastPointDto>();
        public int PredictedTotalDemand { get; set; }
        public decimal ConfidenceLevel { get; set; }
    }

    public class DemandForecastPointDto
    {
        public DateTime Date { get; set; }
        public int PredictedDemand { get; set; }
        public int? MinDemand { get; set; }
        public int? MaxDemand { get; set; }
    }

    /// <summary>
    /// Optimal fiyat önerileri
    /// </summary>
    public class OptimalPriceDto
    {
        public string ServiceType { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RecommendedPrice { get; set; }
        public decimal MinPrice { get; set; } // Minimum marj koruma
        public decimal MaxPrice { get; set; } // Maksimum rekabetçi fiyat
        public string Reason { get; set; } = string.Empty; // Öneri nedeni
        public decimal ExpectedDemand { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public List<PriceScenarioDto> Scenarios { get; set; } = new List<PriceScenarioDto>();
    }

    public class PriceScenarioDto
    {
        public decimal Price { get; set; }
        public decimal ExpectedDemand { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public decimal ProfitMargin { get; set; }
    }
}
