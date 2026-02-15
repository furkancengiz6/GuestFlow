// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Operations.Intelligence.Graph;
using GuestFlow.Application.Operations.Intelligence.Graph.Dtos;
using static GuestFlow.Application.Operations.Intelligence.Graph.GraphNodeTypes;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using GuestFlow.Application.Operations.Notification;

namespace GuestFlow.Application.Operations.Intelligence.Behavioral
{
    /// <summary>
    /// Behavioral tracking service implementation
    /// </summary>
    public class BehavioralTrackingService : IBehavioralTrackingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGraphDataService _graphDataService;
        private readonly GuestFlow.Domain.Entities.Repositories.IRepository<PersonnelEntity> _personnelRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<BehavioralTrackingService> _logger;

        public BehavioralTrackingService(
            IUnitOfWork unitOfWork,
            IGraphDataService graphDataService,
            GuestFlow.Domain.Entities.Repositories.IRepository<PersonnelEntity> personnelRepository,
            INotificationService notificationService,
            ILogger<BehavioralTrackingService> logger)
        {
            _unitOfWork = unitOfWork;
            _graphDataService = graphDataService;
            _personnelRepository = personnelRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task TrackGuestBehaviorAsync(int guestId, string behaviorType, string? category = null,
            string? behaviorValue = null, double? sentimentScore = null, double? satisfactionScore = null,
            decimal? amount = null, string? currency = null, string? relatedEntityType = null, int? relatedEntityId = null)
        {
            try
            {
                var behavior = new GuestBehaviorEntity
                {
                    GuestId = guestId,
                    BehaviorType = behaviorType,
                    Category = category,
                    BehaviorValue = behaviorValue,
                    BehaviorDate = DateTime.UtcNow,
                    TimeOfDay = GetTimeOfDay(DateTime.UtcNow),
                    DayOfWeek = DateTime.UtcNow.DayOfWeek.ToString(),
                    Season = GetSeason(DateTime.UtcNow),
                    SentimentScore = sentimentScore,
                    SatisfactionScore = satisfactionScore,
                    Amount = amount,
                    Currency = currency,
                    RelatedEntityType = relatedEntityType,
                    RelatedEntityId = relatedEntityId,
                    SyncedToGraph = false
                };

                await _unitOfWork.GuestBehaviors.AddAsync(behavior);
                await _unitOfWork.SaveChangesAsync();

                // Dual-write: Sync to Graph immediately
                await SyncGuestBehaviorToGraphAsync(behavior);

                _logger.LogInformation("Guest behavior tracked: GuestId={GuestId}, Type={BehaviorType}", guestId, behaviorType);

                // Fetch guest info once for all notifications
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);

                // Notify staff for critical behaviors
                if (guest != null && ((sentimentScore.HasValue && sentimentScore < -0.5) || 
                    (satisfactionScore.HasValue && satisfactionScore <= 2.0)))
                {
                    try
                    {
                        await _notificationService.CreateAndSendNotificationAsync(new GuestFlow.Application.Operations.Notification.Dtos.CreateNotificationDto
                        {
                            Title = "Kritik Misafir Davranışı Tespiti",
                            Content = $"{guest.FullName} için olumsuz bir deneyim tespit edildi. Tür: {behaviorType}, Kategori: {category}",
                            NotificationType = "Push",
                            RelatedEntityType = "GuestBehavior",
                            RelatedEntityId = behavior.Id
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send notification for critical behavior");
                    }
                }

                // VIP Misafir Takibi ve Özel Bildirim
                if (guest != null && guest.IsSpecialGuest)
                {
                    try
                    {
                        await _notificationService.CreateAndSendNotificationAsync(new GuestFlow.Application.Operations.Notification.Dtos.CreateNotificationDto
                        {
                            Title = "⭐ VIP MİSAFİR AKTİVİTESİ",
                            Content = $"VIP Misafir {guest.FullName} için yeni bir aktivite tespit edildi: {behaviorType}. Lütfen öncelikli hizmet sağlayın.",
                            NotificationType = "Push",
                            RelatedEntityType = "Guest",
                            RelatedEntityId = guestId
                        });
                        _logger.LogInformation("VIP notification sent for GuestId={GuestId}", guestId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to send VIP notification");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking guest behavior: {Message}", ex.Message);
            }
        }

        public async Task TrackStaffBehaviorAsync(int staffId, string behaviorType, string? category = null,
            string? behaviorValue = null, int? guestId = null, int? serviceId = null, string? serviceType = null,
            double? successScore = null, double? guestSatisfaction = null, int? responseTimeMinutes = null,
            bool preferenceLearned = false, bool problemSolved = false)
        {
            try
            {
                var behavior = new StaffBehaviorEntity
                {
                    StaffId = staffId,
                    BehaviorType = behaviorType,
                    Category = category,
                    BehaviorValue = behaviorValue,
                    BehaviorDate = DateTime.UtcNow,
                    GuestId = guestId,
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    SuccessScore = successScore,
                    GuestSatisfaction = guestSatisfaction,
                    ResponseTimeMinutes = responseTimeMinutes,
                    PreferenceLearned = preferenceLearned,
                    ProblemSolved = problemSolved,
                    SyncedToGraph = false
                };

                await _unitOfWork.StaffBehaviors.AddAsync(behavior);
                await _unitOfWork.SaveChangesAsync();

                // Dual-write: Sync to Graph immediately
                await SyncStaffBehaviorToGraphAsync(behavior);

                _logger.LogInformation("Staff behavior tracked: StaffId={StaffId}, Type={BehaviorType}", staffId, behaviorType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track staff behavior: StaffId={StaffId}", staffId);
                throw;
            }
        }

        public async Task TrackGuestStaffInteractionAsync(int guestId, int staffId, string interactionType,
            string? channel = null, int? durationMinutes = null, double? sentimentScore = null,
            double? satisfactionScore = null, string? context = null, int? serviceId = null, string? serviceType = null)
        {
            try
            {
                var interaction = new GuestStaffInteractionEntity
                {
                    GuestId = guestId,
                    StaffId = staffId,
                    InteractionType = interactionType,
                    Channel = channel,
                    InteractionDate = DateTime.UtcNow,
                    DurationMinutes = durationMinutes,
                    SentimentScore = sentimentScore,
                    SatisfactionScore = satisfactionScore,
                    Context = context,
                    ServiceId = serviceId,
                    ServiceType = serviceType,
                    SyncedToGraph = false
                };

                await _unitOfWork.GuestStaffInteractions.AddAsync(interaction);
                await _unitOfWork.SaveChangesAsync();

                // Dual-write: Sync to Graph immediately
                await SyncInteractionToGraphAsync(interaction);

                // Calculate relationship strength
                await CalculateRelationshipStrengthAsync(guestId, staffId);

                _logger.LogInformation("Guest-Staff interaction tracked: GuestId={GuestId}, StaffId={StaffId}, Type={InteractionType}",
                    guestId, staffId, interactionType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track guest-staff interaction: GuestId={GuestId}, StaffId={StaffId}",
                    guestId, staffId);
                throw;
            }
        }

        public async Task SyncBehavioralDataToGraphAsync(int? guestId = null, int? staffId = null)
        {
            try
            {
                // Sync guest behaviors
                if (guestId.HasValue)
                {
                    await SyncGuestBehaviorsToGraphAsync(guestId.Value);
                    await SyncGuestStaffInteractionsToGraphAsync(guestId.Value);
                }
                else
                {
                    // Sync all unsynced guest behaviors
                    var unsyncedBehaviors = await _unitOfWork.GuestBehaviors
                        .GetAll(b => !b.SyncedToGraph && !b.IsDeleted)
                        .Take(100)
                        .ToListAsync();

                    foreach (var behavior in unsyncedBehaviors)
                    {
                        await SyncGuestBehaviorToGraphAsync(behavior);
                    }
                }

                // Sync staff behaviors
                if (staffId.HasValue)
                {
                    await SyncStaffBehaviorsToGraphAsync(staffId.Value);
                }
                else
                {
                    var unsyncedStaffBehaviors = await _unitOfWork.StaffBehaviors
                        .GetAll(b => !b.SyncedToGraph && !b.IsDeleted)
                        .Take(100)
                        .ToListAsync();

                    foreach (var behavior in unsyncedStaffBehaviors)
                    {
                        await SyncStaffBehaviorToGraphAsync(behavior);
                    }
                }

                // Sync interactions
                if (!guestId.HasValue)
                {
                    var unsyncedInteractions = await _unitOfWork.GuestStaffInteractions
                        .GetAll(i => !i.SyncedToGraph && !i.IsDeleted)
                        .Take(100)
                        .ToListAsync();

                    foreach (var interaction in unsyncedInteractions)
                    {
                        await SyncInteractionToGraphAsync(interaction);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync behavioral data to graph");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetGuestBehaviorPatternsAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.GuestBehaviors
                    .GetAll(b => b.GuestId == guestId && !b.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(b => b.BehaviorDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(b => b.BehaviorDate <= endDate.Value);

                var behaviors = await query.ToListAsync();

                var patterns = new Dictionary<string, object>
                {
                    ["TotalBehaviors"] = behaviors.Count,
                    ["BehaviorTypes"] = behaviors.GroupBy(b => b.BehaviorType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ["Categories"] = behaviors.Where(b => !string.IsNullOrEmpty(b.Category))
                        .GroupBy(b => b.Category!)
                        .ToDictionary(g => g.Key!, g => g.Count()),
                    ["TimeOfDay"] = behaviors.Where(b => !string.IsNullOrEmpty(b.TimeOfDay))
                        .GroupBy(b => b.TimeOfDay!)
                        .ToDictionary(g => g.Key!, g => g.Count()),
                    ["DayOfWeek"] = behaviors.Where(b => !string.IsNullOrEmpty(b.DayOfWeek))
                        .GroupBy(b => b.DayOfWeek!)
                        .ToDictionary(g => g.Key!, g => g.Count()),
                    ["AverageSentiment"] = behaviors.Where(b => b.SentimentScore.HasValue)
                        .Average(b => b.SentimentScore!.Value),
                    ["AverageSatisfaction"] = behaviors.Where(b => b.SatisfactionScore.HasValue)
                        .Average(b => b.SatisfactionScore!.Value),
                    ["TotalSpending"] = behaviors.Where(b => b.Amount.HasValue)
                        .Sum(b => b.Amount!.Value)
                };

                return patterns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get guest behavior patterns: GuestId={GuestId}", guestId);
                return new Dictionary<string, object>();
            }
        }

        public async Task<Dictionary<string, object>> GetStaffBehaviorPatternsAsync(int staffId, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.StaffBehaviors
                    .GetAll(b => b.StaffId == staffId && !b.IsDeleted);

                if (startDate.HasValue)
                    query = query.Where(b => b.BehaviorDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(b => b.BehaviorDate <= endDate.Value);

                var behaviors = await query.ToListAsync();

                var patterns = new Dictionary<string, object>
                {
                    ["TotalBehaviors"] = behaviors.Count,
                    ["BehaviorTypes"] = behaviors.GroupBy(b => b.BehaviorType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    ["AverageSuccessScore"] = behaviors.Where(b => b.SuccessScore.HasValue)
                        .Average(b => b.SuccessScore!.Value),
                    ["AverageGuestSatisfaction"] = behaviors.Where(b => b.GuestSatisfaction.HasValue)
                        .Average(b => b.GuestSatisfaction!.Value),
                    ["AverageResponseTime"] = behaviors.Where(b => b.ResponseTimeMinutes.HasValue)
                        .Average(b => b.ResponseTimeMinutes!.Value),
                    ["PreferenceLearningRate"] = behaviors.Count(b => b.PreferenceLearned) / (double)behaviors.Count * 100,
                    ["ProblemSolvingRate"] = behaviors.Count(b => b.ProblemSolved) / (double)behaviors.Count * 100
                };

                return patterns;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get staff behavior patterns: StaffId={StaffId}", staffId);
                return new Dictionary<string, object>();
            }
        }

        private async Task SyncGuestBehaviorToGraphAsync(GuestBehaviorEntity behavior)
        {
            try
            {
                // Ensure guest node exists
                var guest = await _unitOfWork.Guests.GetByIdAsync(behavior.GuestId);
                if (guest != null)
                {
                    await _graphDataService.CreateOrUpdateGuestNodeAsync(behavior.GuestId, guest.FullName, guest.GuestCode);
                }

                // Create time node
                await _graphDataService.CreateOrUpdateTimeNodeAsync(behavior.BehaviorDate, behavior.TimeOfDay, behavior.Season);

                // Create preference relationship if applicable
                if (behavior.BehaviorType == "Preference" && !string.IsNullOrEmpty(behavior.Category))
                {
                    var preferenceDto = new GuestPreferenceDto
                    {
                        GuestId = behavior.GuestId,
                        PreferenceType = behavior.Category,
                        PreferenceValue = behavior.BehaviorValue ?? string.Empty,
                        Weight = 0.1,
                        Frequency = 1,
                        Sentiment = behavior.SentimentScore ?? 0.0,
                        Satisfaction = behavior.SatisfactionScore ?? 5.0,
                        Timestamp = behavior.BehaviorDate,
                        Context = behavior.BehaviorValue
                    };

                    await _graphDataService.CreateOrUpdateGuestPreferenceAsync(preferenceDto);
                }

                // Create service relationship if applicable
                if (behavior.BehaviorType == "Service" && !string.IsNullOrEmpty(behavior.Category) && behavior.RelatedEntityId.HasValue)
                {
                    // Ensure service node exists
                    var serviceName = behavior.BehaviorValue ?? $"{behavior.Category} #{behavior.RelatedEntityId}";
                    await _graphDataService.CreateOrUpdateServiceNodeAsync(behavior.RelatedEntityId.Value, behavior.Category, serviceName);

                    // Create USES and OCCURS_AT relationships
                    await _graphDataService.CreateOccursAtRelationshipAsync(behavior.GuestId, behavior.RelatedEntityId.Value, behavior.BehaviorDate);

                    // If satisfaction/sentiment available, create SATISFIES relationship
                    if (behavior.SatisfactionScore.HasValue || behavior.SentimentScore.HasValue)
                    {
                        var satisfactionDto = new ServiceSatisfactionDto
                        {
                            GuestId = behavior.GuestId,
                            ServiceId = behavior.RelatedEntityId.Value,
                            ServiceType = behavior.Category,
                            Satisfaction = behavior.SatisfactionScore ?? 5.0,
                            Sentiment = behavior.SentimentScore ?? 0.0,
                            Timestamp = behavior.BehaviorDate,
                            Context = behavior.BehaviorValue
                        };
                        await _graphDataService.CreateOrUpdateServiceSatisfactionAsync(satisfactionDto);
                    }
                }

                // Create emotion relationship if sentiment available
                if (behavior.SentimentScore.HasValue)
                {
                    var emotionType = behavior.SentimentScore.Value > 0.3 ? EmotionTypes.Positive :
                                     behavior.SentimentScore.Value < -0.3 ? EmotionTypes.Negative :
                                     EmotionTypes.Neutral;

                    await _graphDataService.CreateFeelsRelationshipAsync(
                        behavior.GuestId, emotionType, behavior.SentimentScore.Value, behavior.BehaviorDate);
                }

                behavior.SyncedToGraph = true;
                behavior.GraphSyncDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync guest behavior to graph: BehaviorId={BehaviorId}", behavior.Id);
            }
        }

        private async Task SyncStaffBehaviorToGraphAsync(StaffBehaviorEntity behavior)
        {
            try
            {
                // Ensure staff node exists
                var staff = await _personnelRepository.GetByIdAsync(behavior.StaffId);
                if (staff != null)
                {
                    await _graphDataService.CreateOrUpdateStaffNodeAsync(behavior.StaffId, staff.FullName);
                }

                behavior.SyncedToGraph = true;
                behavior.GraphSyncDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync staff behavior to graph: BehaviorId={BehaviorId}", behavior.Id);
            }
        }

        private async Task SyncInteractionToGraphAsync(GuestStaffInteractionEntity interaction)
        {
            try
            {
                // Ensure nodes exist
                var guest = await _unitOfWork.Guests.GetByIdAsync(interaction.GuestId);
                var staff = await _unitOfWork.Personnels.GetByIdAsync(interaction.StaffId);

                if (guest != null && staff != null)
                {
                    await _graphDataService.CreateOrUpdateGuestNodeAsync(interaction.GuestId, guest.FullName, guest.GuestCode);
                    await _graphDataService.CreateOrUpdateStaffNodeAsync(interaction.StaffId, staff.FullName);

                    var interactionDto = new GuestStaffInteractionDto
                    {
                        GuestId = interaction.GuestId,
                        StaffId = interaction.StaffId,
                        InteractionType = interaction.InteractionType,
                        Weight = 0.1,
                        Frequency = 1,
                        Sentiment = interaction.SentimentScore ?? 0.0,
                        Satisfaction = interaction.SatisfactionScore ?? 5.0,
                        Timestamp = interaction.InteractionDate,
                        Context = interaction.Context
                    };

                    await _graphDataService.CreateOrUpdateGuestStaffInteractionAsync(interactionDto);
                }

                interaction.SyncedToGraph = true;
                interaction.GraphSyncDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync interaction to graph: InteractionId={InteractionId}", interaction.Id);
            }
        }

        private async Task SyncGuestBehaviorsToGraphAsync(int guestId)
        {
            var behaviors = await _unitOfWork.GuestBehaviors
                .GetAll(b => b.GuestId == guestId && !b.SyncedToGraph && !b.IsDeleted)
                .ToListAsync();

            foreach (var behavior in behaviors)
            {
                await SyncGuestBehaviorToGraphAsync(behavior);
            }
        }

        private async Task SyncStaffBehaviorsToGraphAsync(int staffId)
        {
            var behaviors = await _unitOfWork.StaffBehaviors
                .GetAll(b => b.StaffId == staffId && !b.SyncedToGraph && !b.IsDeleted)
                .ToListAsync();

            foreach (var behavior in behaviors)
            {
                await SyncStaffBehaviorToGraphAsync(behavior);
            }
        }

        private async Task SyncGuestStaffInteractionsToGraphAsync(int guestId)
        {
            var interactions = await _unitOfWork.GuestStaffInteractions
                .GetAll(i => i.GuestId == guestId && !i.SyncedToGraph && !i.IsDeleted)
                .ToListAsync();

            foreach (var interaction in interactions)
            {
                await SyncInteractionToGraphAsync(interaction);
            }
        }

        private async Task CalculateRelationshipStrengthAsync(int guestId, int staffId)
        {
            try
            {
                var compatibility = await _graphDataService.CalculateGuestStaffCompatibilityAsync(guestId, staffId);

                var interaction = await _unitOfWork.GuestStaffInteractions
                    .GetAll(i => i.GuestId == guestId && i.StaffId == staffId && !i.IsDeleted)
                    .OrderByDescending(i => i.InteractionDate)
                    .FirstOrDefaultAsync();

                if (interaction != null)
                {
                    interaction.RelationshipStrength = compatibility;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to calculate relationship strength: GuestId={GuestId}, StaffId={StaffId}",
                    guestId, staffId);
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
