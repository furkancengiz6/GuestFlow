using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Analytics;
using GuestFlow.Application.Operations.Analytics.Dtos;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize(Roles = "Admin,Staff")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IPredictiveAnalyticsService _predictiveService;
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(
            IPredictiveAnalyticsService predictiveService,
            IAnalyticsService analyticsService)
        {
            _predictiveService = predictiveService;
            _analyticsService = analyticsService;
        }

        [HttpGet("kpis/realtime")]
        public async Task<IActionResult> GetRealTimeKpis([FromQuery] DateTime? date = null)
        {
            var result = await _analyticsService.GetRealTimeKpisAsync(date);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("revenue/trend")]
        public async Task<IActionResult> GetRevenueTrend(
            [FromQuery] string period = "daily",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeComparison = false)
        {
            var result = await _analyticsService.GetRevenueTrendAsync(period, startDate, endDate, includeComparison);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("seasonal/comparison")]
        public async Task<IActionResult> GetSeasonalComparison([FromQuery] int? year = null)
        {
            var result = await _analyticsService.GetSeasonalComparisonAsync(year);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("growth/yearly")]
        public async Task<IActionResult> GetYearlyGrowth([FromQuery] int? year = null)
        {
            var result = await _analyticsService.GetYearlyGrowthAsync(year);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("segmentation/guests")]
        public async Task<IActionResult> GetGuestSegmentation([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _analyticsService.GetGuestSegmentationAsync(startDate, endDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("distribution/services")]
        public async Task<IActionResult> GetServiceDistribution([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _analyticsService.GetServiceDistributionAsync(startDate, endDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("performance/cities")]
        public async Task<IActionResult> GetCityPerformance([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _analyticsService.GetCityPerformanceAsync(startDate, endDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("profitability/suppliers")]
        public async Task<IActionResult> GetSupplierProfitability([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _analyticsService.GetSupplierProfitabilityAsync(startDate, endDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("forecast/revenue")]
        public async Task<IActionResult> GetRevenueForecast([FromQuery] int monthsAhead = 1)
        {
            var result = await _analyticsService.GetRevenueForecastAsync(monthsAhead);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("forecast/demand")]
        public async Task<IActionResult> GetDemandForecast([FromQuery] string serviceType, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var result = await _analyticsService.GetDemandForecastAsync(serviceType, startDate, endDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("pricing/optimal")]
        public async Task<IActionResult> GetOptimalPriceSuggestions([FromQuery] string serviceType, [FromQuery] DateTime? targetDate = null)
        {
            var result = await _analyticsService.GetOptimalPriceSuggestionsAsync(serviceType, targetDate);
            return Ok(new { success = true, data = result });
        }

        [HttpGet("predict/occupancy")]
        public async Task<IActionResult> PredictOccupancy([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (endDate < startDate)
                return BadRequest("End date must be after start date");

            if ((endDate - startDate).TotalDays > 365)
                return BadRequest("Prediction horizon cannot exceed 1 year");

            var result = await _predictiveService.PredictOccupancyAsync(startDate, endDate);
            return Ok(result);
        }

        [HttpGet("predict/revenue")]
        public async Task<IActionResult> PredictRevenue([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (endDate < startDate)
                return BadRequest("End date must be after start date");

            var result = await _predictiveService.PredictRevenueAsync(startDate, endDate);
            return Ok(result);
        }
    }
}
