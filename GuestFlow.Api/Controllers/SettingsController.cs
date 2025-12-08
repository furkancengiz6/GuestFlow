using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Operations.Setting.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Sistem ayarları yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")] // Bu controller'a sadece Admin ve Staff rolleri erişebilir.
    [Tags("Ayarlar")]
    public class SettingsController : BaseController
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

        /// <summary>
        /// Bakım modunu açıp kapatır
        /// </summary>
        /// <returns>Bakım modu durumu</returns>
        /// <response code="200">Bakım modu başarıyla değiştirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPatch("maintenance/toggle")]
        [Authorize(Roles = "Admin")] // Sadece Admin bakım modunu değiştirebilir
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ToggleMaintenence()
        {
            try
            {
                _logger.LogInformation("Bakım modu değiştirme isteği alındı.");
                var result = await _settingsService.ToggleMaintenence();
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bakım modu değiştirilirken hata çıktı: {ex.Message}");
                return Error("Bakım modu değiştirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bakım modunun durumunu sorgular
        /// </summary>
        /// <returns>Bakım modu durumu</returns>
        /// <response code="200">Bakım modu durumu başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("maintenance")]
        [Authorize(Roles = "Admin")] // Bu endpoint'e sadece Admin rolü erişebilir.
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMaintenanceState()
        {
            try
            {
                _logger.LogInformation("Bakım modu durumu sorgulandı.");
                var isMaintenance = await _settingsService.GetMaintenanceState();
                return Success(new { MaintenanceMode = isMaintenance }, "Bakım modu durumu başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bakım modu durumu sorgulanırken hata çıktı: {ex.Message}");
                return Error("Bakım modu durumu sorgulanırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm ayarları getirir
        /// </summary>
        /// <returns>Tüm sistem ayarları listesi</returns>
        /// <response code="200">Ayarlar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllSettings()
        {
            try
            {
                var result = await _settingsService.GetAllSettingsAsync();
                return Success(result, "Ayarlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayarlar getirilirken hata oluştu.");
                return Error("Ayarlar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Kategoriye göre ayarları getirir
        /// </summary>
        /// <param name="category">Ayar kategorisi</param>
        /// <returns>Kategoriye göre ayarlar listesi</returns>
        /// <response code="200">Kategoriye göre ayarlar başarıyla getirildi</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("category/{category}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSettingsByCategory(string category)
        {
            try
            {
                var result = await _settingsService.GetSettingsByCategoryAsync(category);
                return Success(result, $"{category} kategorisindeki ayarlar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategoriye göre ayarlar getirilirken hata oluştu: {category}");
                return Error("Ayarlar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Ayarı anahtara göre getirir
        /// </summary>
        /// <param name="key">Ayar anahtarı</param>
        /// <returns>Ayar bilgileri</returns>
        /// <response code="200">Ayar başarıyla getirildi</response>
        /// <response code="404">Ayar bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet("key/{key}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSettingByKey(string key)
        {
            try
            {
                var result = await _settingsService.GetSettingByKeyAsync(key);
                if (result == null)
                    return Error("Ayar bulunamadı.", 404);
                
                return Success(result, "Ayar başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayar getirilirken hata oluştu: {key}");
                return Error("Ayar getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Ayarı günceller
        /// </summary>
        /// <param name="key">Ayar anahtarı</param>
        /// <param name="request">Güncellenecek ayar değeri</param>
        /// <returns>Güncelleme sonucu</returns>
        /// <response code="200">Ayar başarıyla güncellendi</response>
        /// <response code="400">Geçersiz istek verisi</response>
        /// <response code="404">Ayar bulunamadı</response>
        /// <response code="500">Sunucu hatası</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpPut("key/{key}")]
        [Authorize(Roles = "Admin")] // Sadece Admin ayar güncelleyebilir
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _settingsService.UpdateSettingAsync(key, request.Value);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayar güncellenirken hata oluştu: {key}");
                return Error("Ayar güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Birden fazla ayarı günceller
        /// </summary>
        [HttpPut("bulk")]
        [Authorize(Roles = "Admin")] // Sadece Admin ayar güncelleyebilir
        public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, string> settings)
        {
            try
            {
                if (settings == null || settings.Count == 0)
                    return BadRequest(new { Message = "Ayarlar boş olamaz." });

                var result = await _settingsService.UpdateSettingsAsync(settings);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayarlar güncellenirken hata oluştu.");
                return Error("Ayarlar güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Ayar kategorilerini getirir
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetSettingCategories()
        {
            try
            {
                var result = await _settingsService.GetSettingCategoriesAsync();
                return Success(result, "Ayar kategorileri başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayar kategorileri getirilirken hata oluştu.");
                return Error("Ayar kategorileri getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Sistem ayarları özetini getirir
        /// </summary>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSystemSettingsSummary()
        {
            try
            {
                var result = await _settingsService.GetSystemSettingsSummaryAsync();
                return Success(result, "Sistem ayarları özeti başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sistem ayarları özeti getirilirken hata oluştu.");
                return Error("Sistem ayarları özeti getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Ayar güncelleme isteği
    /// </summary>
    public class UpdateSettingRequest
    {
        public string Value { get; set; } = string.Empty;
    }
}