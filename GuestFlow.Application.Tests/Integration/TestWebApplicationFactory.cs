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

    static TestWebApplicationFactory()
    {
        // One-time static initialization of the bridge to avoid race conditions
        // We use an ephemeral provider here that doesn't need DI container build
        var services = new ServiceCollection();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        var sp = services.BuildServiceProvider();
        var dp = sp.GetRequiredService<IDataProtectionProvider>();
        
        // Wrap it in our domain interface
        var domainDp = new EphemeralDataProtection(dp);
        GuestFlow.Domain.DataProtection.DataProtectionBridge.Initialize(domainDp);
    }

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

            // Replace DbContext with SQLite in-memory provider
            services.RemoveAll<DbContextOptions<GuestFlowDbContext>>();
            services.RemoveAll<GuestFlowDbContext>(); // Ensure context itself is removed if registered directly

            // Create a singleton connection for the lifetime of the factory/test run
            // Note: In a real scenario, we might want one connection per test via IClassFixture,
            // but for WebApplicationFactory, the services are configured once. 
            // We use a singleton connection to keep the in-memory DB alive across requests.
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();

            services.AddDbContext<GuestFlowDbContext>(options =>
            {
                options.UseSqlite(connection);
            });

            // Register the connection to be disposed when the provider is disposed (if needed)
            // or just let the OS reclaim it. For robust tests, we can register a cleanup service.

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

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        return host;
    }

    // Helper class to adapt IDataProtectionProvider to IDataProtection
    private class EphemeralDataProtection : GuestFlow.Domain.DataProtection.IDataProtection
    {
        private readonly IDataProtectionProvider _provider;

        public EphemeralDataProtection(IDataProtectionProvider provider)
        {
            _provider = provider;
        }

        public string? Protect(string? value)
        {
            if (value == null) return null;
            return _provider.CreateProtector("Test").Protect(value);
        }

        public string? Unprotect(string? value)
        {
            if (value == null) return null;
            return _provider.CreateProtector("Test").Unprotect(value);
        }
    }
}

