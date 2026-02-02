using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Finance.Revenue;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/revenue")]
    [ApiVersion("1.0")]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<RevenueDashboardDto>> GetDashboard([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
             var start = startDate ?? DateTime.Today.AddDays(-30);
             var end = endDate ?? DateTime.Today;

             var result = await _revenueService.GetRevenueDashboardAsync(start, end);
             return Ok(result);
        }
    }
}
