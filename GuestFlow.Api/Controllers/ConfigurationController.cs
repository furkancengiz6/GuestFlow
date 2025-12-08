using GuestFlow.Application.Operations.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ConfigurationController : BaseController
    {
        private readonly IConfigurationService _configurationService;

        public ConfigurationController(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        /// <summary>
        /// Tüm konfigürasyon ayarlarını getirir (sadece okuma)
        /// </summary>
        [HttpGet]
        public IActionResult GetConfiguration()
        {
            try
            {
                var config = new
                {
                    Jwt = _configurationService.GetJwtSettings(),
                    Pdf = _configurationService.GetPdfSettings(),
                    Email = new
                    {
                        SmtpHost = _configurationService.GetEmailSettings().SmtpHost,
                        SmtpPort = _configurationService.GetEmailSettings().SmtpPort,
                        FromEmail = _configurationService.GetEmailSettings().FromEmail,
                        FromName = _configurationService.GetEmailSettings().FromName,
                        UseSsl = _configurationService.GetEmailSettings().UseSsl,
                        BaseUrl = _configurationService.GetEmailSettings().BaseUrl,
                        Enabled = _configurationService.GetEmailSettings().Enabled
                        // Hassas bilgiler (şifreler) gösterilmez
                    },
                    File = _configurationService.GetFileSettings(),
                    Currency = _configurationService.GetCurrencySettings(),
                    Sms = new
                    {
                        Enabled = _configurationService.GetSmsSettings().Enabled,
                        Provider = _configurationService.GetSmsSettings().Provider,
                        SenderName = _configurationService.GetSmsSettings().SenderName
                        // Hassas bilgiler (API key/secret) gösterilmez
                    },
                    Localization = _configurationService.GetLocalizationSettings(),
                    App = _configurationService.GetAppSettings()
                };

                return Success(config, "Konfigürasyon ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"Konfigürasyon ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// JWT ayarlarını getirir
        /// </summary>
        [HttpGet("jwt")]
        public IActionResult GetJwtSettings()
        {
            try
            {
                var settings = _configurationService.GetJwtSettings();
                return Success(settings, "JWT ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"JWT ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// PDF ayarlarını getirir
        /// </summary>
        [HttpGet("pdf")]
        public IActionResult GetPdfSettings()
        {
            try
            {
                var settings = _configurationService.GetPdfSettings();
                return Success(settings, "PDF ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"PDF ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// E-posta ayarlarını getirir (hassas bilgiler hariç)
        /// </summary>
        [HttpGet("email")]
        public IActionResult GetEmailSettings()
        {
            try
            {
                var settings = _configurationService.GetEmailSettings();
                var safeSettings = new
                {
                    SmtpHost = settings.SmtpHost,
                    SmtpPort = settings.SmtpPort,
                    FromEmail = settings.FromEmail,
                    FromName = settings.FromName,
                    UseSsl = settings.UseSsl,
                    BaseUrl = settings.BaseUrl,
                    Enabled = settings.Enabled
                };
                return Success(safeSettings, "E-posta ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"E-posta ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Dosya ayarlarını getirir
        /// </summary>
        [HttpGet("file")]
        public IActionResult GetFileSettings()
        {
            try
            {
                var settings = _configurationService.GetFileSettings();
                return Success(settings, "Dosya ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"Dosya ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Para birimi ayarlarını getirir
        /// </summary>
        [HttpGet("currency")]
        public IActionResult GetCurrencySettings()
        {
            try
            {
                var settings = _configurationService.GetCurrencySettings();
                return Success(settings, "Para birimi ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"Para birimi ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// SMS ayarlarını getirir (hassas bilgiler hariç)
        /// </summary>
        [HttpGet("sms")]
        public IActionResult GetSmsSettings()
        {
            try
            {
                var settings = _configurationService.GetSmsSettings();
                var safeSettings = new
                {
                    Enabled = settings.Enabled,
                    Provider = settings.Provider,
                    SenderName = settings.SenderName
                };
                return Success(safeSettings, "SMS ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"SMS ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Yerelleştirme ayarlarını getirir
        /// </summary>
        [HttpGet("localization")]
        public IActionResult GetLocalizationSettings()
        {
            try
            {
                var settings = _configurationService.GetLocalizationSettings();
                return Success(settings, "Yerelleştirme ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"Yerelleştirme ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }

        /// <summary>
        /// Uygulama ayarlarını getirir
        /// </summary>
        [HttpGet("app")]
        public IActionResult GetAppSettings()
        {
            try
            {
                var settings = _configurationService.GetAppSettings();
                return Success(settings, "Uygulama ayarları başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return Error($"Uygulama ayarları getirilirken hata oluştu: {ex.Message}", 500);
            }
        }
    }
}

