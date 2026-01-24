// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Proactive
{
    /// <summary>
    /// Proactive Intelligence Service - Proaktif zeka servisi
    /// </summary>
    public interface IProactiveIntelligenceService
    {
        /// <summary>
        /// Get proactive service recommendations (proaktif hizmet önerileri)
        /// </summary>
        Task<List<ProactiveRecommendation>> GetProactiveRecommendationsAsync(int guestId, DateTime? targetDate = null);

        /// <summary>
        /// Get proactive problem prevention alerts (proaktif problem önleme uyarıları)
        /// </summary>
        Task<List<ProblemPreventionAlert>> GetProblemPreventionAlertsAsync(int? guestId = null);

        /// <summary>
        /// Get proactive personalization suggestions (proaktif kişiselleştirme önerileri)
        /// </summary>
        Task<List<PersonalizationSuggestion>> GetPersonalizationSuggestionsAsync(int guestId);

        /// <summary>
        /// Get early warning signals (erken uyarı sinyalleri)
        /// </summary>
        Task<List<EarlyWarningSignal>> GetEarlyWarningSignalsAsync(int? guestId = null);

        /// <summary>
        /// Get automatic action recommendations (otomatik aksiyon önerileri)
        /// </summary>
        Task<List<AutomaticAction>> GetAutomaticActionRecommendationsAsync(int guestId);
    }

    /// <summary>
    /// Proactive recommendation
    /// </summary>
    public class ProactiveRecommendation
    {
        public int GuestId { get; set; }
        public string RecommendationType { get; set; } = string.Empty; // Service, Time, Emotion, Relationship
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Priority { get; set; } // 0.0 to 1.0
        public string? RecommendedAction { get; set; }
        public DateTime? RecommendedDate { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Problem prevention alert
    /// </summary>
    public class ProblemPreventionAlert
    {
        public int? GuestId { get; set; }
        public string AlertType { get; set; } = string.Empty; // Dissatisfaction, Cancellation, Problem, Risk
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? RecommendedIntervention { get; set; }
        public DateTime? AlertDate { get; set; }
        public Dictionary<string, object>? RiskFactors { get; set; }
    }

    /// <summary>
    /// Personalization suggestion
    /// </summary>
    public class PersonalizationSuggestion
    {
        public int GuestId { get; set; }
        public string SuggestionType { get; set; } = string.Empty; // Preference, Service, Communication, Experience
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string? SuggestedAction { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Early warning signal
    /// </summary>
    public class EarlyWarningSignal
    {
        public int? GuestId { get; set; }
        public string SignalType { get; set; } = string.Empty; // Sentiment, Behavior, Satisfaction, Engagement
        public string Severity { get; set; } = "Low";
        public string Message { get; set; } = string.Empty;
        public DateTime DetectedAt { get; set; }
        public Dictionary<string, object>? Indicators { get; set; }
    }

    /// <summary>
    /// Automatic action recommendation
    /// </summary>
    public class AutomaticAction
    {
        public int GuestId { get; set; }
        public string ActionType { get; set; } = string.Empty; // Message, Service, Upgrade, Discount
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool CanExecuteAutomatically { get; set; }
        public string? ExecutionDetails { get; set; }
        public double Confidence { get; set; }
    }
}
