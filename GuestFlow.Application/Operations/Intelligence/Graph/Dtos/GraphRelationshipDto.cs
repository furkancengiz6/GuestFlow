// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Graph.Dtos
{
    /// <summary>
    /// Graph relationship properties DTO
    /// </summary>
    public class GraphRelationshipDto
    {
        /// <summary>
        /// Relationship weight (0.0-1.0)
        /// </summary>
        public double Weight { get; set; } = 1.0;

        /// <summary>
        /// Frequency (how many times)
        /// </summary>
        public int Frequency { get; set; } = 1;

        /// <summary>
        /// Sentiment score (-1.0 to 1.0)
        /// </summary>
        public double Sentiment { get; set; } = 0.0;

        /// <summary>
        /// Satisfaction score (0-10)
        /// </summary>
        public double Satisfaction { get; set; } = 5.0;

        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Context (JSON string)
        /// </summary>
        public string? Context { get; set; }
    }

    /// <summary>
    /// Guest-Staff interaction relationship
    /// </summary>
    public class GuestStaffInteractionDto : GraphRelationshipDto
    {
        public int GuestId { get; set; }
        public int StaffId { get; set; }
        public string InteractionType { get; set; } = string.Empty; // Service, Communication, etc.
    }

    /// <summary>
    /// Guest preference relationship
    /// </summary>
    public class GuestPreferenceDto : GraphRelationshipDto
    {
        public int GuestId { get; set; }
        public string PreferenceType { get; set; } = string.Empty; // Service, Room, Food, etc.
        public string PreferenceValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service satisfaction relationship
    /// </summary>
    public class ServiceSatisfactionDto : GraphRelationshipDto
    {
        public int GuestId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
    }
}
