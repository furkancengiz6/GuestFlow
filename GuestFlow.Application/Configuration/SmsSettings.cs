namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// SMS ayarları
    /// </summary>
    public class SmsSettings
    {
        public bool Enabled { get; set; } = false;
        public string Provider { get; set; } = "Mock";
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string SenderName { get; set; } = "GuestFlow";
    }
}

