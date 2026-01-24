// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Predictive
{
    /// <summary>
    /// Predictive Intelligence Service - Tahminsel zeka servisi
    /// </summary>
    public interface IPredictiveIntelligenceService
    {
        /// <summary>
        /// Predict guest behavior (misafir davranışını tahmin et)
        /// </summary>
        Task<BehaviorPredictionResult> PredictGuestBehaviorAsync(int guestId, string behaviorType, DateTime? targetDate = null);

        /// <summary>
        /// Predict service demand (hizmet talebini tahmin et)
        /// </summary>
        Task<ServiceDemandPrediction> PredictServiceDemandAsync(string serviceType, DateTime targetDate);

        /// <summary>
        /// Predict guest satisfaction (misafir memnuniyetini tahmin et)
        /// </summary>
        Task<SatisfactionPrediction> PredictGuestSatisfactionAsync(int guestId, int? serviceId = null, string? serviceType = null);

        /// <summary>
        /// Predict risk factors (risk faktörlerini tahmin et)
        /// </summary>
        Task<RiskPredictionResult> PredictRisksAsync(int guestId);

        /// <summary>
        /// Detect opportunities (fırsatları tespit et)
        /// </summary>
        Task<List<OpportunityDetection>> DetectOpportunitiesAsync(int guestId);

        /// <summary>
        /// Predict guest spending (misafir harcamasını tahmin et)
        /// </summary>
        Task<SpendingPrediction> PredictGuestSpendingAsync(int guestId, DateTime? targetDate = null);
    }

    /// <summary>
    /// Behavior prediction result
    /// </summary>
    public class BehaviorPredictionResult
    {
        public int GuestId { get; set; }
        public string BehaviorType { get; set; } = string.Empty;
        public double Probability { get; set; } // 0.0 to 1.0
        public string Prediction { get; set; } = string.Empty;
        public double Confidence { get; set; } // 0.0 to 1.0
        public Dictionary<string, object>? Factors { get; set; }
    }

    /// <summary>
    /// Service demand prediction
    /// </summary>
    public class ServiceDemandPrediction
    {
        public string ServiceType { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public int PredictedDemand { get; set; }
        public double Confidence { get; set; }
        public Dictionary<string, object>? Factors { get; set; }
    }

    /// <summary>
    /// Satisfaction prediction
    /// </summary>
    public class SatisfactionPrediction
    {
        public int GuestId { get; set; }
        public double PredictedSatisfaction { get; set; } // 0-10
        public double Confidence { get; set; }
        public string RiskLevel { get; set; } = "Low"; // Low, Medium, High
        public Dictionary<string, object>? Factors { get; set; }
    }

    /// <summary>
    /// Risk prediction result
    /// </summary>
    public class RiskPredictionResult
    {
        public int GuestId { get; set; }
        public List<RiskFactor> Risks { get; set; } = new List<RiskFactor>();
        public double OverallRiskScore { get; set; } // 0.0 to 1.0
    }

    /// <summary>
    /// Risk factor
    /// </summary>
    public class RiskFactor
    {
        public string RiskType { get; set; } = string.Empty; // Dissatisfaction, Cancellation, Problem, Churn
        public double RiskScore { get; set; } // 0.0 to 1.0
        public string Severity { get; set; } = "Low"; // Low, Medium, High, Critical
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object>? Factors { get; set; }
    }

    /// <summary>
    /// Opportunity detection
    /// </summary>
    public class OpportunityDetection
    {
        public string OpportunityType { get; set; } = string.Empty; // Upsell, CrossSell, Personalization, Loyalty
        public string Description { get; set; } = string.Empty;
        public double OpportunityScore { get; set; } // 0.0 to 1.0
        public string? RecommendedAction { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Spending prediction
    /// </summary>
    public class SpendingPrediction
    {
        public int GuestId { get; set; }
        public decimal PredictedSpending { get; set; }
        public string Currency { get; set; } = "TRY";
        public double Confidence { get; set; }
        public Dictionary<string, decimal>? SpendingByCategory { get; set; }
    }
}
