using System.Collections.Generic;

namespace GuestFlow.Application.Models.AI
{
    public class AIChatRequest
    {
        public string Message { get; set; } = string.Empty;
        public int? GuestId { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
        public string? Context { get; set; }
    }

    public class AIChatResponse
    {
        public string Response { get; set; } = string.Empty;
        public List<AIAction>? SuggestedActions { get; set; }
        public int ConfidenceScoreLevel { get; set; } // 1-10 scale
        public float ConfidenceScore { get; set; } // 0.0-1.0 scale
    }

    public class AIAction
    {
        public string ActionType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
