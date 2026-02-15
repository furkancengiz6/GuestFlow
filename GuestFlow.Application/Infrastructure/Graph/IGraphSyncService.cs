using System.Threading;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public interface IGraphSyncService
    {
        Task SyncToNeo4jAsync(OutboxEvent outboxEvent, CancellationToken ct);
        Task TriggerGraphIntelligenceAsync(CancellationToken ct);
        Task ForgetGuestFromGraphAsync(int guestId, CancellationToken ct);
    }
}
