using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GuestFlow.Api.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications and live updates
    /// </summary>
    [Authorize]
    public class NotificationsHub : Hub
    {
        private readonly ILogger<NotificationsHub> _logger;

        public NotificationsHub(ILogger<NotificationsHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation($"Client connected: {connectionId}, User: {userId}");

            // Add user to their personal group
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
            }

            // Add to all users group
            await Groups.AddToGroupAsync(connectionId, "all_users");

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation($"Client disconnected: {connectionId}, User: {userId}");

            if (exception != null)
            {
                _logger.LogError(exception, $"Client disconnected with error: {exception.Message}");
            }

            // Remove user from their personal group
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
            }

            // Remove from all users group
            await Groups.RemoveFromGroupAsync(connectionId, "all_users");

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Join a specific group
        /// </summary>
        public async Task JoinGroup(string groupName)
        {
            var connectionId = Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogInformation($"Client {connectionId} joined group: {groupName}");
        }

        /// <summary>
        /// Leave a specific group
        /// </summary>
        public async Task LeaveGroup(string groupName)
        {
            var connectionId = Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogInformation($"Client {connectionId} left group: {groupName}");
        }

        /// <summary>
        /// Get current connection ID (for testing)
        /// </summary>
        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }
    }
}

