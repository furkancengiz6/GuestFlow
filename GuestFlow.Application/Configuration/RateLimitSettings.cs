namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Rate limiting ayarları
    /// </summary>
    public class RateLimitSettings
    {
        public bool Enabled { get; set; } = true;
        public int DefaultRequestsPerMinute { get; set; } = 60;
        public int DefaultRequestsPerHour { get; set; } = 1000;
        public int DefaultRequestsPerDay { get; set; } = 5000;
        public int BlockDurationMinutes { get; set; } = 15;
        public bool EnableIpBlocking { get; set; } = true;
        public bool EnableUserBlocking { get; set; } = true;
        public Dictionary<string, EndpointRateLimit> EndpointLimits { get; set; } = new Dictionary<string, EndpointRateLimit>();
        public List<string> WhitelistedPaths { get; set; } = new List<string>();
        public List<string> BlockedUserAgents { get; set; } = new List<string>();
    }

    /// <summary>
    /// Endpoint bazlı rate limit ayarları
    /// </summary>
    public class EndpointRateLimit
    {
        public int RequestsPerMinute { get; set; } = 60;
        public int RequestsPerHour { get; set; } = 1000;
        public int? RequestsPerDay { get; set; }
    }
}

