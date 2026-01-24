// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Analytics.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Analytics
{
    /// <summary>
    /// Gelişmiş Analytics servisi implementasyonu
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IRepository<PaymentEntity> _paymentRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<InvoicesEntity> _invoiceRepository;
        private readonly IRepository<SupplierCost> _supplierCostRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<GuestFlow.Domain.Entities.Core.Supplier> _supplierRepository;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(
            IRepository<PaymentEntity> paymentRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<GuestEntity> guestRepository,
            IRepository<InvoicesEntity> invoiceRepository,
            IRepository<SupplierCost> supplierCostRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<GuestFlow.Domain.Entities.Core.Supplier> supplierRepository,
            ILogger<AnalyticsService> logger)
        {
            _paymentRepository = paymentRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _guestRepository = guestRepository;
            _invoiceRepository = invoiceRepository;
            _supplierCostRepository = supplierCostRepository;
            _cityRepository = cityRepository;
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        public async Task<RealTimeKpiDto> GetRealTimeKpisAsync(DateTime? date = null)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow.Date;
                var monthStart = new DateTime(targetDate.Year, targetDate.Month, 1);
                var lastMonthStart = monthStart.AddMonths(-1);
                var lastMonthEnd = monthStart.AddDays(-1);

                // Bugünkü gelir
                var todayRevenue = await GetRevenueForDateRangeAsync(targetDate, targetDate);
                var todayCost = await GetCostForDateRangeAsync(targetDate, targetDate);
                var todayServiceCount = await GetServiceCountForDateRangeAsync(targetDate, targetDate);

                // Bu ayın geliri
                var thisMonthRevenue = await GetRevenueForDateRangeAsync(monthStart, targetDate);
                var thisMonthCost = await GetCostForDateRangeAsync(monthStart, targetDate);
                var thisMonthServiceCount = await GetServiceCountForDateRangeAsync(monthStart, targetDate);
                var thisMonthNetProfit = thisMonthRevenue - thisMonthCost;

                // Geçen ayın geliri (karşılaştırma için)
                var lastMonthRevenue = await GetRevenueForDateRangeAsync(lastMonthStart, lastMonthEnd);
                var revenueGrowthRate = lastMonthRevenue > 0
                    ? ((thisMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100
                    : 0;

                // Ortalama hizmet başına gelir
                var averageRevenuePerService = thisMonthServiceCount > 0
                    ? thisMonthRevenue / thisMonthServiceCount
                    : 0;

                // Kâr marjı
                var profitMargin = thisMonthRevenue > 0
                    ? (thisMonthNetProfit / thisMonthRevenue) * 100
                    : 0;

                // En karlı hizmetler
                var mostProfitableServices = await GetMostProfitableServicesAsync(monthStart, targetDate);

                return new RealTimeKpiDto
                {
                    TodayRevenue = todayRevenue,
                    ThisMonthRevenue = thisMonthRevenue,
                    ThisMonthNetProfit = thisMonthNetProfit,
                    AverageRevenuePerService = averageRevenuePerService,
                    TodayServiceCount = todayServiceCount,
                    ThisMonthServiceCount = thisMonthServiceCount,
                    MostProfitableServices = mostProfitableServices,
                    RevenueGrowthRate = revenueGrowthRate,
                    ProfitMargin = profitMargin
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Real-time KPI'lar hesaplanırken hata oluştu");
                throw;
            }
        }

        public async Task<RevenueTrendDto> GetRevenueTrendAsync(
            string period = "daily",
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool includeComparison = false)
        {
            try
            {
                var end = endDate ?? DateTime.UtcNow.Date;
                var start = startDate ?? end.AddDays(-30);

                var dataPoints = new List<RevenueTrendPointDto>();
                var comparisonDataPoints = includeComparison ? new List<RevenueTrendPointDto>() : null;

                if (period.ToLower() == "daily")
                {
                    var currentDate = start;
                    while (currentDate <= end)
                    {
                        var revenue = await GetRevenueForDateRangeAsync(currentDate, currentDate);
                        var cost = await GetCostForDateRangeAsync(currentDate, currentDate);
                        var serviceCount = await GetServiceCountForDateRangeAsync(currentDate, currentDate);

                        dataPoints.Add(new RevenueTrendPointDto
                        {
                            Label = currentDate.ToString("dd.MM"),
                            Date = currentDate,
                            Revenue = revenue,
                            Cost = cost,
                            NetProfit = revenue - cost,
                            ServiceCount = serviceCount
                        });

                        if (includeComparison)
                        {
                            var comparisonDate = currentDate.AddYears(-1);
                            var compRevenue = await GetRevenueForDateRangeAsync(comparisonDate, comparisonDate);
                            var compCost = await GetCostForDateRangeAsync(comparisonDate, comparisonDate);
                            var compServiceCount = await GetServiceCountForDateRangeAsync(comparisonDate, comparisonDate);

                            comparisonDataPoints!.Add(new RevenueTrendPointDto
                            {
                                Label = comparisonDate.ToString("dd.MM"),
                                Date = comparisonDate,
                                Revenue = compRevenue,
                                Cost = compCost,
                                NetProfit = compRevenue - compCost,
                                ServiceCount = compServiceCount
                            });
                        }

                        currentDate = currentDate.AddDays(1);
                    }
                }
                else if (period.ToLower() == "weekly")
                {
                    var currentDate = start;
                    while (currentDate <= end)
                    {
                        var weekEnd = currentDate.AddDays(6);
                        if (weekEnd > end) weekEnd = end;

                        var revenue = await GetRevenueForDateRangeAsync(currentDate, weekEnd);
                        var cost = await GetCostForDateRangeAsync(currentDate, weekEnd);
                        var serviceCount = await GetServiceCountForDateRangeAsync(currentDate, weekEnd);

                        dataPoints.Add(new RevenueTrendPointDto
                        {
                            Label = $"{currentDate:dd.MM} - {weekEnd:dd.MM}",
                            Date = currentDate,
                            Revenue = revenue,
                            Cost = cost,
                            NetProfit = revenue - cost,
                            ServiceCount = serviceCount
                        });

                        currentDate = weekEnd.AddDays(1);
                    }
                }
                else if (period.ToLower() == "monthly")
                {
                    var currentDate = new DateTime(start.Year, start.Month, 1);
                    while (currentDate <= end)
                    {
                        var monthEnd = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month));
                        if (monthEnd > end) monthEnd = end;

                        var revenue = await GetRevenueForDateRangeAsync(currentDate, monthEnd);
                        var cost = await GetCostForDateRangeAsync(currentDate, monthEnd);
                        var serviceCount = await GetServiceCountForDateRangeAsync(currentDate, monthEnd);

                        dataPoints.Add(new RevenueTrendPointDto
                        {
                            Label = currentDate.ToString("MMM yyyy"),
                            Date = currentDate,
                            Revenue = revenue,
                            Cost = cost,
                            NetProfit = revenue - cost,
                            ServiceCount = serviceCount
                        });

                        currentDate = monthEnd.AddMonths(1).AddDays(1 - monthEnd.Day);
                    }
                }

                var totalRevenue = dataPoints.Sum(d => d.Revenue);
                var averageRevenue = dataPoints.Count > 0 ? totalRevenue / dataPoints.Count : 0;
                var growthRate = dataPoints.Count > 1
                    ? ((dataPoints.Last().Revenue - dataPoints.First().Revenue) / (dataPoints.First().Revenue > 0 ? dataPoints.First().Revenue : 1)) * 100
                    : 0;

                return new RevenueTrendDto
                {
                    Period = period,
                    DataPoints = dataPoints,
                    ComparisonDataPoints = comparisonDataPoints,
                    TotalRevenue = totalRevenue,
                    AverageRevenue = averageRevenue,
                    GrowthRate = growthRate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gelir trend analizi hesaplanırken hata oluştu");
                throw;
            }
        }

        // Helper methods
        private async Task<decimal> GetRevenueForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var completedPayments = await _paymentRepository.GetAll()
                .Where(p => p.PaymentDate.Date >= startDate.Date &&
                           p.PaymentDate.Date <= endDate.Date &&
                           p.Status == PaymentStatus.Completed &&
                           !p.IsDeleted)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var refundedPayments = await _paymentRepository.GetAll()
                .Where(p => p.RefundDate.HasValue &&
                           p.RefundDate.Value.Date >= startDate.Date &&
                           p.RefundDate.Value.Date <= endDate.Date &&
                           p.Status == PaymentStatus.Refunded &&
                           !p.IsDeleted)
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            return completedPayments - refundedPayments;
        }

        private async Task<decimal> GetCostForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            // SupplierCostEntity'den maliyet hesapla
            var costs = await _supplierCostRepository.GetAll()
                .Where(c => c.CreatedDate.Date >= startDate.Date &&
                           c.CreatedDate.Date <= endDate.Date &&
                           !c.IsDeleted)
                .SumAsync(c => (decimal?)c.CostAmount) ?? 0;

            return costs;
        }

        private async Task<int> GetServiceCountForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transfers = await _transferRepository.GetAll()
                .Where(t => t.TransferDate.Date >= startDate.Date &&
                           t.TransferDate.Date <= endDate.Date &&
                           !t.IsDeleted)
                .CountAsync();

            var cityTours = await _cityTourRepository.GetAll()
                .Where(ct => ct.TourDate.Date >= startDate.Date &&
                            ct.TourDate.Date <= endDate.Date &&
                            !ct.IsDeleted)
                .CountAsync();

            var yachtTours = await _yachtTourRepository.GetAll()
                .Where(yt => yt.TourDate.Date >= startDate.Date &&
                            yt.TourDate.Date <= endDate.Date &&
                            !yt.IsDeleted)
                .CountAsync();

            return transfers + cityTours + yachtTours;
        }

        private async Task<List<MostProfitableServiceDto>> GetMostProfitableServicesAsync(DateTime startDate, DateTime endDate)
        {
            var services = new List<MostProfitableServiceDto>();

            // Transferler
            var transferPayments = await _paymentRepository.GetAll()
                .Where(p => p.TransferId.HasValue &&
                           p.PaymentDate.Date >= startDate.Date &&
                           p.PaymentDate.Date <= endDate.Date &&
                           p.Status == PaymentStatus.Completed &&
                           !p.IsDeleted)
                .ToListAsync();

            var transferRevenue = transferPayments.Sum(p => p.Amount);
            var transferCosts = await _supplierCostRepository.GetAll()
                .Where(c => c.TransferId.HasValue &&
                           c.CreatedDate.Date >= startDate.Date &&
                           c.CreatedDate.Date <= endDate.Date &&
                           !c.IsDeleted)
                .SumAsync(c => (decimal?)c.CostAmount) ?? 0;
            var transferCount = transferPayments.Select(p => p.TransferId).Distinct().Count();

            if (transferCount > 0)
            {
                services.Add(new MostProfitableServiceDto
                {
                    ServiceType = "Transfer",
                    TotalRevenue = transferRevenue,
                    TotalCost = transferCosts,
                    NetProfit = transferRevenue - transferCosts,
                    ProfitMargin = transferRevenue > 0 ? ((transferRevenue - transferCosts) / transferRevenue) * 100 : 0,
                    ServiceCount = transferCount
                });
            }

            // Şehir Turları
            var cityTourPayments = await _paymentRepository.GetAll()
                .Where(p => p.CityTourId.HasValue &&
                           p.PaymentDate.Date >= startDate.Date &&
                           p.PaymentDate.Date <= endDate.Date &&
                           p.Status == PaymentStatus.Completed &&
                           !p.IsDeleted)
                .ToListAsync();

            var cityTourRevenue = cityTourPayments.Sum(p => p.Amount);
            var cityTourCosts = await _supplierCostRepository.GetAll()
                .Where(c => c.CityTourId.HasValue &&
                           c.CreatedDate.Date >= startDate.Date &&
                           c.CreatedDate.Date <= endDate.Date &&
                           !c.IsDeleted)
                .SumAsync(c => (decimal?)c.CostAmount) ?? 0;
            var cityTourCount = cityTourPayments.Select(p => p.CityTourId).Distinct().Count();

            if (cityTourCount > 0)
            {
                services.Add(new MostProfitableServiceDto
                {
                    ServiceType = "CityTour",
                    TotalRevenue = cityTourRevenue,
                    TotalCost = cityTourCosts,
                    NetProfit = cityTourRevenue - cityTourCosts,
                    ProfitMargin = cityTourRevenue > 0 ? ((cityTourRevenue - cityTourCosts) / cityTourRevenue) * 100 : 0,
                    ServiceCount = cityTourCount
                });
            }

            // Yat Turları
            var yachtTourPayments = await _paymentRepository.GetAll()
                .Where(p => p.YachtTourId.HasValue &&
                           p.PaymentDate.Date >= startDate.Date &&
                           p.PaymentDate.Date <= endDate.Date &&
                           p.Status == PaymentStatus.Completed &&
                           !p.IsDeleted)
                .ToListAsync();

            var yachtTourRevenue = yachtTourPayments.Sum(p => p.Amount);
            var yachtTourCosts = await _supplierCostRepository.GetAll()
                .Where(c => c.YachtTourId.HasValue &&
                           c.CreatedDate.Date >= startDate.Date &&
                           c.CreatedDate.Date <= endDate.Date &&
                           !c.IsDeleted)
                .SumAsync(c => (decimal?)c.CostAmount) ?? 0;
            var yachtTourCount = yachtTourPayments.Select(p => p.YachtTourId).Distinct().Count();

            if (yachtTourCount > 0)
            {
                services.Add(new MostProfitableServiceDto
                {
                    ServiceType = "YachtTour",
                    TotalRevenue = yachtTourRevenue,
                    TotalCost = yachtTourCosts,
                    NetProfit = yachtTourRevenue - yachtTourCosts,
                    ProfitMargin = yachtTourRevenue > 0 ? ((yachtTourRevenue - yachtTourCosts) / yachtTourRevenue) * 100 : 0,
                    ServiceCount = yachtTourCount
                });
            }

            return services.OrderByDescending(s => s.NetProfit).ToList();
        }

        // Diğer metodlar placeholder olarak bırakıldı, implement edilecek
        public Task<SeasonalComparisonDto> GetSeasonalComparisonAsync(int? year = null)
        {
            throw new NotImplementedException("GetSeasonalComparisonAsync will be implemented");
        }

        public Task<YearlyGrowthDto> GetYearlyGrowthAsync(int? year = null)
        {
            throw new NotImplementedException("GetYearlyGrowthAsync will be implemented");
        }

        public Task<GuestSegmentationDto> GetGuestSegmentationAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetGuestSegmentationAsync will be implemented");
        }

        public Task<ServiceDistributionDto> GetServiceDistributionAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetServiceDistributionAsync will be implemented");
        }

        public Task<CityPerformanceDto> GetCityPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetCityPerformanceAsync will be implemented");
        }

        public Task<SupplierProfitabilityDto> GetSupplierProfitabilityAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetSupplierProfitabilityAsync will be implemented");
        }

        public Task<RevenueForecastDto> GetRevenueForecastAsync(int monthsAhead = 1)
        {
            throw new NotImplementedException("GetRevenueForecastAsync will be implemented");
        }

        public Task<DemandForecastDto> GetDemandForecastAsync(string serviceType, DateTime? startDate = null, DateTime? endDate = null)
        {
            throw new NotImplementedException("GetDemandForecastAsync will be implemented");
        }

        public Task<OptimalPriceDto> GetOptimalPriceSuggestionsAsync(string serviceType, DateTime? targetDate = null)
        {
            throw new NotImplementedException("GetOptimalPriceSuggestionsAsync will be implemented");
        }
    }
}
