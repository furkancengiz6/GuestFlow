// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Interfaces;

namespace GuestFlow.Domain.Entities.Intelligence
{
    /// <summary>
    /// Guest behavior tracking entity - Misafir davranış takibi
    /// </summary>
    public class GuestBehaviorEntity : BaseEntity
    {
        public int GuestId { get; set; }

        /// <summary>
        /// Behavior type (Reservation, Service, Communication, Spending, Satisfaction)
        /// </summary>
        public string BehaviorType { get; set; } = string.Empty;

        /// <summary>
        /// Behavior category (e.g., "Transfer", "CityTour", "Email", "SMS")
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Behavior value (JSON formatında detaylı bilgi)
        /// </summary>
        public string? BehaviorValue { get; set; }

        /// <summary>
        /// Timestamp of the behavior
        /// </summary>
        public DateTime BehaviorDate { get; set; }

        /// <summary>
        /// Time of day (Morning, Afternoon, Evening, Night)
        /// </summary>
        public string? TimeOfDay { get; set; }

        /// <summary>
        /// Day of week (Monday, Tuesday, etc.)
        /// </summary>
        public string? DayOfWeek { get; set; }

        /// <summary>
        /// Season (Spring, Summer, Autumn, Winter)
        /// </summary>
        public string? Season { get; set; }

        /// <summary>
        /// Sentiment score (-1.0 to 1.0)
        /// </summary>
        public double? SentimentScore { get; set; }

        /// <summary>
        /// Satisfaction score (0-10)
        /// </summary>
        public double? SatisfactionScore { get; set; }

        /// <summary>
        /// Amount (for spending behaviors)
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        public string? Currency { get; set; }

        /// <summary>
        /// Related entity type (Transfer, CityTour, etc.)
        /// </summary>
        public string? RelatedEntityType { get; set; }

        /// <summary>
        /// Related entity ID
        /// </summary>
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Synced to Neo4j
        /// </summary>
        public bool SyncedToGraph { get; set; } = false;

        /// <summary>
        /// Sync timestamp
        /// </summary>
        public DateTime? GraphSyncDate { get; set; }

        // Navigation Properties
        public virtual Core.GuestEntity Guest { get; set; } = null!;
    }
}
