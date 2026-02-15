// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Sentiment;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Intelligence.Proactive
{
    /// <summary>
    /// Proactive Intelligence Service implementation
    /// </summary>
    public class ProactiveIntelligenceService : IProactiveIntelligenceService
    {
        private readonly IPredictiveIntelligenceService _predictiveIntelligenceService;
        private readonly IRelationshipIntelligenceService _relationshipIntelligenceService;
        private readonly ISentimentAnalysisService _sentimentAnalysisService;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IAutomaticActionService _automaticActionService;
        private readonly ILogger<ProactiveIntelligenceService> _logger;

        public ProactiveIntelligenceService(
            IPredictiveIntelligenceService predictiveIntelligenceService,
            IRelationshipIntelligenceService relationshipIntelligenceService,
            ISentimentAnalysisService sentimentAnalysisService,
            IBehavioralTrackingService behavioralTrackingService,
            IAIAssistantService aiAssistantService,
            IAutomaticActionService automaticActionService,
            ILogger<ProactiveIntelligenceService> logger)
        {
            _predictiveIntelligenceService = predictiveIntelligenceService;
            _relationshipIntelligenceService = relationshipIntelligenceService;
            _sentimentAnalysisService = sentimentAnalysisService;
            _behavioralTrackingService = behavioralTrackingService;
            _aiAssistantService = aiAssistantService;
            _automaticActionService = automaticActionService;
            _logger = logger;
        }

        public async Task<List<ProactiveRecommendation>> GetProactiveRecommendationsAsync(int guestId, DateTime? targetDate = null)
        {
            try
            {
                var recommendations = new List<ProactiveRecommendation>();

                // Get service recommendations from Relationship Intelligence
                var serviceRecommendations = await _relationshipIntelligenceService.RecommendServicesAsync(guestId, targetDate);
                foreach (var rec in serviceRecommendations)
                {
                    recommendations.Add(new ProactiveRecommendation
                    {
                        GuestId = guestId,
                        RecommendationType = "Service",
                        Title = $"Recommended Service: {rec.ServiceName}",
                        Description = rec.RecommendationReason,
                        Priority = rec.RecommendationScore,
                        RecommendedAction = $"Offer {rec.ServiceName} to guest",
                        RecommendedDate = rec.RecommendedDate,
                        Context = rec.Context
                    });
                }

                // Get behavior predictions
                var behaviorPrediction = await _predictiveIntelligenceService.PredictGuestBehaviorAsync(
                    guestId, "Service", targetDate);
                
                if (behaviorPrediction.Probability > 0.7)
                {
                    recommendations.Add(new ProactiveRecommendation
                    {
                        GuestId = guestId,
                        RecommendationType = "Behavior",
                        Title = "High Service Demand Expected",
                        Description = $"Guest is likely to request services ({behaviorPrediction.Prediction})",
                        Priority = behaviorPrediction.Probability,
                        RecommendedAction = "Prepare service options in advance",
                        RecommendedDate = targetDate,
                        Context = behaviorPrediction.Factors
                    });
                }

                // Get sentiment-based recommendations
                var sentimentTrends = await _sentimentAnalysisService.GetGuestSentimentTrendsAsync(guestId);
                if (sentimentTrends.ContainsKey("Trend"))
                {
                    var trend = sentimentTrends["Trend"]?.ToString();
                    if (trend == "Declining")
                    {
                        recommendations.Add(new ProactiveRecommendation
                        {
                            GuestId = guestId,
                            RecommendationType = "Emotion",
                            Title = "Sentiment Declining - Intervention Needed",
                            Description = "Guest sentiment is declining. Proactive engagement recommended.",
                            Priority = 0.9,
                            RecommendedAction = "Reach out to guest with personalized service offer",
                            Context = sentimentTrends
                        });
                    }
                }

                var rawRecommendations = recommendations.OrderByDescending(r => r.Priority).ToList();
                
                // Try to refine with AI for higher quality output
                var refined = await RefineRecommendationsWithAIAsync(guestId, rawRecommendations);
                return refined ?? rawRecommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get proactive recommendations: GuestId={GuestId}", guestId);
                return new List<ProactiveRecommendation>();
            }
        }

        private async Task<List<ProactiveRecommendation>?> RefineRecommendationsWithAIAsync(int guestId, List<ProactiveRecommendation> recommendations)
        {
            try
            {
                var prompt = $@"As a luxury hotel concierge manager, refine these proactive recommendations for Guest {guestId}.
                Make the descriptions more professional and provide specific 'ImprovedAction' steps.
                Data: {JsonSerializer.Serialize(recommendations)}

                Return a JSON object:
                {{
                    ""refined"": [
                        {{
                            ""title"": ""Refined Title"",
                            ""description"": ""Refined nuanced description"",
                            ""recommendedAction"": ""Specific next step""
                        }}
                    ]
                }}
                Response ONLY with JSON.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "ProactiveRefinement" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var result = JsonSerializer.Deserialize<RefinedResult>(response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1), 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Refined != null && result.Refined.Count == recommendations.Count)
                {
                    for (int i = 0; i < recommendations.Count; i++)
                    {
                        recommendations[i].Title = result.Refined[i].Title;
                        recommendations[i].Description = result.Refined[i].Description;
                        recommendations[i].RecommendedAction = result.Refined[i].RecommendedAction;
                    }
                    return recommendations;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refine proactive recommendations with AI.");
                return null;
            }
        }

        private class RefinedResult
        {
            public List<RefinedItem> Refined { get; set; } = new();
        }

        private class RefinedItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string RecommendedAction { get; set; } = string.Empty;
        }

        private class RefinedActionItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
        }

        private class RefinedActionResult
        {
            public List<RefinedActionItem> Refined { get; set; } = new();
        }

        public async Task<List<ProblemPreventionAlert>> GetProblemPreventionAlertsAsync(int? guestId = null)
        {
            try
            {
                var alerts = new List<ProblemPreventionAlert>();

                if (guestId.HasValue)
                {
                    // Get risk predictions for specific guest
                    var risks = await _predictiveIntelligenceService.PredictRisksAsync(guestId.Value);
                    
                    foreach (var risk in risks.Risks)
                    {
                        alerts.Add(new ProblemPreventionAlert
                        {
                            GuestId = guestId,
                            AlertType = risk.RiskType,
                            Severity = risk.Severity,
                            Title = $"{risk.RiskType} Risk Detected",
                            Description = risk.Description,
                            RecommendedIntervention = GetInterventionForRisk(risk.RiskType, risk.Severity),
                            AlertDate = DateTime.UtcNow,
                            RiskFactors = risk.Factors
                        });
                    }
                }
                else
                {
                    // Get early warning signals for all guests
                    var warnings = await GetEarlyWarningSignalsAsync(null);
                    foreach (var warning in warnings.Where(w => w.Severity != "Low"))
                    {
                        alerts.Add(new ProblemPreventionAlert
                        {
                            GuestId = warning.GuestId,
                            AlertType = warning.SignalType,
                            Severity = warning.Severity,
                            Title = warning.Message,
                            Description = $"Early warning: {warning.SignalType}",
                            RecommendedIntervention = "Review guest status and consider proactive intervention",
                            AlertDate = warning.DetectedAt,
                            RiskFactors = warning.Indicators
                        });
                    }
                }

                var rawAlerts = alerts.OrderByDescending(a => GetSeverityWeight(a.Severity)).ToList();
                
                // Try to refine with AI for higher quality output
                var refined = await RefineAlertsWithAIAsync(guestId, rawAlerts);
                return refined ?? rawAlerts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get problem prevention alerts: GuestId={GuestId}", guestId);
                return new List<ProblemPreventionAlert>();
            }
        }

        private async Task<List<ProblemPreventionAlert>?> RefineAlertsWithAIAsync(int? guestId, List<ProblemPreventionAlert> alerts)
        {
            try
            {
                var prompt = $@"As a luxury hotel risk manager, refine these proactive problem prevention alerts. 
                Improve the 'Title', 'Description', and provide a highly specific 'RecommendedIntervention'.
                Data: {JsonSerializer.Serialize(alerts)}

                Return a JSON object:
                {{
                    ""refined"": [
                        {{
                            ""title"": ""Refined Title"",
                            ""description"": ""Nuanced risk description"",
                            ""recommendedIntervention"": ""Specific actionable step""
                        }}
                    ]
                }}
                Response ONLY with JSON.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "AlertRefinement" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var result = JsonSerializer.Deserialize<RefinedAlertResult>(response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1), 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Refined != null && result.Refined.Count == alerts.Count)
                {
                    for (int i = 0; i < alerts.Count; i++)
                    {
                        alerts[i].Title = result.Refined[i].Title;
                        alerts[i].Description = result.Refined[i].Description;
                        alerts[i].RecommendedIntervention = result.Refined[i].RecommendedIntervention;
                    }
                    return alerts;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refine alerts with AI.");
                return null;
            }
        }

        private class RefinedAlertResult
        {
            public List<RefinedAlertItem> Refined { get; set; } = new();
        }

        private class RefinedAlertItem
        {
            public string Title { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string RecommendedIntervention { get; set; } = string.Empty;
        }

        public async Task<List<PersonalizationSuggestion>> GetPersonalizationSuggestionsAsync(int guestId)
        {
            try
            {
                var suggestions = new List<PersonalizationSuggestion>();

                // Get preference patterns
                var preferences = await _relationshipIntelligenceService.GetGuestPreferencePatternsAsync(guestId);
                
                if (preferences.ContainsKey("ServicePreferences"))
                {
                    var servicePrefs = preferences["ServicePreferences"] as Dictionary<string, int>;
                    if (servicePrefs != null && servicePrefs.Any())
                    {
                        var topPreference = servicePrefs.OrderByDescending(p => p.Value).First();
                        suggestions.Add(new PersonalizationSuggestion
                        {
                            GuestId = guestId,
                            SuggestionType = "Preference",
                            Title = $"Personalize with {topPreference.Key}",
                            Description = $"Guest has strong preference for {topPreference.Key} services ({topPreference.Value} times)",
                            Confidence = Math.Min(1.0, topPreference.Value / 5.0),
                            SuggestedAction = $"Prioritize {topPreference.Key} services in recommendations",
                            Context = preferences
                        });
                    }
                }

                // Get best staff match for personalization
                var staffMatches = await _relationshipIntelligenceService.FindBestStaffMatchesAsync(guestId, 1);
                if (staffMatches.Any())
                {
                    var bestMatch = staffMatches.First();
                    suggestions.Add(new PersonalizationSuggestion
                    {
                        GuestId = guestId,
                        SuggestionType = "Service",
                        Title = $"Assign {bestMatch.StaffName}",
                        Description = $"Best staff match for personalized service (Compatibility: {bestMatch.CompatibilityScore:F2})",
                        Confidence = bestMatch.CompatibilityScore,
                        SuggestedAction = $"Assign {bestMatch.StaffName} for guest interactions",
                        Context = new Dictionary<string, object>
                        {
                            ["StaffId"] = bestMatch.StaffId,
                            ["CompatibilityScore"] = bestMatch.CompatibilityScore,
                            ["AverageSatisfaction"] = bestMatch.AverageSatisfaction
                        }
                    });
                }

                // Get time-based personalization
                var behaviorPatterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);
                if (behaviorPatterns.ContainsKey("TimeOfDay"))
                {
                    var timePrefs = behaviorPatterns["TimeOfDay"] as Dictionary<string, int>;
                    if (timePrefs != null && timePrefs.Any())
                    {
                        var preferredTime = timePrefs.OrderByDescending(p => p.Value).First();
                        suggestions.Add(new PersonalizationSuggestion
                        {
                            GuestId = guestId,
                            SuggestionType = "Experience",
                            Title = $"Schedule Services for {preferredTime.Key}",
                            Description = $"Guest prefers {preferredTime.Key} activities",
                            Confidence = Math.Min(1.0, preferredTime.Value / 3.0),
                            SuggestedAction = $"Schedule services during {preferredTime.Key} when possible",
                            Context = new Dictionary<string, object> { ["PreferredTime"] = preferredTime.Key }
                        });
                    }
                }

                return suggestions.OrderByDescending(s => s.Confidence).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get personalization suggestions: GuestId={GuestId}", guestId);
                return new List<PersonalizationSuggestion>();
            }
        }

        public async Task<List<EarlyWarningSignal>> GetEarlyWarningSignalsAsync(int? guestId = null)
        {
            try
            {
                var signals = new List<EarlyWarningSignal>();

                // This would typically query all guests or specific guest
                // For now, we'll use predictive intelligence to detect warnings
                if (guestId.HasValue)
                {
                    var satisfactionPrediction = await _predictiveIntelligenceService.PredictGuestSatisfactionAsync(guestId.Value);
                    
                    if (satisfactionPrediction.RiskLevel == "High")
                    {
                        signals.Add(new EarlyWarningSignal
                        {
                            GuestId = guestId,
                            SignalType = "Satisfaction",
                            Severity = "High",
                            Message = $"Low satisfaction predicted: {satisfactionPrediction.PredictedSatisfaction:F1}/10",
                            DetectedAt = DateTime.UtcNow,
                            Indicators = satisfactionPrediction.Factors
                        });
                    }

                    var sentimentTrends = await _sentimentAnalysisService.GetGuestSentimentTrendsAsync(guestId.Value);
                    if (sentimentTrends.ContainsKey("AverageSentiment"))
                    {
                        var avgSentiment = (double)sentimentTrends["AverageSentiment"];
                        if (avgSentiment < -0.3)
                        {
                            signals.Add(new EarlyWarningSignal
                            {
                                GuestId = guestId,
                                SignalType = "Sentiment",
                                Severity = avgSentiment < -0.6 ? "High" : "Medium",
                                Message = $"Negative sentiment detected: {avgSentiment:F2}",
                                DetectedAt = DateTime.UtcNow,
                                Indicators = sentimentTrends
                            });
                        }
                    }
                }

                return signals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get early warning signals: GuestId={GuestId}", guestId);
                return new List<EarlyWarningSignal>();
            }
        }

        public async Task<List<AutomaticAction>> GetAutomaticActionRecommendationsAsync(int guestId)
        {
            try
            {
                var actions = new List<AutomaticAction>();

                // Check if guest needs welcome message
                var behaviorPatterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);
                var totalBehaviors = behaviorPatterns.ContainsKey("TotalBehaviors") 
                    ? (int)behaviorPatterns["TotalBehaviors"] 
                    : 0;

                if (totalBehaviors == 0)
                {
                    actions.Add(new AutomaticAction
                    {
                        GuestId = guestId,
                        ActionType = "Message",
                        Title = "Send Welcome Message",
                        Description = "New guest - send personalized welcome message",
                        CanExecuteAutomatically = true,
                        ExecutionDetails = "Send welcome email/SMS with service information",
                        Confidence = 0.9
                    });
                }

                // Check for high satisfaction - offer loyalty benefits
                var satisfactionPrediction = await _predictiveIntelligenceService.PredictGuestSatisfactionAsync(guestId);
                if (satisfactionPrediction.PredictedSatisfaction > 8.0)
                {
                    actions.Add(new AutomaticAction
                    {
                        GuestId = guestId,
                        ActionType = "Service",
                        Title = "Offer Premium Service",
                        Description = "High satisfaction guest - offer premium service upgrade",
                        CanExecuteAutomatically = false, // Requires approval
                        ExecutionDetails = "Suggest premium service package",
                        Confidence = 0.8
                    });
                }

                // Check for declining sentiment - send check-in message
                var sentimentTrends = await _sentimentAnalysisService.GetGuestSentimentTrendsAsync(guestId);
                if (sentimentTrends.ContainsKey("Trend") && sentimentTrends["Trend"]?.ToString() == "Declining")
                {
                    actions.Add(new AutomaticAction
                    {
                        GuestId = guestId,
                        ActionType = "Message",
                        Title = "Proactive Check-in",
                        Description = "Sentiment declining - send proactive check-in message",
                        CanExecuteAutomatically = true,
                        ExecutionDetails = "Send personalized message asking about experience",
                        Confidence = 0.85
                    });
                }

                // Refine actions with AI to make them "send-ready"
                var refinedActions = await RefineAutomaticActionsWithAIAsync(guestId, actions);
                if (refinedActions != null)
                {
                    actions = refinedActions;
                }

                var finalActions = actions.OrderByDescending(a => a.Confidence).ToList();

                // Process high confidence actions automatically
                foreach (var action in finalActions.Where(a => a.Confidence >= 0.9 && a.CanExecuteAutomatically))
                {
                    _ = Task.Run(() => _automaticActionService.ExecuteActionAsync(action));
                }

                return finalActions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get automatic action recommendations: GuestId={GuestId}", guestId);
                return new List<AutomaticAction>();
            }
        }

        private string GetInterventionForRisk(string riskType, string severity)
        {
            return riskType switch
            {
                "Dissatisfaction" => severity == "High" 
                    ? "Immediate intervention: Assign best staff, offer premium service, manager contact"
                    : "Proactive engagement: Personalized service offer, check-in message",
                "Cancellation" => "Contact guest immediately, offer flexible options, special discount",
                "Problem" => "Review recent interactions, assign problem-solving specialist, follow-up",
                "Churn" => "Loyalty program offer, personalized retention campaign, special recognition",
                _ => "Monitor closely and provide personalized attention"
            };
        }

        private int GetSeverityWeight(string severity)
        {
            return severity switch
            {
                "Critical" => 4,
                "High" => 3,
                "Medium" => 2,
                "Low" => 1,
                _ => 0
            };
        }

        private async Task<List<AutomaticAction>?> RefineAutomaticActionsWithAIAsync(int guestId, List<AutomaticAction> actions)
        {
            try
            {
                var prompt = $@"As an AI customer experience specialist for a luxury hotel, refine these automatic actions for Guest {guestId}.
                For 'Message' actions, the 'Description' should be the EXACT message content to be sent to the guest via WhatsApp/SMS.
                Make it professional, personalized, and warm. Use available context.
                
                Data: {JsonSerializer.Serialize(actions)}

                Return a JSON object:
                {{
                    ""refined"": [
                        {{
                            ""title"": ""Refined Action Title"",
                            ""description"": ""Personalized content or message body""
                        }}
                    ]
                }}
                Response ONLY with JSON.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "AutomaticActionRefinement" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var result = JsonSerializer.Deserialize<RefinedActionResult>(response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Refined != null && result.Refined.Count == actions.Count)
                {
                    for (int i = 0; i < actions.Count; i++)
                    {
                        actions[i].Title = result.Refined[i].Title;
                        actions[i].Description = result.Refined[i].Description;
                    }
                    return actions;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refine automatic actions with AI.");
                return null;
            }
        }
    }
}
