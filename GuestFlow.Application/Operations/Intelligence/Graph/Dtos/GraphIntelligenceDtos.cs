using System;

namespace GuestFlow.Application.Operations.Intelligence.Graph.Dtos
{
    public class HiddenConnectionDto
    {
        public int Guest1Id { get; set; }
        public int Guest2Id { get; set; }
        public string Reason { get; set; } = null!;
        public string? Detail { get; set; }
    }

    public class FrictionRiskDto
    {
        public int GuestId { get; set; }
        public int FrictionCount { get; set; }
        public string Status { get; set; } = "AtRisk";
    }

    public class InfluenceDto
    {
        public int GuestId { get; set; }
        public string Name { get; set; } = null!;
        public double InfluenceScore { get; set; }
    }
}
