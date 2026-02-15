using System.Threading.Tasks;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Dashboard.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/commercial-dashboard")]
    [ApiVersion("1.0")]
    [Authorize] // Requires authentication, usually limited to GM/Admin in real scenarios
    public class CommercialDashboardController : ControllerBase
    {
        private readonly ICommercialStrategyService _strategyService;

        public CommercialDashboardController(ICommercialStrategyService strategyService)
        {
            _strategyService = strategyService;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<CommercialDashboardSummaryDto>> GetExecutiveSummary()
        {
            var summary = await _strategyService.GetExecutiveSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("upsell-opportunities")]
        public async Task<ActionResult<IEnumerable<UpsellOpportunityDto>>> GetUpsellOpportunities()
        {
            var opportunities = await _strategyService.GetUpsellOpportunitiesAsync();
            return Ok(opportunities);
        }

        [HttpGet("friction-report")]
        public async Task<ActionResult<IEnumerable<ServiceFrictionReportDto>>> GetFrictionReport()
        {
            var report = await _strategyService.GetDepartmentFrictionReportAsync();
            return Ok(report);
        }

        [HttpGet("loyalty-insights")]
        public async Task<ActionResult<IEnumerable<LoyaltyIntelligenceDto>>> GetLoyaltyInsights()
        {
            var insights = await _strategyService.GetTopLoyaltyInsightsAsync();
            return Ok(insights);
        }

        [HttpGet("ai-bundled-opportunities")]
        public async Task<ActionResult<IEnumerable<UpsellOpportunityDto>>> GetBundledOpportunities()
        {
            var opportunities = await _strategyService.GetAIBundledOpportunitiesAsync();
            return Ok(opportunities);
        }

        [HttpGet("sustainable-bundles")]
        public async Task<ActionResult<IEnumerable<UpsellOpportunityDto>>> GetSustainableBundles()
        {
            var opportunities = await _strategyService.GetSustainableBundleRecommendationsAsync();
            return Ok(opportunities);
        }
    }
}
