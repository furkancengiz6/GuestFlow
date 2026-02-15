using GuestFlow.Domain.Entities.Operations;
using System;

namespace GuestFlow.Domain.Events
{
    public class GuestReviewAddedEvent : IDomainEvent
    {
        public GuestReview Review { get; }
        public DateTime OccurredOn { get; }

        public GuestReviewAddedEvent(GuestReview review)
        {
            Review = review;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
