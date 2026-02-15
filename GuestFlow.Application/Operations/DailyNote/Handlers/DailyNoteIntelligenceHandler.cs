using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Domain.Events;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.DailyNote.Handlers
{
    public class DailyNoteIntelligenceHandler : IDomainEventHandler<DailyNoteCreatedEvent>
    {
        private readonly IAIAssistantService _aiAssistantService;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DailyNoteIntelligenceHandler> _logger;

        public DailyNoteIntelligenceHandler(
            IAIAssistantService aiAssistantService,
            IBehavioralTrackingService behavioralTrackingService,
            IUnitOfWork unitOfWork,
            ILogger<DailyNoteIntelligenceHandler> logger)
        {
            _aiAssistantService = aiAssistantService;
            _behavioralTrackingService = behavioralTrackingService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task HandleAsync(DailyNoteCreatedEvent domainEvent)
        {
            _logger.LogInformation("Analyzing daily note for intelligence: NoteId={NoteId}", domainEvent.DailyNote.Id);

            try
            {
                // Identify the guest associated with the room number if possible via RoomAssignments
                var noteDate = domainEvent.DailyNote.NoteDate;
                var roomNumberStr = domainEvent.DailyNote.RoomNumber.ToString();
                
                var roomAssignment = await _unitOfWork.RoomAssignments.GetAsync(ra => 
                    ra.RoomNumber == roomNumberStr && 
                    ra.StartDate <= noteDate && 
                    (ra.EndDate == null || ra.EndDate >= noteDate) &&
                    !ra.IsDeleted);

                if (roomAssignment == null)
                {
                    _logger.LogWarning("No active room assignment found for room {RoomNumber} on {Date}. Skipping AI analysis.", 
                        domainEvent.DailyNote.RoomNumber, domainEvent.DailyNote.NoteDate);
                    return;
                }

                var guestId = roomAssignment.GuestId;

                var prompt = $@"Analyze the following staff note for guest insights. 
                Note: ""{domainEvent.DailyNote.NoteText}""
                
                Identify:
                1. Guest Preferences (likes, dislikes, special requests).
                2. Sentiment (Positive, Neutral, Negative).
                3. Satisfaction Score (1-10 scale).
                4. Actionable categories (e.g., Housekeeping, Dining, Service).

                Return a JSON object:
                {{
                    ""insights"": [
                        {{
                            ""type"": ""Preference"",
                            ""category"": ""Dining"",
                            ""value"": ""Prefers low-sodium meals"",
                            ""sentiment"": 0.5,
                            ""satisfaction"": 8.0
                        }}
                    ],
                    ""overallSatisfaction"": 8.5
                }}
                Response ONLY with the JSON block.";

                var aiResponse = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "NoteAnalysis" } }
                });

                if (string.IsNullOrEmpty(aiResponse?.Response)) return;

                var jsonStart = aiResponse.Response.IndexOf('{');
                var jsonEnd = aiResponse.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return;

                var json = aiResponse.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<NoteAnalysisResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result?.Insights != null)
                {
                    foreach (var insight in result.Insights)
                    {
                        await _behavioralTrackingService.TrackGuestBehaviorAsync(
                            guestId,
                            insight.Type,
                            insight.Category,
                            insight.Value,
                            insight.Sentiment,
                            insight.Satisfaction,
                            relatedEntityType: "DailyNote",
                            relatedEntityId: domainEvent.DailyNote.Id
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to analyze daily note for intelligence: NoteId={NoteId}", domainEvent.DailyNote.Id);
            }
        }

        private class NoteAnalysisResult
        {
            public List<NoteInsight> Insights { get; set; } = new();
            public double OverallSatisfaction { get; set; }
        }

        private class NoteInsight
        {
            public string Type { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public double? Sentiment { get; set; }
            public double? Satisfaction { get; set; }
        }
    }
}
