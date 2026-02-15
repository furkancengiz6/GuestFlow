using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuestFlow.Application.Infrastructure.Graph;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Api.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;

        public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Outbox Processor Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOutboxEvents(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing outbox events.");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }

            _logger.LogInformation("Outbox Processor Service is stopping.");
        }

        private async Task ProcessOutboxEvents(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var graphSyncService = scope.ServiceProvider.GetRequiredService<IGraphSyncService>();

            var pendingEvents = await dbContext.OutboxEvents
                .Where(e => e.ProcessedAtUtc == null)
                .OrderBy(e => e.CreatedAtUtc)
                .Take(20)
                .ToListAsync(ct);

            if (!pendingEvents.Any())
            {
                return;
            }

            _logger.LogInformation("Processing {Count} pending outbox events.", pendingEvents.Count);

            foreach (var @event in pendingEvents)
            {
                try
                {
                    await graphSyncService.SyncToNeo4jAsync(@event, ct);
                    @event.ProcessedAtUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    @event.Error = ex.Message;
                    _logger.LogError(ex, "Failed to process outbox event {Id}", @event.Id);
                }
            }

            await dbContext.SaveChangesAsync(ct);
        }
    }
}
