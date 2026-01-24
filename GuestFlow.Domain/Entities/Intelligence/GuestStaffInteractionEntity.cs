// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Interfaces;

namespace GuestFlow.Domain.Entities.Intelligence
{
    /// <summary>
    /// Guest-Staff interaction tracking entity - Misafir-Personel etkileşim takibi
    /// </summary>
    public class GuestStaffInteractionEntity : BaseEntity
    {
        public int GuestId { get; set; }
        public int StaffId { get; set; }

        /// <summary>
        /// Interaction type (Service, Communication, ProblemSolving, Recommendation)
        /// </summary>
        public string InteractionType { get; set; } = string.Empty;

        /// <summary>
        /// Interaction channel (Email, SMS, WhatsApp, InPerson, Phone)
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Interaction date
        /// </summary>
        public DateTime InteractionDate { get; set; }

        /// <summary>
        /// Duration in minutes
        /// </summary>
        public int? DurationMinutes { get; set; }

        /// <summary>
        /// Sentiment score (-1.0 to 1.0)
        /// </summary>
        public double? SentimentScore { get; set; }

        /// <summary>
        /// Satisfaction score (0-10)
        /// </summary>
        public double? SatisfactionScore { get; set; }

        /// <summary>
        /// Interaction context (JSON)
        /// </summary>
        public string? Context { get; set; }

        /// <summary>
        /// Related service ID
        /// </summary>
        public int? ServiceId { get; set; }

        /// <summary>
        /// Related service type
        /// </summary>
        public string? ServiceType { get; set; }

        /// <summary>
        /// Relationship strength (calculated)
        /// </summary>
        public double? RelationshipStrength { get; set; }

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
        public virtual Core.PersonnelEntity Staff { get; set; } = null!;
    }
}
