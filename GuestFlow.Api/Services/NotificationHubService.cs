using Microsoft.AspNetCore.SignalR;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Api.Hubs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Api.Services
{
    /// <summary>
    /// Service for sending notifications via SignalR Hub
    /// </summary>
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly ILogger<NotificationHubService> _logger;

        public NotificationHubService(
            IHubContext<NotificationsHub> hubContext,
            ILogger<NotificationHubService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Send notification to specific user
        /// </summary>
        public async Task SendNotificationToUserAsync(int userId, NotificationDto notification)
        {
            try
            {
                var groupName = $"user_{userId}";
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation($"Notification sent to user {userId} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to user {userId} via SignalR");
            }
        }

        /// <summary>
        /// Send notification to all users
        /// </summary>
        public async Task SendNotificationToAllAsync(NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to all users via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users via SignalR");
            }
        }

        /// <summary>
        /// Send notification to specific group
        /// </summary>
        public async Task SendNotificationToGroupAsync(string groupName, NotificationDto notification)
        {
            try
            {
                await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation($"Notification sent to group {groupName} via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending notification to group {groupName} via SignalR");
            }
        }

        /// <summary>
        /// Send live update to all users
        /// </summary>
        public async Task SendLiveUpdateAsync(string entityType, int entityId, string action, object? data = null)
        {
            try
            {
                var update = new
                {
                    EntityType = entityType,
                    EntityId = entityId,
                    Action = action,
                    Data = data,
                    Timestamp = DateTime.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("ReceiveLiveUpdate", update);
                _logger.LogInformation($"Live update sent: {entityType} {entityId} {action}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending live update via SignalR: {ex.Message}");
            }
        }

        /// <summary>
        /// Send dashboard update to all users
        /// </summary>
        public async Task SendDashboardUpdateAsync(object updateData)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveDashboardUpdate", updateData);
                _logger.LogInformation("Dashboard update sent via SignalR");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending dashboard update via SignalR: {ex.Message}");
            }
        }
    }
}

