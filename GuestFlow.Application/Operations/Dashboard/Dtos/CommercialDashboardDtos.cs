using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Dashboard.Dtos
{
    public class UpsellOpportunityDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = null!;
        public string RecommendedService { get; set; } = null!;
        public double ProbabilityScore { get; set; }
        public string Context { get; set; } = null!;
        public bool IsSustainable { get; set; }
        public string? SustainabilityIncentive { get; set; }
    }

    public class ServiceFrictionReportDto
    {
        public string Department { get; set; } = null!;
        public double AverageFrictionScore { get; set; }
        public int NegativeIncidentCount { get; set; }
        public List<string> CommonIssues { get; set; } = new();
    }

    public class LoyaltyIntelligenceDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = null!;
        public double LifeTimeValue { get; set; }
        public string LoyaltyTier { get; set; } = null!;
        public int TotalInteractions { get; set; }
        public double InfluenceScore { get; set; }
    }

    public class CommercialDashboardSummaryDto
    {
        public List<UpsellOpportunityDto> TopUpsellOpportunities { get; set; } = new();
        public List<ServiceFrictionReportDto> FrictionByDepartment { get; set; } = new();
        public List<LoyaltyIntelligenceDto> HighValueGuests { get; set; } = new();
        public double GlobalQualityScore { get; set; }
    }
}
