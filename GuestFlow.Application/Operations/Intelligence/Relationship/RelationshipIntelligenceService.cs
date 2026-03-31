// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Operations.Intelligence.Graph;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Application.Models.AI;
using GuestFlow.Persistence.Context;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Application.Operations.Intelligence.Relationship
{
    /// <summary>
    /// Relationship Intelligence Service implementation
    /// </summary>
    public class RelationshipIntelligenceService : IRelationshipIntelligenceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GuestFlowDbContext _context;
        private readonly IGraphDataService _graphDataService;
        private readonly IBehavioralTrackingService _behavioralTrackingService;
        private readonly IAIAssistantService _aiAssistantService;
        private readonly ILogger<RelationshipIntelligenceService> _logger;

        public RelationshipIntelligenceService(
            IUnitOfWork unitOfWork,
            GuestFlowDbContext context,
            IGraphDataService graphDataService,
            IBehavioralTrackingService behavioralTrackingService,
            IAIAssistantService aiAssistantService,
            ILogger<RelationshipIntelligenceService> logger)
        {
            _unitOfWork = unitOfWork;
            _context = context;
            _graphDataService = graphDataService;
            _behavioralTrackingService = behavioralTrackingService;
            _aiAssistantService = aiAssistantService;
            _logger = logger;
        }

        public async Task<List<StaffMatchResult>> FindBestStaffMatchesAsync(int guestId, int? limit = 5)
        {
            try
            {
                // Try AI-powered staff matching first
                var aiMatches = await FindBestStaffMatchesWithAIAsync(guestId, limit);
                if (aiMatches != null && aiMatches.Any())
                {
                    _logger.LogInformation("AI-Powered Staff Matching completed for GuestId={GuestId}, Found={Count}", 
                        guestId, aiMatches.Count);
                    return aiMatches;
                }

                // Get all interactions for this guest
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && !i.IsDeleted)
                    .Include(i => i.Staff)
                    .ToListAsync();

                if (!interactions.Any())
                {
                    // If no interactions, return all staff with neutral scores
                    var allStaff = await _unitOfWork.Personnels
                        .GetAll(p => !p.IsDeleted)
                        .Take(limit ?? 5)
                        .ToListAsync();

                    return allStaff.Select(s => new StaffMatchResult
                    {
                        StaffId = s.Id,
                        StaffName = s.FullName,
                        CompatibilityScore = 0.5,
                        RelationshipStrength = 0.0,
                        InteractionCount = 0,
                        AverageSatisfaction = 5.0,
                        MatchReason = "No previous interactions"
                    }).ToList();
                }

                // Group by staff and calculate metrics
                var staffGroups = interactions
                    .GroupBy(i => i.StaffId)
                    .Select(g => new
                    {
                        StaffId = g.Key,
                        Staff = g.First().Staff,
                        Interactions = g.ToList(),
                        InteractionCount = g.Count(),
                        AverageSatisfaction = g.Any(i => i.SatisfactionScore.HasValue) 
                            ? g.Where(i => i.SatisfactionScore.HasValue).Average(i => i.SatisfactionScore!.Value) 
                            : 5.0,
                        AverageSentiment = g.Any(i => i.SentimentScore.HasValue) 
                            ? g.Where(i => i.SentimentScore.HasValue).Average(i => i.SentimentScore!.Value) 
                            : 0.0,
                        RelationshipStrength = g.Any(i => i.RelationshipStrength.HasValue) 
                            ? g.Where(i => i.RelationshipStrength.HasValue).Average(i => i.RelationshipStrength!.Value) 
                            : 0.0
                    })
                    .ToList();

                var results = new List<StaffMatchResult>();

                foreach (var group in staffGroups)
                {
                    // Calculate compatibility score
                    var compatibility = await _graphDataService.CalculateGuestStaffCompatibilityAsync(guestId, group.StaffId);
                    
                    // If graph doesn't have data, calculate from SQL data
                    if (compatibility == 0.0)
                    {
                        compatibility = CalculateCompatibilityFromInteractions(group.Interactions);
                    }

                    results.Add(new StaffMatchResult
                    {
                        StaffId = group.StaffId,
                        StaffName = group.Staff?.FullName ?? "Unknown",
                        CompatibilityScore = compatibility,
                        RelationshipStrength = group.RelationshipStrength,
                        InteractionCount = group.InteractionCount,
                        AverageSatisfaction = group.AverageSatisfaction > 0 ? group.AverageSatisfaction : 5.0,
                        MatchReason = GetMatchReason(group.InteractionCount, group.AverageSatisfaction, compatibility)
                    });
                }

                // Sort by compatibility score and return top matches
                return results
                    .OrderByDescending(r => r.CompatibilityScore)
                    .ThenByDescending(r => r.AverageSatisfaction)
                    .Take(limit ?? 5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find best staff matches: GuestId={GuestId}", guestId);
                return new List<StaffMatchResult>();
            }
        }

        public async Task<List<GuestMatchResult>> FindBestGuestMatchesAsync(int staffId, int? limit = 5)
        {
            try
            {
                // Try AI-powered guest matching first
                var aiMatches = await FindBestGuestMatchesWithAIAsync(staffId, limit);
                if (aiMatches != null && aiMatches.Any())
                {
                    _logger.LogInformation("AI-Powered Guest Matching completed for StaffId={StaffId}, Found={Count}", 
                        staffId, aiMatches.Count);
                    return aiMatches;
                }

                // Get all interactions for this staff member
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.StaffId == staffId && !i.IsDeleted)
                    .Include(i => i.Guest)
                    .ToListAsync();

                if (!interactions.Any())
                {
                    return new List<GuestMatchResult>();
                }

                // Group by guest and calculate metrics
                var guestGroups = interactions
                    .GroupBy(i => i.GuestId)
                    .Select(g => new
                    {
                        GuestId = g.Key,
                        Guest = g.First().Guest,
                        Interactions = g.ToList(),
                        InteractionCount = g.Count(),
                        AverageSatisfaction = g.Any(i => i.SatisfactionScore.HasValue) 
                            ? g.Where(i => i.SatisfactionScore.HasValue).Average(i => i.SatisfactionScore!.Value) 
                            : 5.0,
                        RelationshipStrength = g.Any(i => i.RelationshipStrength.HasValue) 
                            ? g.Where(i => i.RelationshipStrength.HasValue).Average(i => i.RelationshipStrength!.Value) 
                            : 0.0
                    })
                    .ToList();

                var results = new List<GuestMatchResult>();

                foreach (var group in guestGroups)
                {
                    // Calculate compatibility score
                    var compatibility = await _graphDataService.CalculateGuestStaffCompatibilityAsync(group.GuestId, staffId);
                    
                    // If graph doesn't have data, calculate from SQL data
                    if (compatibility == 0.0)
                    {
                        compatibility = CalculateCompatibilityFromInteractions(group.Interactions);
                    }

                    results.Add(new GuestMatchResult
                    {
                        GuestId = group.GuestId,
                        GuestName = group.Guest?.FullName ?? "Unknown",
                        CompatibilityScore = compatibility,
                        RelationshipStrength = group.RelationshipStrength,
                        InteractionCount = group.InteractionCount,
                        AverageSatisfaction = group.AverageSatisfaction > 0 ? group.AverageSatisfaction : 5.0,
                        MatchReason = GetMatchReason(group.InteractionCount, group.AverageSatisfaction, compatibility)
                    });
                }

                // Sort by compatibility score and return top matches
                return results
                    .OrderByDescending(r => r.CompatibilityScore)
                    .ThenByDescending(r => r.AverageSatisfaction)
                    .Take(limit ?? 5)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find best guest matches: StaffId={StaffId}", staffId);
                return new List<GuestMatchResult>();
            }
        }

        public async Task<List<ServiceMatchResult>> FindBestServiceMatchesAsync(int guestId, string? serviceType = null, int? limit = 10)
        {
            try
            {
                // Get guest behaviors related to services
                var query = _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && 
                                b.BehaviorType == "Service" && 
                                !b.IsDeleted);

                if (!string.IsNullOrEmpty(serviceType))
                {
                    query = query.Where(b => b.Category == serviceType);
                }

                var behaviors = await query.ToListAsync();

                // Get service satisfaction from graph or SQL
                var serviceGroups = behaviors
                    .Where(b => !string.IsNullOrEmpty(b.Category) && b.RelatedEntityId.HasValue)
                    .GroupBy(b => new { b.Category, b.RelatedEntityId })
                    .Select(g => new
                    {
                        ServiceId = g.Key.RelatedEntityId!.Value,
                        ServiceType = g.Key.Category!,
                        Behaviors = g.ToList(),
                        UsageCount = g.Count(),
                        AverageSatisfaction = g.Where(b => b.SatisfactionScore.HasValue)
                            .Average(b => b.SatisfactionScore!.Value),
                        AverageSentiment = g.Where(b => b.SentimentScore.HasValue)
                            .Average(b => b.SentimentScore!.Value)
                    })
                    .ToList();

                var results = new List<ServiceMatchResult>();

                foreach (var group in serviceGroups)
                {
                    // Get service name
                    var serviceName = await GetServiceNameAsync(group.ServiceType, group.ServiceId);

                    // Calculate match score
                    var matchScore = CalculateServiceMatchScore(
                        group.UsageCount,
                        group.AverageSatisfaction,
                        group.AverageSentiment);

                    results.Add(new ServiceMatchResult
                    {
                        ServiceId = group.ServiceId,
                        ServiceType = group.ServiceType,
                        ServiceName = serviceName,
                        MatchScore = matchScore,
                        UsageCount = group.UsageCount,
                        AverageSatisfaction = group.AverageSatisfaction > 0 ? group.AverageSatisfaction : 5.0,
                        MatchReason = GetServiceMatchReason(group.UsageCount, group.AverageSatisfaction)
                    });
                }

                return results
                    .OrderByDescending(r => r.MatchScore)
                    .Take(limit ?? 10)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to find best service matches: GuestId={GuestId}", guestId);
                return new List<ServiceMatchResult>();
            }
        }

        public async Task<double> CalculateCompatibilityAsync(int guestId, int staffId)
        {
            try
            {
                // Try to get from graph first
                var graphCompatibility = await _graphDataService.CalculateGuestStaffCompatibilityAsync(guestId, staffId);
                if (graphCompatibility > 0)
                {
                    return graphCompatibility;
                }

                // Fallback to SQL-based calculation
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && i.StaffId == staffId && !i.IsDeleted)
                    .ToListAsync();

                if (!interactions.Any())
                {
                    return 0.5; // Neutral compatibility for no interactions
                }

                return CalculateCompatibilityFromInteractions(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate compatibility: GuestId={GuestId}, StaffId={StaffId}",
                    guestId, staffId);
                return 0.5;
            }
        }

        public async Task<double> GetRelationshipStrengthAsync(int guestId, int staffId)
        {
            try
            {
                var interaction = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && i.StaffId == staffId && !i.IsDeleted)
                    .OrderByDescending(i => i.InteractionDate)
                    .FirstOrDefaultAsync();

                if (interaction?.RelationshipStrength.HasValue == true)
                {
                    return interaction.RelationshipStrength.Value;
                }

                // Calculate from interactions
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && i.StaffId == staffId && !i.IsDeleted)
                    .ToListAsync();

                if (!interactions.Any())
                {
                    return 0.0;
                }

                // Relationship strength based on frequency, satisfaction, and sentiment
                var frequency = interactions.Count;
                var avgSatisfaction = interactions.Where(i => i.SatisfactionScore.HasValue)
                    .Average(i => i.SatisfactionScore!.Value);
                var avgSentiment = interactions.Where(i => i.SentimentScore.HasValue)
                    .Average(i => i.SentimentScore!.Value);

                var strength = (frequency / 10.0 * 0.3) + 
                              (avgSatisfaction / 10.0 * 0.4) + 
                              ((avgSentiment + 1.0) / 2.0 * 0.3);

                return Math.Min(1.0, strength);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get relationship strength: GuestId={GuestId}, StaffId={StaffId}",
                    guestId, staffId);
                return 0.0;
            }
        }

        public async Task<Dictionary<string, object>> GetGuestPreferencePatternsAsync(int guestId)
        {
            try
            {
                var preferences = await _unitOfWork.GuestPreferences
                    .GetAll(p => p.GuestId == guestId && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                var behaviors = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && b.BehaviorType == "Preference" && !b.IsDeleted)
                    .ToListAsync();

                var patterns = new Dictionary<string, object>
                {
                    ["RoomPreferences"] = behaviors
                        .Where(b => b.Category == "Room")
                        .GroupBy(b => b.BehaviorValue)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    ["FoodPreferences"] = behaviors
                        .Where(b => b.Category == "Food")
                        .GroupBy(b => b.BehaviorValue)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    ["ServicePreferences"] = behaviors
                        .Where(b => b.Category == "Service")
                        .GroupBy(b => b.BehaviorValue)
                        .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
                    ["TimePreferences"] = behaviors
                        .Where(b => !string.IsNullOrEmpty(b.TimeOfDay))
                        .GroupBy(b => b.TimeOfDay!)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ["SeasonPreferences"] = behaviors
                        .Where(b => !string.IsNullOrEmpty(b.Season))
                        .GroupBy(b => b.Season!)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // Add explicit preferences if available
                if (preferences != null)
                {
                    patterns["ExplicitPreferences"] = new
                    {
                        PreferredRoomType = preferences.PreferredRoomType,
                        BedPreference = preferences.BedPreference,
                        SmokingPreference = preferences.SmokingPreference,
                        DietaryPreferences = preferences.DietaryPreferences,
                        FoodAllergies = preferences.FoodAllergies,
                        ActivityPreferences = preferences.ActivityPreferences
                    };
                }

                return patterns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest preference patterns: GuestId={GuestId}", guestId);
                return new Dictionary<string, object>();
            }
        }

        public async Task<List<ServiceRecommendation>> RecommendServicesAsync(int guestId, DateTime? targetDate = null)
        {
            try
            {
                // Try AI-powered service recommendations first
                var aiRecommendations = await RecommendServicesWithAIAsync(guestId, targetDate);
                if (aiRecommendations != null && aiRecommendations.Any())
                {
                    _logger.LogInformation("AI-Powered Service Recommendations completed for GuestId={GuestId}, Found={Count}", 
                        guestId, aiRecommendations.Count);
                    return aiRecommendations;
                }

                var recommendations = new List<ServiceRecommendation>();

                // Get guest behavior patterns
                var patterns = await GetGuestPreferencePatternsAsync(guestId);
                var behaviorPatterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);

                // Get best service matches
                var serviceMatches = await FindBestServiceMatchesAsync(guestId, null, 10);

                foreach (var match in serviceMatches.Take(5))
                {
                    var recommendation = new ServiceRecommendation
                    {
                        ServiceType = match.ServiceType,
                        ServiceName = match.ServiceName,
                        RecommendationScore = match.MatchScore,
                        RecommendationReason = $"Based on {match.UsageCount} previous uses with {match.AverageSatisfaction:F1}/10 satisfaction",
                        RecommendedDate = targetDate,
                        Context = new Dictionary<string, object>
                        {
                            ["UsageCount"] = match.UsageCount,
                            ["AverageSatisfaction"] = match.AverageSatisfaction,
                            ["MatchScore"] = match.MatchScore
                        }
                    };

                    recommendations.Add(recommendation);
                }

                // Add time-based recommendations
                if (targetDate.HasValue)
                {
                    var timeOfDay = GetTimeOfDay(targetDate.Value);
                    var season = GetSeason(targetDate.Value);

                    // Check if guest has preferences for this time/season
                    if (patterns.ContainsKey("TimePreferences"))
                    {
                        var timePrefs = patterns["TimePreferences"] as Dictionary<string, int>;
                        if (timePrefs != null && timePrefs.ContainsKey(timeOfDay))
                        {
                            recommendations.Add(new ServiceRecommendation
                            {
                                ServiceType = "General",
                                ServiceName = "Time-based recommendation",
                                RecommendationScore = 0.7,
                                RecommendationReason = $"Guest prefers {timeOfDay} activities",
                                RecommendedDate = targetDate
                            });
                        }
                    }
                }

                return recommendations
                    .OrderByDescending(r => r.RecommendationScore)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to recommend services: GuestId={GuestId}", guestId);
                return new List<ServiceRecommendation>();
            }
        }

        public async Task<RelationshipNetwork> GetGuestRelationshipNetworkAsync(int guestId)
        {
            try
            {
                var network = new RelationshipNetwork { GuestId = guestId };

                // Get guest details for the root node
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest != null)
                {
                    network.GuestNode = new NetworkNode
                    {
                        Id = guestId.ToString(),
                        Type = "Guest",
                        Name = guest.FullName,
                        Properties = new Dictionary<string, object>
                        {
                            { "GuestCode", guest.GuestCode ?? "" }
                        }
                    };
                }

                // Get relationships from graph
                var relationships = await _graphDataService.GetGuestRelationshipsAsync(guestId);

                // Build network nodes and edges
                foreach (var relType in relationships.Keys)
                {
                    if (relationships[relType] is IEnumerable<dynamic> rels)
                    {
                        foreach (var rel in rels)
                        {
                            var relatedId = rel.RelatedId.ToString();
                            var relatedType = rel.RelatedType;
                            var node = new NetworkNode
                            {
                                Id = relatedId,
                                Type = relatedType,
                                Name = rel.RelatedName,
                                Properties = new Dictionary<string, object>
                                {
                                    { "Frequency", rel.Frequency },
                                    { "Satisfaction", rel.Satisfaction },
                                    { "Sentiment", rel.Sentiment }
                                }
                            };

                            // Categorize node
                            if (relatedType == "Staff")
                            {
                                if (!network.StaffNodes.Any(n => n.Id == relatedId))
                                    network.StaffNodes.Add(node);
                            }
                            else if (relatedType == "Service" || relatedType == "Preference")
                            {
                                if (!network.ServiceNodes.Any(n => n.Id == relatedId))
                                    network.ServiceNodes.Add(node);
                            }

                            // Add edge
                            network.Edges.Add(new NetworkEdge
                            {
                                SourceId = guestId.ToString(),
                                TargetId = relatedId,
                                RelationshipType = relType,
                                Weight = rel.Weight,
                                Properties = new Dictionary<string, object>
                                {
                                    { "Weight", rel.Weight }
                                }
                            });
                        }
                    }
                }

                return network;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest relationship network: GuestId={GuestId}", guestId);
                return new RelationshipNetwork { GuestId = guestId };
            }
        }

        private async Task<List<StaffMatchResult>?> FindBestStaffMatchesWithAIAsync(int guestId, int? limit)
        {
            try
            {
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && !i.IsDeleted)
                    .Include(i => i.Staff)
                    .Take(20)
                    .ToListAsync();

                var preferences = await GetGuestPreferencePatternsAsync(guestId);
                var behaviorPatterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);
                
                var availableStaff = await _unitOfWork.Personnels
                    .GetAll(p => !p.IsDeleted)
                    .Take(10)
                    .ToListAsync();

                var context = new
                {
                    GuestId = guestId,
                    RecentInteractions = interactions.Select(i => new { i.StaffId, i.Staff?.FullName, i.Staff?.Department, i.SatisfactionScore, i.SentimentScore }),
                    GuestPreferences = preferences,
                    BehaviorPatterns = behaviorPatterns,
                    AvailableStaff = availableStaff.Select(s => new { s.Id, s.FullName, s.UserType, s.Department })
                };

                var prompt = $@"Analyze guest preferences and staff profiles to find the best staff-guest matches.
                Limit: {limit ?? 5}
                Data: {JsonSerializer.Serialize(context)}

                Return a JSON object with a 'matches' array:
                {{
                  ""matches"": [
                    {{
                      ""staffId"": 1,
                      ""staffName"": ""Name"",
                      ""compatibilityScore"": 0.95,
                      ""relationshipStrength"": 0.8,
                      ""interactionCount"": 5,
                      ""averageSatisfaction"": 9.0,
                      ""matchReason"": ""Personality alignment and positive history""
                    }}
                  ]
                }}
                Response ONLY with the JSON block.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "StaffMatching" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var json = response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<AIStaffMatchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return result?.Matches;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI Staff matching failed for GuestId={GuestId}", guestId);
                return null;
            }
        }

        private async Task<List<GuestMatchResult>?> FindBestGuestMatchesWithAIAsync(int staffId, int? limit)
        {
            try
            {
                var interactions = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.StaffId == staffId && !i.IsDeleted)
                    .Include(i => i.Guest)
                    .Take(20)
                    .ToListAsync();

                if (!interactions.Any()) return null;

                var staff = await _unitOfWork.Personnels.GetByIdAsync(staffId);
                
                var context = new
                {
                    StaffId = staffId,
                    StaffName = staff?.FullName,
                    Department = staff?.Department,
                    RecentInteractions = interactions.Select(i => new { 
                        i.GuestId, 
                        i.Guest?.FullName, 
                        i.SatisfactionScore, 
                        i.SentimentScore 
                    })
                };

                var prompt = $@"Analyze staff history to find best matching guests.
                Limit: {limit ?? 5}
                Data: {JsonSerializer.Serialize(context)}

                Return a JSON object with a 'matches' array:
                {{
                  ""matches"": [
                    {{
                      ""guestId"": 1,
                      ""guestName"": ""Name"",
                      ""compatibilityScore"": 0.95,
                      ""relationshipStrength"": 0.8,
                      ""interactionCount"": 5,
                      ""averageSatisfaction"": 9.0,
                      ""matchReason"": ""High satisfaction and positive sentiment""
                    }}
                  ]
                }}
                Response ONLY with the JSON block.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "GuestMatching" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var json = response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<AIGuestMatchResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return result?.Matches;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI Guest matching failed for StaffId={StaffId}", staffId);
                return null;
            }
        }

        private async Task<List<ServiceRecommendation>?> RecommendServicesWithAIAsync(int guestId, DateTime? targetDate)
        {
            try
            {
                var interactions = await _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && !b.IsDeleted)
                    .OrderByDescending(b => b.BehaviorDate)
                    .Take(30)
                    .ToListAsync();

                var preferences = await GetGuestPreferencePatternsAsync(guestId);
                var behaviorPatterns = await _behavioralTrackingService.GetGuestBehaviorPatternsAsync(guestId);
                
                var context = new
                {
                    GuestId = guestId,
                    TargetDate = targetDate,
                    TimeOfDay = targetDate.HasValue ? GetTimeOfDay(targetDate.Value) : null,
                    Season = targetDate.HasValue ? GetSeason(targetDate.Value) : null,
                    RecentBehaviors = interactions.Select(b => new { b.BehaviorType, b.Category, b.BehaviorValue, b.SatisfactionScore }),
                    GuestPreferences = preferences,
                    BehaviorPatterns = behaviorPatterns
                };

                var prompt = $@"Analyze guest data and recommend personalized services.
                Target Date: {targetDate?.ToString("yyyy-MM-dd") ?? "Now"}
                Data: {JsonSerializer.Serialize(context)}

                Return a JSON object with a 'recommendations' array:
                {{
                  ""recommendations"": [
                    {{
                      ""serviceType"": ""Upsell"",
                      ""serviceName"": ""Luxury Spa Treatment"",
                      ""recommendationScore"": 0.92,
                      ""recommendationReason"": ""Guest showed high satisfaction with relaxation services during summer."",
                      ""recommendedDate"": ""2026-06-15T10:00:00Z"",
                      ""context"": {{ ""type"": ""lifestyle_match"" }}
                    }}
                  ]
                }}
                Response ONLY with the JSON block.";

                var response = await _aiAssistantService.ProcessMessageAsync(new AIChatRequest
                {
                    Message = prompt,
                    Metadata = new Dictionary<string, string> { { "Type", "ServiceRecommendation" } }
                });

                if (string.IsNullOrEmpty(response?.Response)) return null;

                var jsonStart = response.Response.IndexOf('{');
                var jsonEnd = response.Response.LastIndexOf('}');
                if (jsonStart == -1 || jsonEnd == -1) return null;

                var json = response.Response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<AIExtendedServiceResult>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return result?.Recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AI Service recommendation failed for GuestId={GuestId}", guestId);
                return null;
            }
        }

        private class AIExtendedServiceResult
        {
            public List<ServiceRecommendation>? Recommendations { get; set; }
        }

        private class AIStaffMatchResult
        {
            public List<StaffMatchResult>? Matches { get; set; }
        }

        private class AIGuestMatchResult
        {
            public List<GuestMatchResult>? Matches { get; set; }
        }

        private double CalculateCompatibilityFromInteractions(List<Domain.Entities.Intelligence.GuestStaffInteractionEntity> interactions)
        {
            if (!interactions.Any())
                return 0.5;

            var avgSatisfaction = interactions.Where(i => i.SatisfactionScore.HasValue)
                .Average(i => i.SatisfactionScore!.Value);
            var avgSentiment = interactions.Where(i => i.SentimentScore.HasValue)
                .Average(i => i.SentimentScore!.Value);
            var frequency = interactions.Count;

            // Compatibility = weighted average of satisfaction, sentiment, and frequency
            var compatibility = (avgSatisfaction / 10.0 * 0.4) +
                               ((avgSentiment + 1.0) / 2.0 * 0.4) +
                               (Math.Min(frequency / 10.0, 1.0) * 0.2);

            return Math.Max(0.0, Math.Min(1.0, compatibility));
        }

        private double CalculateServiceMatchScore(int usageCount, double avgSatisfaction, double avgSentiment)
        {
            var frequencyScore = Math.Min(usageCount / 5.0, 1.0) * 0.3;
            var satisfactionScore = (avgSatisfaction / 10.0) * 0.5;
            var sentimentScore = ((avgSentiment + 1.0) / 2.0) * 0.2;

            return frequencyScore + satisfactionScore + sentimentScore;
        }

        private string GetMatchReason(int interactionCount, double avgSatisfaction, double compatibility)
        {
            if (interactionCount == 0)
                return "No previous interactions";

            if (compatibility > 0.8)
                return $"Excellent match - {interactionCount} interactions, {avgSatisfaction:F1}/10 satisfaction";
            
            if (compatibility > 0.6)
                return $"Good match - {interactionCount} interactions, {avgSatisfaction:F1}/10 satisfaction";

            return $"Moderate match - {interactionCount} interactions";
        }

        private string GetServiceMatchReason(int usageCount, double avgSatisfaction)
        {
            if (usageCount > 3 && avgSatisfaction > 8)
                return $"Frequently used and highly satisfied ({usageCount} times, {avgSatisfaction:F1}/10)";
            
            if (usageCount > 0)
                return $"Used {usageCount} time(s) with {avgSatisfaction:F1}/10 satisfaction";

            return "Based on guest preferences";
        }

        private async Task<string> GetServiceNameAsync(string serviceType, int serviceId)
        {
            try
            {
                return serviceType switch
                {
                    "Transfer" => (await _unitOfWork.Transfers.GetByIdAsync(serviceId))?.PickupAddress ?? $"Transfer #{serviceId}",
                    "CityTour" => (await _unitOfWork.CityTours.GetByIdAsync(serviceId))?.City?.CityName ?? $"City Tour #{serviceId}",
                    "YachtTour" => (await _unitOfWork.YachtTours.GetByIdAsync(serviceId))?.YachtName ?? $"Yacht Tour #{serviceId}",
                    "Restaurant" => (await _context.Restaurants.FindAsync(serviceId))?.RestaurantName ?? $"Restaurant #{serviceId}",
                    _ => $"{serviceType} #{serviceId}"
                };
            }
            catch
            {
                return $"{serviceType} #{serviceId}";
            }
        }

        private string GetTimeOfDay(DateTime dateTime)
        {
            var hour = dateTime.Hour;
            return hour switch
            {
                >= 5 and < 12 => "Morning",
                >= 12 and < 17 => "Afternoon",
                >= 17 and < 21 => "Evening",
                _ => "Night"
            };
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
