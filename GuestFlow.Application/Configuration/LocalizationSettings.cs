namespace GuestFlow.Application.Configuration
{
    /// <summary>
    /// Yerelleştirme ayarları
    /// </summary>
    public class LocalizationSettings
    {
        public string DefaultCulture { get; set; } = "tr-TR";
        public string[] SupportedCultures { get; set; } = { "tr-TR", "en-US" };
    }
}

