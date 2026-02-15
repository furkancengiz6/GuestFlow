using System.Threading.Tasks;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public interface IGraphAuditService
    {
        Task LogOperationAsync(string operation, object? parameters = null);
    }
}
