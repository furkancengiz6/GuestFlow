using GuestFlow.Domain.Events;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Reservation.Handlers
{
    public class ReservationNotificationHandler : IDomainEventHandler<ReservationCreatedEvent>
    {
        private readonly ILogger<ReservationNotificationHandler> _logger;

        public ReservationNotificationHandler(ILogger<ReservationNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task HandleAsync(ReservationCreatedEvent domainEvent)
        {
            _logger.LogInformation("Notifying staff about new reservation: {ReservationNumber} for Service: {ServiceType}", 
                domainEvent.Reservation.ReservationNumber, domainEvent.Reservation.ServiceType);

            // Logic to notify personnel via SignalR or Mobile Push would go here
            // e.g., _notificationService.NotifyPersonnelAsync(domainEvent.Reservation.PersonnelId, "New Reservation Created");

            return Task.CompletedTask;
        }
    }
}
