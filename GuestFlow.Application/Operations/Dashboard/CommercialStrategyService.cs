using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Infrastructure.Graph;
using GuestFlow.Application.Operations.Dashboard.Dtos;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using GuestFlow.Application.Operations.Intelligence.Graph;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace GuestFlow.Application.Operations.Dashboard
{
    public class CommercialStrategyService : ICommercialStrategyService
    {
        private readonly GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService _neo4jService;
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<CommercialStrategyService> _logger;

        public CommercialStrategyService(
            GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService neo4jService, 
            IAIAssistantService aiAssistantService,
            ILogger<CommercialStrategyService> logger)
        {
            _neo4jService = neo4jService;
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        public async Task<List<UpsellOpportunityDto>> GetUpsellOpportunitiesAsync()
        {
            try
            {
                return await _neo4jService.ExecuteReadAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest)
                        // Misafirin geçmişte kullandığı ama mevcut konaklamasında (son 7 gün) kullanmadığı servisler
                        MATCH (g)-[:USES]->(oldS:Service)
                        WHERE NOT (g)-[:USES]->(oldS {timestamp: datetime() - duration('P7D')})
                        WITH g, oldS, count(oldS) as pastFrequency
                        ORDER BY pastFrequency DESC
                        RETURN g.id as guestId, g.name as guestName, oldS.type as serviceType, 
                               oldS.name as serviceName, (pastFrequency * 0.2) as probability
                        LIMIT 20";

                    var result = await tx.RunAsync(query);
                    var list = new List<UpsellOpportunityDto>();
                    await result.ForEachAsync(record =>
                    {
                        list.Add(new UpsellOpportunityDto
                        {
                            GuestId = record["guestId"].As<int>(),
                            GuestName = record["guestName"].As<string>(),
                            RecommendedService = record["serviceName"].As<string>(),
                            ProbabilityScore = Math.Min(record["probability"].As<double>(), 1.0),
                            Context = $"Based on {record["serviceType"]} history"
                        });
                    });
                    return list;
                }) ?? new List<UpsellOpportunityDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching upsell opportunities");
                return new List<UpsellOpportunityDto>();
            }
        }

        public async Task<List<ServiceFrictionReportDto>> GetDepartmentFrictionReportAsync()
        {
            try
            {
                return await _neo4jService.ExecuteReadAsync(async tx =>
                {
                    var query = @"
                        MATCH (s:Service)<-[r:EXPERIENCED|FEELS]-(g:Guest)
                        WHERE (labels(n) IN ['TransferDelay', 'NegativeSentiment', 'RestrictedAccess']) // n placeholder, context'e göre s veya ilişkili node
                        RETURN s.type as department, 
                               avg(COALESCE(r.weight, 1.0)) as avgFriction, 
                               count(r) as incidentCount
                        ORDER BY avgFriction DESC";

                    // Note: Query adjustment for actual schema labels
                    var refinedQuery = @"
                        MATCH (g:Guest)-[r:SATISFIES]->(s:Service)
                        WHERE r.sentiment < 0 OR r.satisfaction < 5
                        RETURN s.type as department, 
                               avg(abs(r.sentiment) * 10) as frictionScore, 
                               count(r) as incidentCount
                        ORDER BY frictionScore DESC";

                    var result = await tx.RunAsync(refinedQuery);
                    var list = new List<ServiceFrictionReportDto>();
                    await result.ForEachAsync(record =>
                    {
                        list.Add(new ServiceFrictionReportDto
                        {
                            Department = record["department"].As<string>(),
                            AverageFrictionScore = record["frictionScore"].As<double>(),
                            NegativeIncidentCount = record["incidentCount"].As<int>(),
                            CommonIssues = new List<string> { "Late Response", "Service Quality" } // Mocked for demo
                        });
                    });
                    return list;
                }) ?? new List<ServiceFrictionReportDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching friction report");
                return new List<ServiceFrictionReportDto>();
            }
        }

        public async Task<List<LoyaltyIntelligenceDto>> GetTopLoyaltyInsightsAsync()
        {
            try
            {
                return await _neo4jService.ExecuteReadAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest)
                        OPTIONAL MATCH (g)-[r:USES|INTERACTS]->()
                        WITH g, count(r) as interactions, COALESCE(g.influenceScore, 0) as influence
                        SET g.ltv = (interactions * 50) + (influence * 100)
                        RETURN g.id as guestId, g.name as name, g.ltv as ltv, 
                               interactions, influence
                        ORDER BY g.ltv DESC
                        LIMIT 10";

                    var result = await tx.RunAsync(query);
                    var list = new List<LoyaltyIntelligenceDto>();
                    await result.ForEachAsync(record =>
                    {
                        list.Add(new LoyaltyIntelligenceDto
                        {
                            GuestId = record["guestId"].As<int>(),
                            GuestName = record["name"].As<string>(),
                            LifeTimeValue = record["ltv"].As<double>(),
                            TotalInteractions = record["interactions"].As<int>(),
                            InfluenceScore = record["influence"].As<double>(),
                            LoyaltyTier = GetTier(record["ltv"].As<double>())
                        });
                    });
                    return list;
                }) ?? new List<LoyaltyIntelligenceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching loyalty insights");
                return new List<LoyaltyIntelligenceDto>();
            }
        }

        public async Task<List<UpsellOpportunityDto>> GetAIBundledOpportunitiesAsync()
        {
            try
            {
                var rawOpportunities = await GetUpsellOpportunitiesAsync();
                if (!rawOpportunities.Any()) return rawOpportunities;

                var dataToAnalyze = string.Join(" | ", rawOpportunities.Select(o => $"{o.GuestName}: {o.RecommendedService}"));

                var request = new AIChatRequest
                {
                    Message = $"Analyze the following sales opportunities and create 'Combo Packages' (Bundles) for similar profiles. Add a marketing text to make each opportunity more attractive. Include sustainability-themed bundles (e.g., 'Eco-Luxe Package') where possible. Opportunities: {dataToAnalyze}. Return the results in the original list structure but update the 'Context' field with the AI suggestion and set 'IsSustainable' accordingly.",
                    Context = "CommercialStrategyBundling"
                };

                var aiResponse = await _aiAssistantService.ProcessMessageAsync(request);
                
                foreach(var opt in rawOpportunities)
                {
                    opt.Context = aiResponse.Response.Length > 100 ? aiResponse.Response.Substring(0, 100) + "..." : aiResponse.Response;
                    opt.ProbabilityScore = Math.Min(opt.ProbabilityScore + 0.1, 1.0);
                    
                    // Simple heuristic for sustainable labeling in demo
                    if (opt.RecommendedService.Contains("Bike", StringComparison.OrdinalIgnoreCase) || 
                        opt.RecommendedService.Contains("Eco", StringComparison.OrdinalIgnoreCase) ||
                        opt.Context.Contains("sustainable", StringComparison.OrdinalIgnoreCase))
                    {
                        opt.IsSustainable = true;
                        opt.SustainabilityIncentive = "+50 Green Points";
                    }
                }

                return rawOpportunities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AI bundled opportunities");
                return await GetUpsellOpportunitiesAsync();
            }
        }

        public async Task<List<UpsellOpportunityDto>> GetSustainableBundleRecommendationsAsync()
        {
            try
            {
                var opportunities = await GetAIBundledOpportunitiesAsync();
                var sustainableOnes = opportunities.Where(o => o.IsSustainable).ToList();
                
                if (!sustainableOnes.Any() && opportunities.Any())
                {
                    // If no sustainable ones found, try to force-create one via AI for the top opportunity
                    var top = opportunities.First();
                    var request = new AIChatRequest
                    {
                        Message = $"Convert this upsell opportunity into a sustainable bundle: {top.RecommendedService} for guest {top.GuestName}. Provide a creative name and an environmental benefit explanation.",
                        Context = "SustainabilityBundleGeneration"
                    };
                    
                    var response = await _aiAssistantService.ProcessMessageAsync(request);
                    sustainableOnes.Add(new UpsellOpportunityDto
                    {
                        GuestId = top.GuestId,
                        GuestName = top.GuestName,
                        RecommendedService = "Eco-" + top.RecommendedService,
                        ProbabilityScore = top.ProbabilityScore,
                        Context = response.Response,
                        IsSustainable = true,
                        SustainabilityIncentive = "Sustainability Badge & 100 Points"
                    });
                }
                
                return sustainableOnes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sustainable bundle recommendations");
                return new List<UpsellOpportunityDto>();
            }
        }

        public async Task<CommercialDashboardSummaryDto> GetExecutiveSummaryAsync()
        {
            return new CommercialDashboardSummaryDto
            {
                TopUpsellOpportunities = await GetUpsellOpportunitiesAsync(),
                FrictionByDepartment = await GetDepartmentFrictionReportAsync(),
                HighValueGuests = await GetTopLoyaltyInsightsAsync(),
                GlobalQualityScore = 85.5 // Aggregated mock value
            };
        }

        private string GetTier(double ltv)
        {
            if (ltv > 5000) return "Diamond";
            if (ltv > 2000) return "Platinum";
            if (ltv > 1000) return "Gold";
            return "Silver";
        }
    }
}
