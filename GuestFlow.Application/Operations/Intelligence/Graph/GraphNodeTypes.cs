// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Graph
{
    /// <summary>
    /// Graph node types (Graf düğüm tipleri)
    /// </summary>
    public static class GraphNodeTypes
    {
        public const string Guest = "Guest";
        public const string Staff = "Staff";
        public const string Service = "Service";
        public const string Time = "Time";
        public const string Emotion = "Emotion";
    }

    /// <summary>
    /// Graph edge/relationship types (Graf kenar/ilişki tipleri)
    /// </summary>
    public static class GraphEdgeTypes
    {
        public const string Interacts = "INTERACTS";
        public const string Prefers = "PREFERS";
        public const string Satisfies = "SATISFIES";
        public const string Recommends = "RECOMMENDS";
        public const string OccursAt = "OCCURS_AT";
        public const string Feels = "FEELS";
        public const string LearnsFrom = "LEARNS_FROM";
    }

    /// <summary>
    /// Service types for graph nodes
    /// </summary>
    public static class ServiceTypes
    {
        public const string Transfer = "Transfer";
        public const string CityTour = "CityTour";
        public const string YachtTour = "YachtTour";
        public const string Restaurant = "Restaurant";
        public const string Hotel = "Hotel";
    }

    /// <summary>
    /// Emotion types for graph nodes
    /// </summary>
    public static class EmotionTypes
    {
        public const string Positive = "Positive";
        public const string Neutral = "Neutral";
        public const string Negative = "Negative";
        public const string Satisfaction = "Satisfaction";
        public const string Dissatisfaction = "Dissatisfaction";
    }
}
