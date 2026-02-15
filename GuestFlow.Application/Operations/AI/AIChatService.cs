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
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<AIChatService> _logger;

        public AIChatService(IAIAssistantService aiAssistantService, ILogger<AIChatService> logger)
        {
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        public async Task<AIChatResponse> ProcessMessageAsync(AIChatRequest request)
        {
            _logger.LogInformation("Delegating AI Chat Request to Assistant Service: {Message}", request.Message);
            
            // Delegate to the specialized assistant service (which handles RAG, PII masking, and AI logic)
            return await _aiAssistantService.ProcessMessageAsync(request);
        }
    }
}
