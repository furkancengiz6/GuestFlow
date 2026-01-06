using GuestFlow.Application.Types;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Notification
{
    /// <summary>
    /// Otomatik bildirim servisi interface'i
    /// </summary>
    public interface IAutomaticNotificationService
    {
        /// <summary>
        /// Transfer oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        Task<ServiceMessage> NotifyTransferCreatedAsync(int transferId);

        /// <summary>
        /// Transfer durumu değiştiğinde otomatik bildirim gönderir
        /// </summary>
        Task<ServiceMessage> NotifyTransferStatusChangedAsync(int transferId, string oldStatus, string newStatus);

        /// <summary>
        /// İtinerary oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        Task<ServiceMessage> NotifyItineraryCreatedAsync(int itineraryId);

        /// <summary>
        /// Restoran rezervasyonu oluşturulduğunda otomatik bildirim gönderir
        /// </summary>
        Task<ServiceMessage> NotifyRestaurantReservationCreatedAsync(int reservationId);

        /// <summary>
        /// Yaklaşan aktiviteler için hatırlatma bildirimi gönderir
        /// </summary>
        Task<ServiceMessage> SendUpcomingActivityRemindersAsync();
    }
}

