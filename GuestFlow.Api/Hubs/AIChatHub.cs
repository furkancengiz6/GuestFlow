using GuestFlow.Application.Models.AI;
using GuestFlow.Application.Operations.AI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace GuestFlow.Api.Hubs
{
    [Authorize] // Requires JWT token
    public class AIChatHub : Hub
    {
        private readonly IAIChatService _aiChatService;
        private readonly ILogger<AIChatHub> _logger;

        public AIChatHub(IAIChatService aiChatService, ILogger<AIChatHub> logger)
        {
            _aiChatService = aiChatService;
            _logger = logger;
        }

        public async Task SendMessage(AIChatRequest request)
        {
            try
            {
                _logger.LogInformation("Received message from {ConnectionId}", Context.ConnectionId);

                // Notify client that AI is processing
                await Clients.Caller.SendAsync("AIProcessing", true);

                // Process message via service
                var response = await _aiChatService.ProcessMessageAsync(request);

                // Send response back to caller
                await Clients.Caller.SendAsync("ReceiveAIResponse", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AIChatHub.SendMessage");
                await Clients.Caller.SendAsync("ReceiveAIResponse", new AIChatResponse 
                { 
                    Response = "I encountered an error while processing your message. Please try again later.",
                    ConfidenceScoreLevel = 0
                });
            }
            finally
            {
                // Notify client that processing is finished
                await Clients.Caller.SendAsync("AIProcessing", false);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User connected to AI Chat Hub: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("User disconnected from AI Chat Hub: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
