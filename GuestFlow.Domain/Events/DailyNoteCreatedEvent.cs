using GuestFlow.Domain.Entities.Core;
using System;

namespace GuestFlow.Domain.Events
{
    public class DailyNoteCreatedEvent : IDomainEvent
    {
        public DailyNoteEntity DailyNote { get; }
        public DateTime OccurredOn { get; }

        public DailyNoteCreatedEvent(DailyNoteEntity dailyNote)
        {
            DailyNote = dailyNote;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
