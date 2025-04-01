using GuestFlow.Application.Operations.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Bu controller'a sadece Admin ve Staff rolleri erişebilir.
    public class SettingsController : ControllerBase
    {
        // Burada kullanacağım değişkenleri tanımlıyorum.
        // _settingsService: Ayarlarla ilgili işlemleri yapmak için kullanıyorum.
        // _logger: Hataları veya bilgileri loglamak için kullanıyorum.
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _logger;

        // Constructor: Bu sınıf oluşturulurken bağımlılıkları buradan alıyorum.
        public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        // Bu metodumla bakım modunu açıp kapatıyorum.
        [HttpPatch]
        public async Task<IActionResult> ToggleMaintenence()
        {
            try
            {
                // Bakım modu değiştirme isteğini logluyorum.
                _logger.LogInformation("Bakım modu değiştirme isteği alındı.");
                // Servisten bakım modunu değiştirme işlemini yapıyorum.
                var result = await _settingsService.ToggleMaintenence();

                if (!result.IsSuccess)
                {
                    // Eğer işlem başarısızsa, bunu logluyorum ve hata mesajı döndürüyorum.
                    _logger.LogWarning($"Bakım modu değiştirme başarısız: {result.Message}");
                    return BadRequest(new { Message = result.Message });
                }

                // Eğer işlem başarılıysa, bunu logluyorum ve başarı mesajı döndürüyorum.
                _logger.LogInformation($"Bakım modu başarıyla değiştirildi: {result.Message}");
                return Ok(new { Message = result.Message });
            }
            catch (Exception ex)
            {
                // Eğer bir hata çıkarsa, bunu logluyorum ve 500 hata koduyla hata mesajı döndürüyorum.
                _logger.LogError(ex, $"Bakım modu değiştirilirken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                return StatusCode(500, new { Message = "Bakım modu değiştirilirken bir hata oluştu." });
            }
        }

        // Bu metodumla bakım modunun durumunu sorguluyorum.
        [HttpGet("maintenance")]
        [Authorize(Roles = "Admin")] // Bu endpoint'e sadece Admin rolü erişebilir.
        public async Task<IActionResult> GetMaintenanceState()
        {
            try
            {
                // Bakım modu durumunun sorgulandığını logluyorum.
                _logger.LogInformation("Bakım modu durumu sorgulandı.");
                // Servisten bakım modu durumunu alıyorum.
                var isMaintenance = await _settingsService.GetMaintenanceState();
                // Durumu JSON formatında döndürüyorum.
                return Ok(new { MaintenanceMode = isMaintenance });
            }
            catch (Exception ex)
            {
                // Eğer bir hata çıkarsa, bunu logluyorum ve 500 hata koduyla hata mesajı döndürüyorum.
                _logger.LogError(ex, $"Bakım modu durumu sorgulanırken hata çıktı: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                return StatusCode(500, new { Message = "Bakım modu durumu sorgulanırken bir hata oluştu." });
            }
        }
    }
}