// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Sentiment;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Application.Operations.Intelligence.Predictive
{
    /// <summary>
    /// Predictive Intelligence Service implementation
    /// Note: This is a basic statistical model. For production, integrate ML.NET or external ML services
    /// </summary>
    public class PredictiveIntelligenceService : IPredictiveIntelligenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly IRelationshipIntelligenceService _relationshipIntelligenceService;
        private readonly ISentimentAnalysisService _sentimentAnalysisService;
        private readonly ILogger<PredictiveIntelligenceService> _logger;

        public PredictiveIntelligenceService(
            IUnitOfWork unitOfWork,
            IBehavioralTrackingService behavioralTrackingService,
            IRelationshipIntelligenceService relationshipIntelligenceService,
            ISentimentAnalysisService sentimentAnalysisService,
            ILogger<PredictiveIntelligenceService> logger)
        {
            _unitOfWork = unitOfWork;
            _behavioralTrackingService = behavioralTrackingService;
            _relationshipIntelligenceService = relationshipIntelligenceService;
            _sentimentAnalysisService = sentimentAnalysisService;
            _logger = logger;
        }

        public async Task<BehaviorPredictionResult> PredictGuestBehaviorAsync(int guestId, string behaviorType, DateTime? targetDate = null)
        {
            try
            {
                // Get historical behaviors
                var behaviors = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.BehaviorType == behaviorType && 
                                !b.IsDeleted)
                    .ToListAsync();

                if (!behaviors.Any())
                {
                    return new BehaviorPredictionResult
                    {
                        GuestId = guestId,
                        BehaviorType = behaviorType,
                        Probability = 0.5,
                        Prediction = "No historical data available",
                        Confidence = 0.0
                    };
                }

                // Calculate probability based on frequency and patterns
                var frequency = behaviors.Count;
                var recentBehaviors = behaviors.Where(b => b.BehaviorDate >= DateTime.UtcNow.AddDays(-30)).Count();
                var probability = Math.Min(1.0, (frequency / 10.0) * 0.5 + (recentBehaviors / 5.0) * 0.5);

                // Time-based factors
                if (targetDate.HasValue)
                {
                    var dayOfWeek = targetDate.Value.DayOfWeek.ToString();
                    var timeOfDay = GetTimeOfDay(targetDate.Value);
                    
                    var dayMatches = behaviors.Count(b => b.DayOfWeek == dayOfWeek);
                    var timeMatches = behaviors.Count(b => b.TimeOfDay == timeOfDay);
                    
                    if (dayMatches > 0 || timeMatches > 0)
                    {
                        probability += 0.2; // Increase probability if pattern matches
                    }
                }

                var prediction = probability > 0.7 ? "High likelihood" :
                                probability > 0.4 ? "Moderate likelihood" :
                                "Low likelihood";

                return new BehaviorPredictionResult
                {
                    GuestId = guestId,
                    BehaviorType = behaviorType,
                    Probability = Math.Min(1.0, probability),
                    Prediction = prediction,
                    Confidence = CalculateConfidence(behaviors.Count),
                    Factors = new Dictionary<string, object>
                    {
                        ["Frequency"] = frequency,
                        ["RecentFrequency"] = recentBehaviors,
                        ["AverageSatisfaction"] = behaviors.Where(b => b.SatisfactionScore.HasValue)
                            .Average(b => b.SatisfactionScore!.Value)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to predict guest behavior: GuestId={GuestId}, Type={BehaviorType}",
                    guestId, behaviorType);
                return new BehaviorPredictionResult
                {
                    GuestId = guestId,
                    BehaviorType = behaviorType,
                    Probability = 0.0,
                    Prediction = "Error",
                    Confidence = 0.0
                };
            }
        }

        public async Task<ServiceDemandPrediction> PredictServiceDemandAsync(string serviceType, DateTime targetDate)
        {
            try
            {
                // Get historical service data for this date range (same day of week, same season)
                var dayOfWeek = targetDate.DayOfWeek.ToString();
                var season = GetSeason(targetDate);
                var dateRange = new DateTimeRange(targetDate.AddDays(-30), targetDate.AddDays(30));

                var historicalServices = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.Category == serviceType && 
                                b.BehaviorType == "Service" &&
                                b.BehaviorDate >= dateRange.Start &&
                                b.BehaviorDate <= dateRange.End &&
                                !b.IsDeleted)
                    .ToListAsync();

                // Filter by day of week and season
                var similarDayServices = historicalServices
                    .Where(b => b.DayOfWeek == dayOfWeek)
                    .Count();

                var similarSeasonServices = historicalServices
                    .Where(b => b.Season == season)
                    .Count();

                // Simple prediction: average of similar days
                var predictedDemand = (similarDayServices + similarSeasonServices) / 2.0;
                if (predictedDemand == 0)
                {
                    predictedDemand = historicalServices.Count / 60.0; // Average per day
                }

                return new ServiceDemandPrediction
                {
                    ServiceType = serviceType,
                    TargetDate = targetDate,
                    PredictedDemand = (int)Math.Ceiling(predictedDemand),
                    Confidence = CalculateConfidence(historicalServices.Count),
                    Factors = new Dictionary<string, object>
                    {
                        ["DayOfWeek"] = dayOfWeek,
                        ["Season"] = season,
                        ["HistoricalCount"] = historicalServices.Count,
                        ["SimilarDayCount"] = similarDayServices,
                        ["SimilarSeasonCount"] = similarSeasonServices
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to predict service demand: ServiceType={ServiceType}, Date={Date}",
                    serviceType, targetDate);
                return new ServiceDemandPrediction
                {
                    ServiceType = serviceType,
                    TargetDate = targetDate,
                    PredictedDemand = 0,
                    Confidence = 0.0
                };
            }
        }

        public async Task<SatisfactionPrediction> PredictGuestSatisfactionAsync(int guestId, int? serviceId = null, string? serviceType = null)
        {
            try
            {
                var query = _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.SatisfactionScore.HasValue &&
                                !b.IsDeleted);

                if (serviceId.HasValue)
                {
                    query = query.Where(b => b.RelatedEntityId == serviceId.Value);
                }
                if (!string.IsNullOrEmpty(serviceType))
                {
                    query = query.Where(b => b.Category == serviceType);
                }

                var behaviors = await query.ToListAsync();

                if (!behaviors.Any())
                {
                    return new SatisfactionPrediction
                    {
                        GuestId = guestId,
                        PredictedSatisfaction = 5.0, // Neutral
                        Confidence = 0.0,
                        RiskLevel = "Low"
                    };
                }

                // Calculate average satisfaction
                var avgSatisfaction = behaviors.Average(b => b.SatisfactionScore!.Value);
                
                // Get recent sentiment trends
                var sentimentTrends = await _sentimentAnalysisService.GetGuestSentimentTrendsAsync(guestId);
                var avgSentiment = sentimentTrends.ContainsKey("AverageSentiment") 
                    ? (double)sentimentTrends["AverageSentiment"] 
                    : 0.0;

                // Adjust prediction based on sentiment
                var predictedSatisfaction = (avgSatisfaction * 0.7) + ((avgSentiment + 1.0) * 5.0 * 0.3);

                // Determine risk level
                var riskLevel = predictedSatisfaction switch
                {
                    < 4.0 => "High",
                    < 6.0 => "Medium",
                    _ => "Low"
                };

                return new SatisfactionPrediction
                {
                    GuestId = guestId,
                    PredictedSatisfaction = Math.Max(0.0, Math.Min(10.0, predictedSatisfaction)),
                    Confidence = CalculateConfidence(behaviors.Count),
                    RiskLevel = riskLevel,
                    Factors = new Dictionary<string, object>
                    {
                        ["HistoricalAverage"] = avgSatisfaction,
                        ["AverageSentiment"] = avgSentiment,
                        ["DataPoints"] = behaviors.Count,
                        ["Trend"] = sentimentTrends.ContainsKey("Trend") ? sentimentTrends["Trend"] : "Stable"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to predict satisfaction: GuestId={GuestId}", guestId);
                return new SatisfactionPrediction
                {
                    GuestId = guestId,
                    PredictedSatisfaction = 5.0,
                    Confidence = 0.0,
                    RiskLevel = "Low"
                };
            }
        }

        public async Task<RiskPredictionResult> PredictRisksAsync(int guestId)
        {
            try
            {
                var risks = new List<RiskFactor>();

                // Dissatisfaction risk
                var satisfactionPrediction = await PredictGuestSatisfactionAsync(guestId);
                if (satisfactionPrediction.PredictedSatisfaction < 6.0)
                {
                    risks.Add(new RiskFactor
                    {
                        RiskType = "Dissatisfaction",
                        RiskScore = 1.0 - (satisfactionPrediction.PredictedSatisfaction / 10.0),
                        Severity = satisfactionPrediction.RiskLevel,
                        Description = $"Predicted satisfaction: {satisfactionPrediction.PredictedSatisfaction:F1}/10",
                        Factors = satisfactionPrediction.Factors
                    });
                }

                // Cancellation risk (based on recent behaviors and sentiment)
                var behaviors = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && !b.IsDeleted)
                    .OrderByDescending(b => b.BehaviorDate)
                    .Take(10)
                    .ToListAsync();

                var negativeBehaviors = behaviors.Count(b => b.SentimentScore.HasValue && b.SentimentScore.Value < -0.3);
                if (negativeBehaviors > 2)
                {
                    risks.Add(new RiskFactor
                    {
                        RiskType = "Cancellation",
                        RiskScore = Math.Min(1.0, negativeBehaviors / 5.0),
                        Severity = negativeBehaviors > 4 ? "High" : "Medium",
                        Description = $"{negativeBehaviors} recent negative interactions detected"
                    });
                }

                // Problem risk (based on complaint patterns)
                var complaints = behaviors.Count(b => b.BehaviorType == "Complaint" || 
                                                     (b.SentimentScore.HasValue && b.SentimentScore.Value < -0.5));
                if (complaints > 0)
                {
                    risks.Add(new RiskFactor
                    {
                        RiskType = "Problem",
                        RiskScore = Math.Min(1.0, complaints / 3.0),
                        Severity = complaints > 2 ? "High" : "Medium",
                        Description = $"{complaints} complaint(s) detected"
                    });
                }

                // Churn risk (based on decreasing engagement)
                var recentBehaviors = behaviors.Count(b => b.BehaviorDate >= DateTime.UtcNow.AddDays(-30));
                var olderBehaviors = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.BehaviorDate >= DateTime.UtcNow.AddDays(-60) &&
                                b.BehaviorDate < DateTime.UtcNow.AddDays(-30) &&
                                !b.IsDeleted)
                    .CountAsync();

                if (olderBehaviors > 0 && recentBehaviors < olderBehaviors * 0.5)
                {
                    risks.Add(new RiskFactor
                    {
                        RiskType = "Churn",
                        RiskScore = 0.6,
                        Severity = "Medium",
                        Description = "Decreasing engagement detected"
                    });
                }

                var overallRiskScore = risks.Any() ? risks.Average(r => r.RiskScore) : 0.0;

                return new RiskPredictionResult
                {
                    GuestId = guestId,
                    Risks = risks,
                    OverallRiskScore = overallRiskScore
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to predict risks: GuestId={GuestId}", guestId);
                return new RiskPredictionResult
                {
                    GuestId = guestId,
                    Risks = new List<RiskFactor>(),
                    OverallRiskScore = 0.0
                };
            }
        }

        public async Task<List<OpportunityDetection>> DetectOpportunitiesAsync(int guestId)
        {
            try
            {
                var opportunities = new List<OpportunityDetection>();

                // Get guest behavior patterns
                var patterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);
                var preferences = await _relationshipIntelligenceService.GetGuestPreferencePatternsAsync(guestId);

                // Upsell opportunities (guest has high satisfaction but low spending)
                var avgSatisfaction = patterns.ContainsKey("AverageSatisfaction") 
                    ? (double)patterns["AverageSatisfaction"] 
                    : 5.0;
                var totalSpending = patterns.ContainsKey("TotalSpending") 
                    ? (decimal)patterns["TotalSpending"] 
                    : 0;

                if (avgSatisfaction > 7.0 && totalSpending < 1000)
                {
                    opportunities.Add(new OpportunityDetection
                    {
                        OpportunityType = "Upsell",
                        Description = "High satisfaction guest with low spending - upsell potential",
                        OpportunityScore = 0.8,
                        RecommendedAction = "Recommend premium services or packages",
                        Context = new Dictionary<string, object>
                        {
                            ["AverageSatisfaction"] = avgSatisfaction,
                            ["TotalSpending"] = totalSpending
                        }
                    });
                }

                // Cross-sell opportunities (guest uses one service type but not others)
                var serviceMatches = await _relationshipIntelligenceService.FindBestServiceMatchesAsync(guestId, null, 10);
                var usedServiceTypes = serviceMatches.Select(m => m.ServiceType).Distinct().ToList();
                var allServiceTypes = new[] { "Transfer", "CityTour", "YachtTour", "Restaurant" };
                var unusedServiceTypes = allServiceTypes.Except(usedServiceTypes).ToList();

                if (unusedServiceTypes.Any() && usedServiceTypes.Any())
                {
                    opportunities.Add(new OpportunityDetection
                    {
                        OpportunityType = "CrossSell",
                        Description = $"Guest uses {string.Join(", ", usedServiceTypes)} but hasn't tried {string.Join(", ", unusedServiceTypes)}",
                        OpportunityScore = 0.7,
                        RecommendedAction = $"Recommend {unusedServiceTypes.First()} services",
                        Context = new Dictionary<string, object>
                        {
                            ["UsedServices"] = usedServiceTypes,
                            ["UnusedServices"] = unusedServiceTypes
                        }
                    });
                }

                // Personalization opportunities (guest has clear preferences)
                if (preferences.ContainsKey("ServicePreferences"))
                {
                    var servicePrefs = preferences["ServicePreferences"] as Dictionary<string, int>;
                    if (servicePrefs != null && servicePrefs.Count > 0)
                    {
                        opportunities.Add(new OpportunityDetection
                        {
                            OpportunityType = "Personalization",
                            Description = "Guest has clear service preferences - personalize recommendations",
                            OpportunityScore = 0.9,
                            RecommendedAction = "Use preference-based recommendations",
                            Context = new Dictionary<string, object>
                            {
                                ["Preferences"] = servicePrefs
                            }
                        });
                    }
                }

                // Loyalty opportunities (repeat guest with high satisfaction)
                var totalBehaviors = patterns.ContainsKey("TotalBehaviors") 
                    ? (int)patterns["TotalBehaviors"] 
                    : 0;
                
                if (totalBehaviors > 5 && avgSatisfaction > 8.0)
                {
                    opportunities.Add(new OpportunityDetection
                    {
                        OpportunityType = "Loyalty",
                        Description = "High-value repeat guest - loyalty program candidate",
                        OpportunityScore = 0.85,
                        RecommendedAction = "Offer loyalty benefits or special recognition",
                        Context = new Dictionary<string, object>
                        {
                            ["TotalBehaviors"] = totalBehaviors,
                            ["AverageSatisfaction"] = avgSatisfaction
                        }
                    });
                }

                return opportunities.OrderByDescending(o => o.OpportunityScore).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to detect opportunities: GuestId={GuestId}", guestId);
                return new List<OpportunityDetection>();
            }
        }

        public async Task<SpendingPrediction> PredictGuestSpendingAsync(int guestId, DateTime? targetDate = null)
        {
            try
            {
                var behaviors = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.Amount.HasValue &&
                                !b.IsDeleted)
                    .ToListAsync();

                if (!behaviors.Any())
                {
                    return new SpendingPrediction
                    {
                        GuestId = guestId,
                        PredictedSpending = 0,
                        Currency = "TRY",
                        Confidence = 0.0
                    };
                }

                // Calculate average spending
                var avgSpending = behaviors.Average(b => b.Amount!.Value);
                var currency = behaviors.First().Currency ?? "TRY";

                // Adjust for date if provided (seasonal factors)
                var predictedSpending = avgSpending;
                if (targetDate.HasValue)
                {
                    var season = GetSeason(targetDate.Value);
                    var seasonalBehaviors = behaviors.Where(b => b.Season == season).ToList();
                    if (seasonalBehaviors.Any())
                    {
                        predictedSpending = seasonalBehaviors.Average(b => b.Amount!.Value);
                    }
                }

                // Spending by category
                var spendingByCategory = behaviors
                    .Where(b => !string.IsNullOrEmpty(b.Category) && b.Amount.HasValue)
                    .GroupBy(b => b.Category!)
                    .ToDictionary(g => g.Key, g => g.Sum(b => b.Amount!.Value));

                return new SpendingPrediction
                {
                    GuestId = guestId,
                    PredictedSpending = predictedSpending,
                    Currency = currency,
                    Confidence = CalculateConfidence(behaviors.Count),
                    SpendingByCategory = spendingByCategory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to predict spending: GuestId={GuestId}", guestId);
                return new SpendingPrediction
                {
                    GuestId = guestId,
                    PredictedSpending = 0,
                    Currency = "TRY",
                    Confidence = 0.0
                };
            }
        }

        private double CalculateConfidence(int dataPoints)
        {
            // Confidence increases with more data points
            return Math.Min(1.0, dataPoints / 20.0);
        }

        private string GetTimeOfDay(DateTime dateTime)
        {
            var hour = dateTime.Hour;
            return hour switch
            {
                >= 5 and < 12 => "Morning",
                >= 12 and < 17 => "Afternoon",
                >= 17 and < 21 => "Evening",
                _ => "Night"
            };
        }

        private string GetSeason(DateTime date)
        {
            var month = date.Month;
            return month switch
            {
                >= 3 and <= 5 => "Spring",
                >= 6 and <= 8 => "Summer",
                >= 9 and <= 11 => "Autumn",
                _ => "Winter"
            };
        }
    }

    /// <summary>
    /// DateTime range helper
    /// </summary>
    internal class DateTimeRange
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public DateTimeRange(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}
