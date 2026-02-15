using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Events
{
    /// <summary>
    /// No-op implementation of IDomainEventDispatcher for use in tests or design-time.
    /// </summary>
    public class NullDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchEventsAsync(IEnumerable<IDomainEvent> events)
        {
            return Task.CompletedTask;
        }
    }
}
