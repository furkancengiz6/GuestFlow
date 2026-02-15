using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Graph.Dtos;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using GuestFlow.Application.Infrastructure.Graph;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    public class GraphIntelligenceService : IGraphIntelligenceService
    {
        private readonly INeo4jService _neo4jService;
        private readonly IGraphAuditService _auditService;
        private readonly ILogger<GraphIntelligenceService> _logger;

        public GraphIntelligenceService(INeo4jService neo4jService, IGraphAuditService auditService, ILogger<GraphIntelligenceService> logger)
        {
            _neo4jService = neo4jService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<List<HiddenConnectionDto>> DiscoverHiddenConnectionsAsync()
        {
            await _auditService.LogOperationAsync("DiscoverHiddenConnections");
            try
            {
                return await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        // Discover by Email Domain
                        MATCH (g1:Guest), (g2:Guest)
                        WHERE id(g1) < id(g2)
                          AND g1.email CONTAINS '@' AND g2.email CONTAINS '@'
                          AND split(g1.email, '@')[1] = split(g2.email, '@')[1]
                          AND NOT split(g1.email, '@')[1] IN ['gmail.com', 'outlook.com', 'hotmail.com', 'yahoo.com', 'icloud.com']
                        MERGE (g1)-[r:IS_CONNECTED_TO {reason: 'Corporate Email Domain'}]-(g2)
                        SET r.detail = split(g1.email, '@')[1]
                        
                        UNION
                        
                        // Discover by Phone
                        MATCH (g1:Guest), (g2:Guest)
                        WHERE id(g1) < id(g2) AND g1.phone = g2.phone AND g1.phone IS NOT NULL
                        MERGE (g1)-[r:IS_CONNECTED_TO {reason: 'Shared Phone Number'}]-(g2)
                        SET r.detail = g1.phone
                        
                        UNION
                        
                        // Discover by Shared VIP Tour
                        MATCH (g1:Guest)-[:USES]->(s:Service {type: 'Special VIP Tour'})<-[:USES]-(g2:Guest)
                        WHERE id(g1) < id(g2)
                        MERGE (g1)-[r:IS_CONNECTED_TO {reason: 'Shared Special VIP Tour'}]-(g2)
                        SET r.detail = s.name
                        
                        RETURN g1.id as g1Id, g2.id as g2Id, r.reason as reason, r.detail as detail";

                    var result = await tx.RunAsync(query);
                    var connections = new List<HiddenConnectionDto>();

                    await result.ForEachAsync(record =>
                    {
                        connections.Add(new HiddenConnectionDto
                        {
                            Guest1Id = record["g1Id"].As<int>(),
                            Guest2Id = record["g2Id"].As<int>(),
                            Reason = record["reason"].As<string>(),
                            Detail = record["detail"].As<string>()
                        });
                    });

                    return connections;
                }) ?? new List<HiddenConnectionDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DiscoverHiddenConnectionsAsync");
                return new List<HiddenConnectionDto>();
            }
        }

        public async Task<List<FrictionRiskDto>> PredictFrictionRisksAsync()
        {
            await _auditService.LogOperationAsync("PredictFrictionRisks");
            try
            {
                return await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest)-[r:EXPERIENCED|FEELS]->(n)
                        WHERE (n:TransferDelay OR n:NegativeSentiment OR n:RestrictedAccess)
                          AND r.timestamp >= datetime() - duration('P2D')
                        WITH g, count(n) as frictionCount
                        WHERE frictionCount >= 3
                        SET g.status = 'AtRisk', 
                            g.frictionLevel = frictionCount, 
                            g.riskLastUpdated = datetime()
                        RETURN g.id as guestId, frictionCount";

                    var result = await tx.RunAsync(query);
                    var risks = new List<FrictionRiskDto>();

                    await result.ForEachAsync(record =>
                    {
                        risks.Add(new FrictionRiskDto
                        {
                            GuestId = record["guestId"].As<int>(),
                            FrictionCount = record["frictionCount"].As<int>()
                        });
                    });

                    return risks;
                }) ?? new List<FrictionRiskDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PredictFrictionRisksAsync");
                return new List<FrictionRiskDto>();
            }
        }

        public async Task<List<InfluenceDto>> MapGuestInfluenceAsync()
        {
            await _auditService.LogOperationAsync("MapGuestInfluence");
            try
            {
                return await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest)
                        OPTIONAL MATCH (g)-[r:INTERACTS|USES]->()
                        WITH g, count(r) as interactionCount
                        OPTIONAL MATCH (g)-[:IS_CONNECTED_TO]-(related)
                        WITH g, interactionCount, count(related) as networkSize
                        SET g.influenceScore = (interactionCount * 0.6) + (networkSize * 0.4)
                        RETURN g.id as guestId, g.name as name, g.influenceScore as score
                        ORDER BY g.influenceScore DESC
                        LIMIT 10";

                    var result = await tx.RunAsync(query);
                    var influencers = new List<InfluenceDto>();

                    await result.ForEachAsync(record =>
                    {
                        influencers.Add(new InfluenceDto
                        {
                            GuestId = record["guestId"].As<int>(),
                            Name = record["name"].As<string>(),
                            InfluenceScore = record["score"].As<double>()
                        });
                    });

                    return influencers;
                }) ?? new List<InfluenceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MapGuestInfluenceAsync");
                return new List<InfluenceDto>();
            }
        }
    }
}
