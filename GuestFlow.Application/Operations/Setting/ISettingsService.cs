using GuestFlow.Application.Operations.Setting.Dtos;
using GuestFlow.Application.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Setting
{
    public interface ISettingsService
    {
        Task<ServiceMessage> ToggleMaintenence();
        Task<bool> GetMaintenanceState();
        
        /// <summary>
        /// Tüm ayarları getirir
        /// </summary>
        Task<List<SettingDto>> GetAllSettingsAsync();
        
        /// <summary>
        /// Kategoriye göre ayarları getirir
        /// </summary>
        Task<List<SettingDto>> GetSettingsByCategoryAsync(string category);
        
        /// <summary>
        /// Ayarı anahtara göre getirir
        /// </summary>
        Task<SettingDto?> GetSettingByKeyAsync(string key);
        
        /// <summary>
        /// Ayarı günceller
        /// </summary>
        Task<ServiceMessage> UpdateSettingAsync(string key, string value);
        
        /// <summary>
        /// Birden fazla ayarı günceller
        /// </summary>
        Task<ServiceMessage> UpdateSettingsAsync(Dictionary<string, string> settings);
        
        /// <summary>
        /// Ayar kategorilerini getirir
        /// </summary>
        Task<List<SettingCategoryDto>> GetSettingCategoriesAsync();
        
        /// <summary>
        /// Sistem ayarları özetini getirir
        /// </summary>
        Task<SystemSettingsSummaryDto> GetSystemSettingsSummaryAsync();
    }
}
