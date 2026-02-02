using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/analytics")]
    [ApiVersion("1.0")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IPredictiveAnalyticsService _analyticsService;

        public AnalyticsController(IPredictiveAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("predict/occupancy")]
        public async Task<IActionResult> PredictOccupancy([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (endDate < startDate)
                return BadRequest("End date must be after start date");

            if ((endDate - startDate).TotalDays > 365)
                return BadRequest("Prediction horizon cannot exceed 1 year");

            var result = await _analyticsService.PredictOccupancyAsync(startDate, endDate);
            return Ok(result);
        }

        [HttpGet("predict/revenue")]
        public async Task<IActionResult> PredictRevenue([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (endDate < startDate)
                return BadRequest("End date must be after start date");

            var result = await _analyticsService.PredictRevenueAsync(startDate, endDate);
            return Ok(result);
        }
    }
}
