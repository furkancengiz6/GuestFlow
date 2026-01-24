// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Interfaces;

namespace GuestFlow.Domain.Entities.Intelligence
{
    /// <summary>
    /// Staff behavior tracking entity - Personel davranış takibi
    /// </summary>
    public class StaffBehaviorEntity : BaseEntity
    {
        public int StaffId { get; set; }

        /// <summary>
        /// Behavior type (ServiceDelivery, GuestInteraction, ProblemSolving, PreferenceLearning)
        /// </summary>
        public string BehaviorType { get; set; } = string.Empty;

        /// <summary>
        /// Behavior category (e.g., "Transfer", "CityTour", "Communication")
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
        /// Guest ID (if related to guest interaction)
        /// </summary>
        public int? GuestId { get; set; }

        /// <summary>
        /// Service ID (if related to service)
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Service type
        /// </summary>
        public string? ServiceType { get; set; }

        /// <summary>
        /// Success score (0-10)
        /// </summary>
        public double? SuccessScore { get; set; }

        /// <summary>
        /// Guest satisfaction (0-10)
        /// </summary>
        public double? GuestSatisfaction { get; set; }

        /// <summary>
        /// Response time in minutes
        /// </summary>
        public int? ResponseTimeMinutes { get; set; }

        /// <summary>
        /// Preference learning indicator
        /// </summary>
        public bool PreferenceLearned { get; set; } = false;

        /// <summary>
        /// Problem solved indicator
        /// </summary>
        public bool ProblemSolved { get; set; } = false;

        /// <summary>
        /// Synced to Neo4j
        /// </summary>
        public bool SyncedToGraph { get; set; } = false;

        /// <summary>
        /// Sync timestamp
        /// </summary>
        public DateTime? GraphSyncDate { get; set; }

        // Navigation Properties
        public virtual Core.PersonnelEntity Staff { get; set; } = null!;
        public virtual Core.GuestEntity? Guest { get; set; }
    }
}
