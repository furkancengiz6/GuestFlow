using GuestFlow.Domain.Entities.Core;
using System;

namespace GuestFlow.Domain.Events
{
    public class GuestCreatedEvent : IDomainEvent
    {
        public GuestEntity Guest { get; }
        public DateTime OccurredOn { get; }

        public GuestCreatedEvent(GuestEntity guest)
        {
            Guest = guest;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
