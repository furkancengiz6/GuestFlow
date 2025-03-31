using GuestFlow.Application.Operations.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISettingsService settingsService, ILogger<SettingsController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        [HttpPatch]
        public async Task<IActionResult> ToggleMaintenence()
        {
            try
            {
                _logger.LogInformation("Bakım modu değiştirme isteği alındı.");
                var result = await _settingsService.ToggleMaintenence();

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Bakım modu değiştirme başarısız: {Message}", result.Message);
                    return BadRequest(new { message = result.Message });
                }

                _logger.LogInformation("Bakım modu başarıyla değiştirildi: {Message}", result.Message);
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bakım modu değiştirilirken hata oluştu.");
                return StatusCode(500, new { message = "Bakım modu değiştirilirken bir hata oluştu." });
            }
        }
        [HttpGet("maintenance")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetMaintenanceState()
        {
            try
            {
                _logger.LogInformation("Bakım modu durumu sorgulandı.");
                var isMaintenance = await _settingsService.GetMaintenanceState();
                return Ok(new { maintenanceMode = isMaintenance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bakım modu durumu sorgulanırken hata oluştu.");
                return StatusCode(500, new { message = "Bakım modu durumu sorgulanırken bir hata oluştu." });
            }
        }
    }
}