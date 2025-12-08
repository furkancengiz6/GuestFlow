namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Cache ayarları
    /// </summary>
    public class CacheSettings
    {
        public bool Enabled { get; set; } = true;
        public int DefaultCacheDurationMinutes { get; set; } = 60;
        public Dictionary<string, int> CacheDurations { get; set; } = new Dictionary<string, int>();
    }
}

