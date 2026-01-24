// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Analytics;
using GuestFlow.Application.Operations.Analytics.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,Owner")]
    [Tags("Analytics")]
    public class AnalyticsController : BaseController
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Gerçek zamanlı KPI'ları getirir
        /// </summary>
        [HttpGet("kpis/realtime")]
        [ProducesResponseType(typeof(ApiResponse<RealTimeKpiDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRealTimeKpis([FromQuery] DateTime? date = null)
        {
            try
            {
                var result = await _analyticsService.GetRealTimeKpisAsync(date);
                return Ok(ApiResponse<RealTimeKpiDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("KPI'lar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Gelir trend analizini getirir
        /// </summary>
        [HttpGet("revenue/trend")]
        [ProducesResponseType(typeof(ApiResponse<RevenueTrendDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRevenueTrend(
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeComparison = false)
        {
            try
            {
                var result = await _analyticsService.GetRevenueTrendAsync(period, startDate, endDate, includeComparison);
                return Ok(ApiResponse<RevenueTrendDto>.SuccessResponse(result));
            }
            catch (Exception ex)
            {
                return Error("Gelir trend analizi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Sezon bazlı karşılaştırma getirir
        /// </summary>
        [HttpGet("seasonal/comparison")]
        [ProducesResponseType(typeof(ApiResponse<SeasonalComparisonDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSeasonalComparison([FromQuery] int? year = null)
        {
            try
            {
                var result = await _analyticsService.GetSeasonalComparisonAsync(year);
                return Ok(ApiResponse<SeasonalComparisonDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Sezon karşılaştırması getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yıl bazlı büyüme oranlarını getirir
        /// </summary>
        [HttpGet("growth/yearly")]
        [ProducesResponseType(typeof(ApiResponse<YearlyGrowthDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetYearlyGrowth([FromQuery] int? year = null)
        {
            try
            {
                var result = await _analyticsService.GetYearlyGrowthAsync(year);
                return Ok(ApiResponse<YearlyGrowthDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Yıllık büyüme analizi getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Misafir segmentasyonunu getirir
        /// </summary>
        [HttpGet("segmentation/guests")]
        [ProducesResponseType(typeof(ApiResponse<GuestSegmentationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetGuestSegmentation(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _analyticsService.GetGuestSegmentationAsync(startDate, endDate);
                return Ok(ApiResponse<GuestSegmentationDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Misafir segmentasyonu getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Hizmet tipi dağılımını getirir
        /// </summary>
        [HttpGet("distribution/services")]
        [ProducesResponseType(typeof(ApiResponse<ServiceDistributionDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetServiceDistribution(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _analyticsService.GetServiceDistributionAsync(startDate, endDate);
                return Ok(ApiResponse<ServiceDistributionDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Hizmet dağılımı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Şehir bazlı performansı getirir
        /// </summary>
        [HttpGet("performance/cities")]
        [ProducesResponseType(typeof(ApiResponse<CityPerformanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCityPerformance(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _analyticsService.GetCityPerformanceAsync(startDate, endDate);
                return Ok(ApiResponse<CityPerformanceDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Şehir performansı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tedarikçi bazlı kârlılığı getirir
        /// </summary>
        [HttpGet("profitability/suppliers")]
        [ProducesResponseType(typeof(ApiResponse<SupplierProfitabilityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSupplierProfitability(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _analyticsService.GetSupplierProfitabilityAsync(startDate, endDate);
                return Ok(ApiResponse<SupplierProfitabilityDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Tedarikçi kârlılığı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Gelir tahmini yapar
        /// </summary>
        [HttpGet("forecast/revenue")]
        [ProducesResponseType(typeof(ApiResponse<RevenueForecastDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRevenueForecast([FromQuery] int monthsAhead = 1)
        {
            try
            {
                var result = await _analyticsService.GetRevenueForecastAsync(monthsAhead);
                return Ok(ApiResponse<RevenueForecastDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Gelir tahmini getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Talep tahmini yapar
        /// </summary>
        [HttpGet("forecast/demand")]
        [ProducesResponseType(typeof(ApiResponse<DemandForecastDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDemandForecast(
            [FromQuery] string serviceType,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var result = await _analyticsService.GetDemandForecastAsync(serviceType, startDate, endDate);
                return Ok(ApiResponse<DemandForecastDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Talep tahmini getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Optimal fiyat önerileri getirir
        /// </summary>
        [HttpGet("pricing/optimal")]
        [ProducesResponseType(typeof(ApiResponse<OptimalPriceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOptimalPriceSuggestions(
            [FromQuery] string serviceType,
            [FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _analyticsService.GetOptimalPriceSuggestionsAsync(serviceType, targetDate);
                return Ok(ApiResponse<OptimalPriceDto>.SuccessResponse(result));
            }
            catch (NotImplementedException)
            {
                return Error("Bu özellik henüz implement edilmedi.", 501);
            }
            catch (Exception ex)
            {
                return Error("Fiyat önerileri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}
