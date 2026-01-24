// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Intelligence.Sentiment;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Application.Operations.Intelligence.Proactive;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Intelligence Layer API endpoints - Turizm Operasyon Intelligence Layer
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    [Tags("Intelligence Layer")]
    public class IntelligenceController : BaseController
    {
        private readonly ISentimentAnalysisService _sentimentAnalysisService;
        private readonly IRelationshipIntelligenceService _relationshipIntelligenceService;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly IPredictiveIntelligenceService _predictiveIntelligenceService;
        private readonly IProactiveIntelligenceService _proactiveIntelligenceService;

        public IntelligenceController(
            ISentimentAnalysisService sentimentAnalysisService,
            IRelationshipIntelligenceService relationshipIntelligenceService,
            IBehavioralTrackingService behavioralTrackingService,
            IPredictiveIntelligenceService predictiveIntelligenceService,
            IProactiveIntelligenceService proactiveIntelligenceService)
        {
            _sentimentAnalysisService = sentimentAnalysisService;
            _relationshipIntelligenceService = relationshipIntelligenceService;
            _behavioralTrackingService = behavioralTrackingService;
            _predictiveIntelligenceService = predictiveIntelligenceService;
            _proactiveIntelligenceService = proactiveIntelligenceService;
        }

        /// <summary>
        /// Analyze text sentiment
        /// </summary>
        [HttpPost("sentiment/analyze")]
        [ProducesResponseType(typeof(ApiResponse<SentimentAnalysisResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AnalyzeSentiment([FromBody] AnalyzeSentimentRequest request)
        {
            try
            {
                var result = await _sentimentAnalysisService.AnalyzeSentimentAsync(request.Text, request.Language);
                return Success(result, "Sentiment analyzed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to analyze sentiment.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get guest sentiment trends
        /// </summary>
        [HttpGet("guests/{guestId}/sentiment-trends")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGuestSentimentTrends(
            int guestId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var trends = await _sentimentAnalysisService.GetGuestSentimentTrendsAsync(guestId, startDate, endDate);
                return Success(trends, "Guest sentiment trends retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get guest sentiment trends.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Find best staff matches for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/staff-matches")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<StaffMatchResult>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindBestStaffMatches(int guestId, [FromQuery] int? limit = 5)
        {
            try
            {
                var matches = await _relationshipIntelligenceService.FindBestStaffMatchesAsync(guestId, limit);
                return Success(matches, "Best staff matches retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to find staff matches.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Find best service matches for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/service-matches")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<ServiceMatchResult>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> FindBestServiceMatches(
            int guestId,
            [FromQuery] string? serviceType = null,
            [FromQuery] int? limit = 10)
        {
            try
            {
                var matches = await _relationshipIntelligenceService.FindBestServiceMatchesAsync(guestId, serviceType, limit);
                return Success(matches, "Best service matches retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to find service matches.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate guest-staff compatibility
        /// </summary>
        [HttpGet("guests/{guestId}/staff/{staffId}/compatibility")]
        [ProducesResponseType(typeof(ApiResponse<double>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CalculateCompatibility(int guestId, int staffId)
        {
            try
            {
                var compatibility = await _relationshipIntelligenceService.CalculateCompatibilityAsync(guestId, staffId);
                return Success(compatibility, "Compatibility calculated successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to calculate compatibility.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get guest preference patterns
        /// </summary>
        [HttpGet("guests/{guestId}/preference-patterns")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGuestPreferencePatterns(int guestId)
        {
            try
            {
                var patterns = await _relationshipIntelligenceService.GetGuestPreferencePatternsAsync(guestId);
                return Success(patterns, "Guest preference patterns retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get preference patterns.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Recommend services for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/service-recommendations")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<ServiceRecommendation>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecommendServices(int guestId, [FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var recommendations = await _relationshipIntelligenceService.RecommendServicesAsync(guestId, targetDate);
                return Success(recommendations, "Service recommendations retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get service recommendations.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get guest behavior patterns
        /// </summary>
        [HttpGet("guests/{guestId}/behavior-patterns")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGuestBehaviorPatterns(
            int guestId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var patterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId, startDate, endDate);
                return Success(patterns, "Guest behavior patterns retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get behavior patterns.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get staff behavior patterns
        /// </summary>
        [HttpGet("staff/{staffId}/behavior-patterns")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStaffBehaviorPatterns(
            int staffId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var patterns = await _behavioralTrackingService.GetStaffBehaviorPatternsAsync(staffId, startDate, endDate);
                return Success(patterns, "Staff behavior patterns retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get staff behavior patterns.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Sync behavioral data to graph database
        /// </summary>
        [HttpPost("sync-to-graph")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SyncToGraph([FromQuery] int? guestId = null, [FromQuery] int? staffId = null)
        {
            try
            {
                await _behavioralTrackingService.SyncBehavioralDataToGraphAsync(guestId, staffId);
                return Success(true, "Behavioral data synced to graph successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to sync to graph.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Predict guest behavior
        /// </summary>
        [HttpPost("guests/{guestId}/predict-behavior")]
        [ProducesResponseType(typeof(ApiResponse<BehaviorPredictionResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PredictGuestBehavior(
            int guestId,
            [FromBody] PredictBehaviorRequest request)
        {
            try
            {
                var result = await _predictiveIntelligenceService.PredictGuestBehaviorAsync(
                    guestId, request.BehaviorType, request.TargetDate);
                return Success(result, "Behavior prediction completed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to predict behavior.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Predict service demand
        /// </summary>
        [HttpGet("services/{serviceType}/predict-demand")]
        [ProducesResponseType(typeof(ApiResponse<ServiceDemandPrediction>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PredictServiceDemand(
            string serviceType,
            [FromQuery] DateTime targetDate)
        {
            try
            {
                var result = await _predictiveIntelligenceService.PredictServiceDemandAsync(serviceType, targetDate);
                return Success(result, "Service demand prediction completed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to predict service demand.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Predict guest satisfaction
        /// </summary>
        [HttpGet("guests/{guestId}/predict-satisfaction")]
        [ProducesResponseType(typeof(ApiResponse<SatisfactionPrediction>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PredictGuestSatisfaction(
            int guestId,
            [FromQuery] int? serviceId = null,
            [FromQuery] string? serviceType = null)
        {
            try
            {
                var result = await _predictiveIntelligenceService.PredictGuestSatisfactionAsync(guestId, serviceId, serviceType);
                return Success(result, "Satisfaction prediction completed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to predict satisfaction.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Predict risks for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/predict-risks")]
        [ProducesResponseType(typeof(ApiResponse<RiskPredictionResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PredictRisks(int guestId)
        {
            try
            {
                var result = await _predictiveIntelligenceService.PredictRisksAsync(guestId);
                return Success(result, "Risk prediction completed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to predict risks.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Detect opportunities for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/detect-opportunities")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<OpportunityDetection>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DetectOpportunities(int guestId)
        {
            try
            {
                var result = await _predictiveIntelligenceService.DetectOpportunitiesAsync(guestId);
                return Success(result, "Opportunities detected successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to detect opportunities.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Predict guest spending
        /// </summary>
        [HttpGet("guests/{guestId}/predict-spending")]
        [ProducesResponseType(typeof(ApiResponse<SpendingPrediction>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PredictGuestSpending(int guestId, [FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _predictiveIntelligenceService.PredictGuestSpendingAsync(guestId, targetDate);
                return Success(result, "Spending prediction completed successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to predict spending.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get proactive recommendations for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/proactive-recommendations")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<ProactiveRecommendation>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProactiveRecommendations(int guestId, [FromQuery] DateTime? targetDate = null)
        {
            try
            {
                var result = await _proactiveIntelligenceService.GetProactiveRecommendationsAsync(guestId, targetDate);
                return Success(result, "Proactive recommendations retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get proactive recommendations.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get problem prevention alerts
        /// </summary>
        [HttpGet("problem-prevention-alerts")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<ProblemPreventionAlert>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProblemPreventionAlerts([FromQuery] int? guestId = null)
        {
            try
            {
                var result = await _proactiveIntelligenceService.GetProblemPreventionAlertsAsync(guestId);
                return Success(result, "Problem prevention alerts retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get problem prevention alerts.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get personalization suggestions for a guest
        /// </summary>
        [HttpGet("guests/{guestId}/personalization-suggestions")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<PersonalizationSuggestion>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPersonalizationSuggestions(int guestId)
        {
            try
            {
                var result = await _proactiveIntelligenceService.GetPersonalizationSuggestionsAsync(guestId);
                return Success(result, "Personalization suggestions retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get personalization suggestions.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get early warning signals
        /// </summary>
        [HttpGet("early-warning-signals")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<EarlyWarningSignal>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEarlyWarningSignals([FromQuery] int? guestId = null)
        {
            try
            {
                var result = await _proactiveIntelligenceService.GetEarlyWarningSignalsAsync(guestId);
                return Success(result, "Early warning signals retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get early warning signals.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get automatic action recommendations
        /// </summary>
        [HttpGet("guests/{guestId}/automatic-actions")]
        [ProducesResponseType(typeof(ApiResponse<System.Collections.Generic.List<AutomaticAction>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAutomaticActions(int guestId)
        {
            try
            {
                var result = await _proactiveIntelligenceService.GetAutomaticActionRecommendationsAsync(guestId);
                return Success(result, "Automatic action recommendations retrieved successfully.");
            }
            catch (Exception ex)
            {
                return Error("Failed to get automatic actions.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Analyze sentiment request
    /// </summary>
    public class AnalyzeSentimentRequest
    {
        public string Text { get; set; } = string.Empty;
        public string? Language { get; set; }
    }

    /// <summary>
    /// Predict behavior request
    /// </summary>
    public class PredictBehaviorRequest
    {
        public string BehaviorType { get; set; } = string.Empty;
        public DateTime? TargetDate { get; set; }
    }
}
