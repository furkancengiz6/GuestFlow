// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Intelligence.Relationship
{
    /// <summary>
    /// Relationship Intelligence Service - İlişki zekası servisi
    /// </summary>
    public interface IRelationshipIntelligenceService
    {
        /// <summary>
        /// Find best staff match for a guest (misafir için en uygun personeli bul)
        /// </summary>
        Task<List<StaffMatchResult>> FindBestStaffMatchesAsync(int guestId, int? limit = 5);

        /// <summary>
        /// Find best service matches for a guest (misafir için en uygun hizmetleri bul)
        /// </summary>
        Task<List<ServiceMatchResult>> FindBestServiceMatchesAsync(int guestId, string? serviceType = null, int? limit = 10);

        /// <summary>
        /// Calculate guest-staff compatibility score (misafir-personel uyum skoru)
        /// </summary>
        Task<double> CalculateCompatibilityAsync(int guestId, int staffId);

        /// <summary>
        /// Get relationship strength between guest and staff
        /// </summary>
        Task<double> GetRelationshipStrengthAsync(int guestId, int staffId);

        /// <summary>
        /// Get guest preference patterns (misafir tercih kalıpları)
        /// </summary>
        Task<Dictionary<string, object>> GetGuestPreferencePatternsAsync(int guestId);

        /// <summary>
        /// Recommend services based on guest behavior (davranış bazlı hizmet önerileri)
        /// </summary>
        Task<List<ServiceRecommendation>> RecommendServicesAsync(int guestId, DateTime? targetDate = null);

        /// <summary>
        /// Get guest relationship network (misafir ilişki ağı)
        /// </summary>
        Task<RelationshipNetwork> GetGuestRelationshipNetworkAsync(int guestId);
    }

    /// <summary>
    /// Staff match result
    /// </summary>
    public class StaffMatchResult
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public double CompatibilityScore { get; set; }
        public double RelationshipStrength { get; set; }
        public int InteractionCount { get; set; }
        public double AverageSatisfaction { get; set; }
        public string? MatchReason { get; set; }
    }

    /// <summary>
    /// Service match result
    /// </summary>
    public class ServiceMatchResult
    {
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public double MatchScore { get; set; }
        public int UsageCount { get; set; }
        public double AverageSatisfaction { get; set; }
        public string? MatchReason { get; set; }
    }

    /// <summary>
    /// Service recommendation
    /// </summary>
    public class ServiceRecommendation
    {
        public string ServiceType { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public double RecommendationScore { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public DateTime? RecommendedDate { get; set; }
        public Dictionary<string, object>? Context { get; set; }
    }

    /// <summary>
    /// Relationship network
    /// </summary>
    public class RelationshipNetwork
    {
        public int GuestId { get; set; }
        public List<NetworkNode> StaffNodes { get; set; } = new List<NetworkNode>();
        public List<NetworkNode> ServiceNodes { get; set; } = new List<NetworkNode>();
        public List<NetworkEdge> Edges { get; set; } = new List<NetworkEdge>();
    }

    /// <summary>
    /// Network node
    /// </summary>
    public class NetworkNode
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Network edge
    /// </summary>
    public class NetworkEdge
    {
        public string SourceId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public double Weight { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
