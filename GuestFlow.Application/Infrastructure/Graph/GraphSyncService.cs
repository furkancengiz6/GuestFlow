using System;
using System.Threading;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Core;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using GuestFlow.Application.Operations.Intelligence.Graph;

namespace GuestFlow.Application.Infrastructure.Graph
{
    public class GraphSyncService : IGraphSyncService
    {
        private readonly GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService _neo4jService;
        private readonly IGraphIntelligenceService _intelligenceService;
        private readonly IGraphAuditService _auditService;
        private readonly ILogger<GraphSyncService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;

        public GraphSyncService(GuestFlow.Application.Operations.Intelligence.Graph.INeo4jService neo4jService, IGraphIntelligenceService intelligenceService, IGraphAuditService auditService, ILogger<GraphSyncService> logger)
        {
            _neo4jService = neo4jService;
            _intelligenceService = intelligenceService;
            _auditService = auditService;
            _logger = logger;

            // 3 retries with exponential backoff
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(ex, "Neo4j sync failed. Retry {RetryCount}/3 after {Delay}ms", retryCount, timeSpan.TotalMilliseconds);
                    });

            // Break after 5 consecutive failures for 30 seconds
            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30),
                    onBreak: (ex, duration) => _logger.LogError(ex, "Circuit Breaker OPEN for Neo4j. Duration: {Duration}s", duration.TotalSeconds),
                    onReset: () => _logger.LogInformation("Circuit Breaker CLOSED. Neo4j connectivity restored."),
                    onHalfOpen: () => _logger.LogInformation("Circuit Breaker HALF-OPEN. Testing Neo4j connectivity..."));
        }

        public async Task SyncToNeo4jAsync(OutboxEvent outboxEvent, CancellationToken ct)
        {
            await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
            {
                // In a real scenario, you would parse the Content and call specific Neo4j methods
                // For demonstration, we'll log the action. 
                // You might use _neo4jService to execute Cypher queries.
                
                _logger.LogInformation("Processing OutboxEvent {Id}: {EventType}", outboxEvent.Id, outboxEvent.EventType);
                
                // Actual sync logic would go here:
                // var data = JsonSerializer.Deserialize<YourType>(outboxEvent.Content);
                // await _neo4jService.ExecuteQueryAsync("...", data);

                await Task.CompletedTask;
            });
        }
        public async Task TriggerGraphIntelligenceAsync(CancellationToken ct)
        {
            await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
            {
                _logger.LogInformation("Triggering Graph Intelligence Algorithms...");
                
                var connections = await _intelligenceService.DiscoverHiddenConnectionsAsync();
                _logger.LogInformation("Discovered {Count} hidden connections.", connections.Count);

                var risks = await _intelligenceService.PredictFrictionRisksAsync();
                _logger.LogInformation("Identified {Count} guests at risk.", risks.Count);

                var influencers = await _intelligenceService.MapGuestInfluenceAsync();
                _logger.LogInformation("Mapped {Count} influential guests.", influencers.Count);
            });
        }

        public async Task ForgetGuestFromGraphAsync(int guestId, CancellationToken ct)
        {
            await _auditService.LogOperationAsync("ForgetGuest", new { guestId });
            await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
            {
                _logger.LogInformation("GDPR: Processing Right to be Forgotten for GuestId={GuestId}", guestId);
                
                await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        // Clean sensitive related nodes that might contain personal data (phones, emails, specific preferences)
                        OPTIONAL MATCH (g)-[:HAS_IDENTIFIER|PREFERS]->(n)
                        WHERE labels(n) IN ['Identifier', 'SensitivePreference']
                        DETACH DELETE n, g
                        RETURN count(*) as deletedCount";

                    return await tx.RunAsync(query, new { guestId });
                });

                _logger.LogInformation("GDPR: GuestId={GuestId} and sensitive relations removed from Graph.", guestId);
            });
        }
    }
}
