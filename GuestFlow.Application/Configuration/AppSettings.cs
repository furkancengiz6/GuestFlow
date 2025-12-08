namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Uygulama genel ayarları
    /// </summary>
    public class AppSettings
    {
        public string Name { get; set; } = "GuestFlow";
        public string Version { get; set; } = "1.0.0";
        public string Environment { get; set; } = "Development";
    }
}

