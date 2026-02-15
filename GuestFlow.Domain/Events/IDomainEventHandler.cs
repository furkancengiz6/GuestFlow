using System.Threading.Tasks;

namespace GuestFlow.Domain.Events
{
    public interface IDomainEventHandler<in T> where T : IDomainEvent
    {
        Task HandleAsync(T domainEvent);
    }
}
