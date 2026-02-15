using GuestFlow.Persistence.Context;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GuestFlow.Application.Tests.Integration;

/// <summary>
/// Test host that swaps SQL Server for InMemory EF Core and disables background hosted services.
/// Keeps the real middleware pipeline so we can do true integration smoke tests.
/// </summary>
public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"GuestFlow_TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ensure versioned routes + swagger config match dev expectations
        builder.UseEnvironment(Environments.Development);

        // Ensure JWT secret exists so /auth/login can generate tokens (AuthController reads configuration directly)
        Environment.SetEnvironmentVariable("JWT__SecretKey", new string('x', 128));
        Environment.SetEnvironmentVariable("JWT__Issuer", "GuestFlowApp");
        Environment.SetEnvironmentVariable("JWT__Audience", "http://localhost");

        // Reduce noise/flakiness in integration tests
        Environment.SetEnvironmentVariable("RateLimitSettings__Enabled", "false");

        builder.ConfigureServices(services =>
        {
            // Use ephemeral data protection provider for deterministic tests
            services.AddDataProtection().UseEphemeralDataProtectionProvider();

            // Replace DbContext with in-memory provider
            services.RemoveAll<DbContextOptions<GuestFlowDbContext>>();
            services.AddDbContext<GuestFlowDbContext>(options =>
            {
                // Use a stable database name for the lifetime of the test host so multiple requests share state.
                options.UseInMemoryDatabase(_dbName);
                // InMemory provider doesn't support transactions; the app uses transactions in services.
                // Ignore the transaction warning so tests can exercise the real code paths.
                options.ConfigureWarnings(w =>
                    w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            // Disable background services for test determinism
            RemoveHostedServiceByImplementationType(services, "EmailQueueBackgroundService");
            RemoveHostedServiceByImplementationType(services, "RefreshTokenCleanupBackgroundService");
            RemoveHostedServiceByImplementationType(services, "ServiceConfirmationBackgroundService");
            RemoveHostedServiceByImplementationType(services, "PaymentReminderBackgroundService");
            RemoveHostedServiceByImplementationType(services, "DailyRevenueBackgroundService");
            
            // Add removal of other background services identified
            RemoveHostedServiceByImplementationType(services, "PMSPollingBackgroundService");
            RemoveHostedServiceByImplementationType(services, "SmartNotificationBackgroundService");
            RemoveHostedServiceByImplementationType(services, "NotificationRuleBackgroundService");
            RemoveHostedServiceByImplementationType(services, "OTAWebhookRetryBackgroundService");
            RemoveHostedServiceByImplementationType(services, "OutboxProcessor");
        });
    }

    private static void RemoveHostedServiceByImplementationType(IServiceCollection services, string implementationTypeName)
    {
        // AddHostedService<T> registers as IHostedService with ImplementationType = typeof(T)
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType?.Name == implementationTypeName)
            .ToList();

        foreach (var d in descriptors)
            services.Remove(d);
    }
}

