using GuestFlow.Application.Operations.Notification.Dtos;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Notification
{
    /// <summary>
    /// Interface for SignalR notification hub service
    /// </summary>
    public interface INotificationHubService
    {
        /// <summary>
        /// Send notification to specific user
        /// </summary>
        Task SendNotificationToUserAsync(int userId, NotificationDto notification);

        /// <summary>
        /// Send notification to all users
        /// </summary>
        Task SendNotificationToAllAsync(NotificationDto notification);

        /// <summary>
        /// Send notification to specific group
        /// </summary>
        Task SendNotificationToGroupAsync(string groupName, NotificationDto notification);

        /// <summary>
        /// Send live update to all users
        /// </summary>
        Task SendLiveUpdateAsync(string entityType, int entityId, string action, object? data = null);

        /// <summary>
        /// Send dashboard update to all users
        /// </summary>
        Task SendDashboardUpdateAsync(object updateData);

        /// <summary>
        /// Send daily operations update to all users
        /// </summary>
        Task SendDailyOperationsUpdateAsync(string action, object? data = null);
    }
}

