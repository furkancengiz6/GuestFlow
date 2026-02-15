using GuestFlow.Domain.Entities.Core;
using System;

namespace GuestFlow.Domain.Events
{
    public class ReservationCreatedEvent : IDomainEvent
    {
        public ReservationEntity Reservation { get; }
        public DateTime OccurredOn { get; }

        public ReservationCreatedEvent(ReservationEntity reservation)
        {
            Reservation = reservation;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
