using System;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Setting
{
    public class SettingManager : ISettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SettingEntity> _settingRepository;
        private readonly ILogger<SettingManager> _logger;

        public SettingManager(
            IUnitOfWork unitOfWork,
            IRepository<SettingEntity> settingRepository,
            ILogger<SettingManager> logger)
        {
            _unitOfWork = unitOfWork;
            _settingRepository = settingRepository;
            _logger = logger;
        }

        public async Task<bool> GetMaintenanceState()
        {
            try
            {
                var setting = await _settingRepository.GetAsync(x => x.Id == 1);
                if (setting == null)
                {
                    _logger.LogWarning("Ayar bulunamadı, varsayılan bakım durumu false döndürülüyor.");
                    return false;
                }

                return setting.MainteneceMode; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bakım durumu getirilirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");
                throw new Exception($"Bakım durumu getirilirken hata: {ex.Message}{(ex.InnerException != null ? $" InnerException: {ex.InnerException.Message}" : "")}");
            }
        }

        public async Task<ServiceMessage> ToggleMaintenence()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Ayar kontrolü
                var setting = await _settingRepository.GetAsync(x => x.Id == 1);
                if (setting == null)
                {
                    _logger.LogWarning("Ayar bulunamadı.");
                    return new ServiceMessage { IsSuccess = false, Message = "Ayar bulunamadı." };
                }

                // Bakım durumunu değiştir
                setting.MainteneceMode = !setting.MainteneceMode; 

                await _settingRepository.UpdateAsync(setting);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Bakım durumu güncellendi: {setting.MainteneceMode}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"Bakım durumu başarıyla güncellendi. Yeni durum: {(setting.MainteneceMode ? "Açık" : "Kapalı")}"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Bakım durumu güncellenirken hata: {ex.Message}. InnerException: {ex.InnerException?.Message}");

                string errorMessage = $"Bakım durumu güncellenirken hata: {ex.Message}";
                if (ex.InnerException != null)
                    errorMessage += $" InnerException: {ex.InnerException.Message}";

                return new ServiceMessage { IsSuccess = false, Message = errorMessage };
            }
        }
    }
}