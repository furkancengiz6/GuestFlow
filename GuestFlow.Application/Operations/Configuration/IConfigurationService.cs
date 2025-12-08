using GuestFlow.Application.Configuration;

namespace GuestFlow.Application.Operations.Configuration
{
    /// <summary>
    /// Konfigürasyon servisi interface'i
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// JWT ayarlarını getirir
        /// </summary>
        JwtSettings GetJwtSettings();

        /// <summary>
        /// PDF ayarlarını getirir
        /// </summary>
        PdfSettings GetPdfSettings();

        /// <summary>
        /// E-posta ayarlarını getirir
        /// </summary>
        EmailSettings GetEmailSettings();

        /// <summary>
        /// Dosya ayarlarını getirir
        /// </summary>
        FileSettings GetFileSettings();

        /// <summary>
        /// Para birimi ayarlarını getirir
        /// </summary>
        CurrencySettings GetCurrencySettings();

        /// <summary>
        /// SMS ayarlarını getirir
        /// </summary>
        SmsSettings GetSmsSettings();

        /// <summary>
        /// Yerelleştirme ayarlarını getirir
        /// </summary>
        LocalizationSettings GetLocalizationSettings();

        /// <summary>
        /// Uygulama ayarlarını getirir
        /// </summary>
        AppSettings GetAppSettings();

        /// <summary>
        /// Connection string'i getirir
        /// </summary>
        string GetConnectionString(string name = "DefaultConnection");

        /// <summary>
        /// Belirli bir konfigürasyon değerini getirir
        /// </summary>
        string? GetValue(string key);

        /// <summary>
        /// Belirli bir konfigürasyon değerini getirir (generic)
        /// </summary>
        T? GetValue<T>(string key) where T : class;
    }
}

