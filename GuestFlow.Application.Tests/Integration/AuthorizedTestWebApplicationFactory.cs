using GuestFlow.Persistence.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GuestFlow.Application.Tests.Integration;

/// <summary>
/// Integration test host with:
/// - InMemory EF Core database
/// - Background hosted services disabled
/// - Authentication replaced with a deterministic Admin principal (TestAuthHandler)
/// </summary>
public sealed class AuthorizedTestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"GuestFlow_TestDb_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        // Ensure JWT config doesn't break startup (even though we bypass auth for most tests).
        Environment.SetEnvironmentVariable("JWT__SecretKey", new string('x', 128));
        Environment.SetEnvironmentVariable("JWT__Issuer", "GuestFlowApp");
        Environment.SetEnvironmentVariable("JWT__Audience", "http://localhost");

        // Reduce noise/flakiness in integration tests
        Environment.SetEnvironmentVariable("RateLimitSettings__Enabled", "false");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory provider
            services.RemoveAll<DbContextOptions<GuestFlowDbContext>>();
            services.AddDbContext<GuestFlowDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.ConfigureWarnings(w =>
                    w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            // Replace auth with deterministic test scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // Disable background services for test determinism
            RemoveHostedServiceByImplementationType(services, "EmailQueueBackgroundService");
            RemoveHostedServiceByImplementationType(services, "RefreshTokenCleanupBackgroundService");
            RemoveHostedServiceByImplementationType(services, "ServiceConfirmationBackgroundService");
            RemoveHostedServiceByImplementationType(services, "PaymentReminderBackgroundService");
            RemoveHostedServiceByImplementationType(services, "DailyRevenueBackgroundService");
            RemoveHostedServiceByImplementationType(services, "PMSPollingBackgroundService");
            RemoveHostedServiceByImplementationType(services, "SmartNotificationBackgroundService");
            RemoveHostedServiceByImplementationType(services, "NotificationRuleBackgroundService");
            RemoveHostedServiceByImplementationType(services, "OTAWebhookRetryBackgroundService");

            // Mock Neo4j to prevent deadlocks from open connections
            services.RemoveAll<GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService>();
            services.AddSingleton<GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService, Neo4jNoOpStub>();
        });
    }

    // Stub to prevent real Neo4j connections
    private class Neo4jNoOpStub : GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService
    {
        public Neo4j.Driver.IDriver Driver => null!;
        public Task<T?> ExecuteReadAsync<T>(Func<Neo4j.Driver.IAsyncQueryRunner, Task<T>> work) => Task.FromResult<T?>(default);
        public Task<T?> ExecuteWriteAsync<T>(Func<Neo4j.Driver.IAsyncQueryRunner, Task<T>> work) => Task.FromResult<T?>(default);
        public Task<bool> TestConnectionAsync() => Task.FromResult(true);
        public void Dispose() { }
    }

    private static void RemoveHostedServiceByImplementationType(IServiceCollection services, string implementationTypeName)
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType?.Name == implementationTypeName)
            .ToList();

        foreach (var d in descriptors)
            services.Remove(d);
    }
}

