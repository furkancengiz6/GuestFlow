using GuestFlow.Application.Operations.Cache;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Sockets;

namespace GuestFlow.Api.HealthChecks
{
    /// <summary>
    /// Redis cache health check implementation
    /// </summary>
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly ICacheService _cache;

        public RedisHealthCheck(ICacheService cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Test cache with a simple set/get operation
                var testKey = $"healthcheck:{Guid.NewGuid()}";
                var testValue = $"test-{DateTime.UtcNow.Ticks}";

                await _cache.SetAsync(testKey, testValue, TimeSpan.FromSeconds(30));
                var retrievedValue = await _cache.GetAsync<string>(testKey);

                // Clean up
                await _cache.RemoveAsync(testKey);

                if (retrievedValue == testValue)
                {
                    return HealthCheckResult.Healthy("Redis cache is healthy");
                }
                else
                {
                    return HealthCheckResult.Degraded("Redis cache set/get test failed");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Redis cache connection failed", ex);
            }
        }
    }
}