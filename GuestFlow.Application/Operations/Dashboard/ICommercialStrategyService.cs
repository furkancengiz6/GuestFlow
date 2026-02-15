using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Dashboard.Dtos;

namespace GuestFlow.Application.Operations.Dashboard
{
    public interface ICommercialStrategyService
    {
        Task<List<UpsellOpportunityDto>> GetUpsellOpportunitiesAsync();
        Task<List<ServiceFrictionReportDto>> GetDepartmentFrictionReportAsync();
        Task<List<LoyaltyIntelligenceDto>> GetTopLoyaltyInsightsAsync();
        Task<List<UpsellOpportunityDto>> GetAIBundledOpportunitiesAsync();
        Task<List<UpsellOpportunityDto>> GetSustainableBundleRecommendationsAsync();
        Task<CommercialDashboardSummaryDto> GetExecutiveSummaryAsync();
    }
}
