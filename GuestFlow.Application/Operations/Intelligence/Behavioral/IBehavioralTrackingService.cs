// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Behavioral
{
    /// <summary>
    /// Behavioral tracking service interface - Davranışsal veri toplama servisi
    /// </summary>
    public interface IBehavioralTrackingService
    {
        /// <summary>
        /// Track guest behavior (misafir davranışını kaydet)
        /// </summary>
        Task TrackGuestBehaviorAsync(int guestId, string behaviorType, string? category = null, 
            string? behaviorValue = null, double? sentimentScore = null, double? satisfactionScore = null,
            decimal? amount = null, string? currency = null, string? relatedEntityType = null, int? relatedEntityId = null);

        /// <summary>
        /// Track staff behavior (personel davranışını kaydet)
        /// </summary>
        Task TrackStaffBehaviorAsync(int staffId, string behaviorType, string? category = null,
            string? behaviorValue = null, int? guestId = null, int? serviceId = null, string? serviceType = null,
            double? successScore = null, double? guestSatisfaction = null, int? responseTimeMinutes = null,
            bool preferenceLearned = false, bool problemSolved = false);

        /// <summary>
        /// Track guest-staff interaction (misafir-personel etkileşimini kaydet)
        /// </summary>
        Task TrackGuestStaffInteractionAsync(int guestId, int staffId, string interactionType,
            string? channel = null, int? durationMinutes = null, double? sentimentScore = null,
            double? satisfactionScore = null, string? context = null, int? serviceId = null, string? serviceType = null);

        /// <summary>
        /// Sync behavioral data to Neo4j graph database
        /// </summary>
        Task SyncBehavioralDataToGraphAsync(int? guestId = null, int? staffId = null);

        /// <summary>
        /// Get guest behavior patterns (misafir davranış kalıplarını getir)
        /// </summary>
        Task<Dictionary<string, object>> GetGuestBehaviorPatternsAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Get staff behavior patterns (personel davranış kalıplarını getir)
        /// </summary>
        Task<Dictionary<string, object>> GetStaffBehaviorPatternsAsync(int staffId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
