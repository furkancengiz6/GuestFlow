using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Events
{
    public interface IDomainEventDispatcher
    {
        Task DispatchEventsAsync(IEnumerable<IDomainEvent> events);
    }
}
