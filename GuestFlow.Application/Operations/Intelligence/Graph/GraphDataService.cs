// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Intelligence.Graph.Dtos;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    /// <summary>
    /// Graph data service implementation - Node ve Edge oluşturma/yönetme
    /// </summary>
    public class GraphDataService : IGraphDataService
    {
        private readonly INeo4jService _neo4jService;
        private readonly ILogger<GraphDataService> _logger;

        public GraphDataService(INeo4jService neo4jService, ILogger<GraphDataService> logger)
        {
            _neo4jService = neo4jService;
            _logger = logger;
        }

        public async Task<bool> CreateOrUpdateGuestNodeAsync(int guestId, string guestName, string? guestCode = null)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (g:Guest {id: $guestId})
                        SET g.name = $guestName,
                            g.guestCode = $guestCode,
                            g.updatedAt = datetime()
                        RETURN g";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId,
                        guestName,
                        guestCode = guestCode ?? string.Empty
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update guest node: GuestId={GuestId}", guestId);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateStaffNodeAsync(int staffId, string staffName)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (s:Staff {id: $staffId})
                        SET s.name = $staffName,
                            s.updatedAt = datetime()
                        RETURN s";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        staffId,
                        staffName
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update staff node: StaffId={StaffId}", staffId);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateServiceNodeAsync(int serviceId, string serviceType, string serviceName)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (s:Service {id: $serviceId})
                        SET s.type = $serviceType,
                            s.name = $serviceName,
                            s.updatedAt = datetime()
                        RETURN s";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        serviceId,
                        serviceType,
                        serviceName
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update service node: ServiceId={ServiceId}", serviceId);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateTimeNodeAsync(DateTime date, string? timeOfDay = null, string? season = null)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var dateKey = date.ToString("yyyy-MM-dd");
                    var query = @"
                        MERGE (t:Time {date: $dateKey})
                        SET t.timeOfDay = $timeOfDay,
                            t.season = $season,
                            t.updatedAt = datetime()
                        RETURN t";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        dateKey,
                        timeOfDay = timeOfDay ?? string.Empty,
                        season = season ?? GetSeason(date)
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update time node: Date={Date}", date);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateEmotionNodeAsync(string emotionType, double sentimentScore)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MERGE (e:Emotion {type: $emotionType})
                        SET e.sentimentScore = $sentimentScore,
                            e.updatedAt = datetime()
                        RETURN e";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        emotionType,
                        sentimentScore
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update emotion node: EmotionType={EmotionType}", emotionType);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateGuestStaffInteractionAsync(GuestStaffInteractionDto dto)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        MATCH (s:Staff {id: $staffId})
                        MERGE (g)-[r:INTERACTS]->(s)
                        SET r.weight = COALESCE(r.weight, 0) + $weightIncrement,
                            r.frequency = COALESCE(r.frequency, 0) + 1,
                            r.sentiment = $sentiment,
                            r.satisfaction = $satisfaction,
                            r.timestamp = datetime(),
                            r.context = $context,
                            r.interactionType = $interactionType
                        RETURN r";

                    var weightIncrement = 0.1; // Her etkileşimde ağırlık artışı
                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId = dto.GuestId,
                        staffId = dto.StaffId,
                        weightIncrement,
                        sentiment = dto.Sentiment,
                        satisfaction = dto.Satisfaction,
                        timestamp = dto.Timestamp,
                        context = dto.Context ?? "{}",
                        interactionType = dto.InteractionType
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update guest-staff interaction: GuestId={GuestId}, StaffId={StaffId}",
                    dto.GuestId, dto.StaffId);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateGuestPreferenceAsync(GuestPreferenceDto dto)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    // Preference için özel node oluştur
                    var preferenceNodeQuery = @"
                        MERGE (p:Preference {type: $preferenceType, value: $preferenceValue})
                        RETURN p";

                    await tx.RunAsync(preferenceNodeQuery, new
                    {
                        preferenceType = dto.PreferenceType,
                        preferenceValue = dto.PreferenceValue
                    });

                    // Guest-Preference ilişkisi
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        MATCH (p:Preference {type: $preferenceType, value: $preferenceValue})
                        MERGE (g)-[r:PREFERS]->(p)
                        SET r.weight = COALESCE(r.weight, 0) + $weightIncrement,
                            r.frequency = COALESCE(r.frequency, 0) + 1,
                            r.sentiment = $sentiment,
                            r.satisfaction = $satisfaction,
                            r.timestamp = datetime(),
                            r.context = $context
                        RETURN r";

                    var weightIncrement = 0.1;
                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId = dto.GuestId,
                        preferenceType = dto.PreferenceType,
                        preferenceValue = dto.PreferenceValue,
                        weightIncrement,
                        sentiment = dto.Sentiment,
                        satisfaction = dto.Satisfaction,
                        timestamp = dto.Timestamp,
                        context = dto.Context ?? "{}"
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update guest preference: GuestId={GuestId}", dto.GuestId);
                return false;
            }
        }

        public async Task<bool> CreateOrUpdateServiceSatisfactionAsync(ServiceSatisfactionDto dto)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        MATCH (s:Service {id: $serviceId})
                        MERGE (g)-[r:SATISFIES]->(s)
                        SET r.weight = COALESCE(r.weight, 0) + $weightIncrement,
                            r.frequency = COALESCE(r.frequency, 0) + 1,
                            r.sentiment = $sentiment,
                            r.satisfaction = $satisfaction,
                            r.timestamp = datetime(),
                            r.context = $context,
                            r.serviceType = $serviceType
                        RETURN r";

                    var weightIncrement = 0.15; // Memnuniyet ilişkileri daha ağırlıklı
                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId = dto.GuestId,
                        serviceId = dto.ServiceId,
                        serviceType = dto.ServiceType,
                        weightIncrement,
                        sentiment = dto.Sentiment,
                        satisfaction = dto.Satisfaction,
                        timestamp = dto.Timestamp,
                        context = dto.Context ?? "{}"
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create/update service satisfaction: GuestId={GuestId}, ServiceId={ServiceId}",
                    dto.GuestId, dto.ServiceId);
                return false;
            }
        }

        public async Task<bool> CreateOccursAtRelationshipAsync(int guestId, int serviceId, DateTime date)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    // Time node oluştur
                    await CreateOrUpdateTimeNodeAsync(date);

                    var dateKey = date.ToString("yyyy-MM-dd");
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        MATCH (s:Service {id: $serviceId})
                        MATCH (t:Time {date: $dateKey})
                        MERGE (s)-[r:OCCURS_AT]->(t)
                        MERGE (g)-[:USES]->(s)
                        RETURN r";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId,
                        serviceId,
                        dateKey
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create OCCURS_AT relationship: GuestId={GuestId}, ServiceId={ServiceId}",
                    guestId, serviceId);
                return false;
            }
        }

        public async Task<bool> CreateFeelsRelationshipAsync(int guestId, string emotionType, double sentimentScore, DateTime timestamp)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    // Emotion node oluştur
                    await CreateOrUpdateEmotionNodeAsync(emotionType, sentimentScore);

                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        MATCH (e:Emotion {type: $emotionType})
                        MERGE (g)-[r:FEELS]->(e)
                        SET r.sentimentScore = $sentimentScore,
                            r.timestamp = datetime($timestamp),
                            r.frequency = COALESCE(r.frequency, 0) + 1
                        RETURN r";

                    var queryResult = await tx.RunAsync(query, new
                    {
                        guestId,
                        emotionType,
                        sentimentScore,
                        timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss")
                    });

                    var record = await queryResult.SingleAsync();
                    return record != null;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create FEELS relationship: GuestId={GuestId}, EmotionType={EmotionType}",
                    guestId, emotionType);
                return false;
            }
        }

        public async Task<bool> DeleteGuestNodeAsync(int guestId)
        {
            try
            {
                var result = await _neo4jService.ExecuteWriteAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})
                        DETACH DELETE g
                        RETURN count(g) as deleted";

                    var queryResult = await tx.RunAsync(query, new { guestId });
                    var record = await queryResult.SingleAsync();
                    return record["deleted"].As<int>() > 0;
                });
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete guest node: GuestId={GuestId}", guestId);
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetGuestRelationshipsAsync(int guestId)
        {
            try
            {
                return await _neo4jService.ExecuteReadAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})-[r]-(related)
                        RETURN type(r) as relationshipType, 
                               labels(related)[0] as relatedType,
                               related.id as relatedId,
                               related.name as relatedName,
                               r.weight as weight,
                               r.frequency as frequency,
                               r.sentiment as sentiment,
                               r.satisfaction as satisfaction
                        ORDER BY r.weight DESC
                        LIMIT 50";

                    var result = await tx.RunAsync(query, new { guestId });
                    var records = await result.ToListAsync();

                    var relationships = new Dictionary<string, object>();
                    foreach (var record in records)
                    {
                        var relType = record["relationshipType"].As<string>();
                        if (!relationships.ContainsKey(relType))
                        {
                            relationships[relType] = new List<object>();
                        }

                        ((List<object>)relationships[relType]).Add(new
                        {
                            RelatedType = record["relatedType"].As<string>(),
                            RelatedId = record["relatedId"].As<int>(),
                            RelatedName = record["relatedName"].As<string>(),
                            Weight = record["weight"].As<double>(),
                            Frequency = record["frequency"].As<int>(),
                            Sentiment = record["sentiment"].As<double>(),
                            Satisfaction = record["satisfaction"].As<double>()
                        });
                    }

                    return relationships;
                }) ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest relationships: GuestId={GuestId}", guestId);
                return new Dictionary<string, object>();
            }
        }

        public async Task<double> CalculateGuestStaffCompatibilityAsync(int guestId, int staffId)
        {
            try
            {
                return await _neo4jService.ExecuteReadAsync(async tx =>
                {
                    var query = @"
                        MATCH (g:Guest {id: $guestId})-[r:INTERACTS]->(s:Staff {id: $staffId})
                        RETURN 
                            COALESCE(r.weight, 0) * 0.4 +
                            (COALESCE(r.satisfaction, 5) / 10.0) * 0.4 +
                            (COALESCE(r.sentiment, 0) + 1.0) / 2.0 * 0.2 as compatibility";

                    var result = await tx.RunAsync(query, new { guestId, staffId });
                    var record = await result.SingleAsync();
                    return record["compatibility"].As<double>();
                    return record["compatibility"].As<double>();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate compatibility: GuestId={GuestId}, StaffId={StaffId}",
                    guestId, staffId);
                return 0.0;
            }
        }

        private string GetSeason(DateTime date)
        {
            var month = date.Month;
            return month switch
            {
                >= 3 and <= 5 => "Spring",
                >= 6 and <= 8 => "Summer",
                >= 9 and <= 11 => "Autumn",
                _ => "Winter"
            };
        }
    }
}
