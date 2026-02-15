using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Persistence.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public class GraphAuditService : IGraphAuditService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GraphAuditService(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogOperationAsync(string operation, object? parameters = null)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();

            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = httpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

            var log = new GraphAuditLog
            {
                Operation = operation,
                UserId = userId,
                UserName = userName,
                QueryParameters = parameters != null ? JsonSerializer.Serialize(parameters) : null,
                IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
                UserAgent = httpContext?.Request?.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            dbContext.GraphAuditLogs.Add(log);
            await dbContext.SaveChangesAsync();
        }
    }
}
