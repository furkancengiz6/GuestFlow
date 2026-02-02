using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Models.AI;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.AI
{
    public interface IAIChatService
    {
        Task<AIChatResponse> ProcessMessageAsync(AIChatRequest request);
    }

    public class AIChatService : IAIChatService
    {
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(ILogger<AIChatService> logger)
        {
            _logger = logger;
        }

        public async Task<AIChatResponse> ProcessMessageAsync(AIChatRequest request)
        {
            _logger.LogInformation("Processing AI Request: {Message}", request.Message);

            // Simulate AI processing delay
            await Task.Delay(500);

            var message = request.Message.ToLowerInvariant();
            var response = new AIChatResponse
            {
                ConfidenceScoreLevel = 9,
                SuggestedActions = new List<AIAction>()
            };

            // Simple Rule-Based Logic for Phase 1
            if (message.Contains("wifi") || message.Contains("password") || message.Contains("internet"))
            {
                response.Response = "The Wi-Fi network is 'GuestFlow_Secure'. The password is 'Welcome2025'.";
                response.SuggestedActions.Add(new AIAction 
                { 
                    ActionType = "CopyText", 
                    Description = "Copy Password",
                    Parameters = new Dictionary<string, object> { { "text", "Welcome2025" } }
                });
            }
            else if (message.Contains("breakfast") || message.Contains("food") || message.Contains("eat"))
            {
                response.Response = "Breakfast is served daily from 07:00 AM to 10:30 AM in the Main Restaurant.";
                response.SuggestedActions.Add(new AIAction 
                { 
                    ActionType = "Navigate", 
                    Description = "View Menu",
                    Parameters = new Dictionary<string, object> { { "screen", "MenuScreen" } }
                });
            }
            else if (message.Contains("checkout") || message.Contains("check out"))
            {
                response.Response = "Standard checkout time is 12:00 PM. Would you like to request a late checkout?";
                response.SuggestedActions.Add(new AIAction 
                { 
                    ActionType = "ServiceRequest", 
                    Description = "Request Late Checkout",
                    Parameters = new Dictionary<string, object> { { "serviceType", "LateCheckout" } }
                });
            }
            else
            {
                response.Response = "I'm sorry, I'm still learning. Could you please rephrase your request? I can help with Wi-Fi, breakfast hours, or checkout.";
                response.ConfidenceScoreLevel = 3;
            }

            return response;
        }
    }
}
