using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Api.HealthChecks
{
    /// <summary>
    /// Database health check implementation
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IRepository<GuestFlow.Domain.Entities.Core.GuestEntity> _guestRepository;

        public DatabaseHealthCheck(IRepository<GuestFlow.Domain.Entities.Core.GuestEntity> guestRepository)
        {
            _guestRepository = guestRepository ?? throw new ArgumentNullException(nameof(guestRepository));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simple query to test database connectivity
                var connectionTest = await _guestRepository.GetAll()
                    .Take(1)
                    .CountAsync(cancellationToken);

                return HealthCheckResult.Healthy("Database connection is healthy");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database connection failed", ex);
            }
        }
    }
}