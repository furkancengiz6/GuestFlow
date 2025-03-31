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

                return setting.MainteneceMode; // Yazım hatası düzeltildi: MainteneceMode -> MaintenanceMode
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bakım durumu getirilirken hata oluştu.");
                throw new Exception("Bakım durumu getirilirken bir hata ile karşılaşıldı: " + ex.Message);
            }
        }

        public async Task<ServiceMessage> ToggleMaintenence()
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var setting = await _settingRepository.GetAsync(x => x.Id == 1);
                if (setting == null)
                {
                    _logger.LogWarning("Ayar bulunamadı.");
                    return new ServiceMessage { IsSuccess = false, Message = "Ayar bulunamadı." };
                }

                setting.MainteneceMode = !setting.MainteneceMode; // Yazım hatası düzeltildi: MainteneceMode -> MaintenanceMode

                await _settingRepository.UpdateAsync(setting);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Bakım durumu güncellendi: {MaintenanceMode}", setting.MainteneceMode);
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"Bakım durumu başarıyla güncellendi. Yeni durum: {(setting.MainteneceMode ? "Açık" : "Kapalı")}"
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Bakım durumu güncellenirken hata oluştu.");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Bakım durumu güncellenirken bir hata ile karşılaşıldı: " + ex.Message
                };
            }
        }
    }
}