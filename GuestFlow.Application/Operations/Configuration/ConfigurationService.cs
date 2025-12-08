using GuestFlow.Application.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GuestFlow.Application.Operations.Configuration
{
    /// <summary>
    /// Konfigürasyon servisi implementasyonu
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IOptions<PdfSettings> _pdfSettings;
        private readonly IOptions<EmailSettings> _emailSettings;
        private readonly IOptions<FileSettings> _fileSettings;
        private readonly IOptions<CurrencySettings> _currencySettings;
        private readonly IOptions<SmsSettings> _smsSettings;
        private readonly IOptions<LocalizationSettings> _localizationSettings;
        private readonly IOptions<AppSettings> _appSettings;

        public ConfigurationService(
            IConfiguration configuration,
            IOptions<JwtSettings> jwtSettings,
            IOptions<PdfSettings> pdfSettings,
            IOptions<EmailSettings> emailSettings,
            IOptions<FileSettings> fileSettings,
            IOptions<CurrencySettings> currencySettings,
            IOptions<SmsSettings> smsSettings,
            IOptions<LocalizationSettings> localizationSettings,
            IOptions<AppSettings> appSettings)
        {
            _configuration = configuration;
            _jwtSettings = jwtSettings;
            _pdfSettings = pdfSettings;
            _emailSettings = emailSettings;
            _fileSettings = fileSettings;
            _currencySettings = currencySettings;
            _smsSettings = smsSettings;
            _localizationSettings = localizationSettings;
            _appSettings = appSettings;
        }

        public JwtSettings GetJwtSettings()
        {
            return _jwtSettings.Value;
        }

        public PdfSettings GetPdfSettings()
        {
            return _pdfSettings.Value;
        }

        public EmailSettings GetEmailSettings()
        {
            return _emailSettings.Value;
        }

        public FileSettings GetFileSettings()
        {
            return _fileSettings.Value;
        }

        public CurrencySettings GetCurrencySettings()
        {
            return _currencySettings.Value;
        }

        public SmsSettings GetSmsSettings()
        {
            return _smsSettings.Value;
        }

        public LocalizationSettings GetLocalizationSettings()
        {
            return _localizationSettings.Value;
        }

        public AppSettings GetAppSettings()
        {
            return _appSettings.Value;
        }

        public string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name) ?? string.Empty;
        }

        public string? GetValue(string key)
        {
            return _configuration[key];
        }

        public T? GetValue<T>(string key) where T : class
        {
            return _configuration.GetSection(key).Get<T>();
        }
    }
}

