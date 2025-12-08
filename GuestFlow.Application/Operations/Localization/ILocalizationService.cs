namespace GuestFlow.Application.Operations.Localization
{
    /// <summary>
    /// Lokalizasyon servisi interface'i
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Lokalize edilmiş string döndürür
        /// </summary>
        string GetString(string key, params object[] args);

        /// <summary>
        /// Mevcut kültürü döndürür
        /// </summary>
        string GetCurrentCulture();

        /// <summary>
        /// Desteklenen dilleri döndürür
        /// </summary>
        List<SupportedLanguage> GetSupportedLanguages();
    }

    /// <summary>
    /// Desteklenen dil bilgisi
    /// </summary>
    public class SupportedLanguage
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NativeName { get; set; } = string.Empty;
    }
}

