using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Setting;
using GuestFlow.Application.Operations.Setting.Dtos;
using GuestFlow.Application.Operations.Cache;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Setting
{
    public class SettingManager : ISettingsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<SettingEntity> _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SettingManager> _logger;

        public SettingManager(
            IUnitOfWork unitOfWork,
            IRepository<SettingEntity> settingRepository,
            IConfiguration configuration,
            ILogger<SettingManager> logger)
        {
            _unitOfWork = unitOfWork;
            _settingRepository = settingRepository;
            _configuration = configuration;
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

        public async Task<List<SettingDto>> GetAllSettingsAsync()
        {
            try
            {
                var settings = new List<SettingDto>();

                // Bakım modu
                var maintenanceMode = await GetMaintenanceState();
                settings.Add(new SettingDto
                {
                    Id = 1,
                    Key = "MaintenanceMode",
                    Value = maintenanceMode.ToString(),
                    Category = "System",
                    Description = "Bakım modu durumu",
                    DataType = "bool"
                });

                // E-posta ayarları
                settings.Add(new SettingDto
                {
                    Key = "EmailSettings:Enabled",
                    Value = _configuration["EmailSettings:Enabled"] ?? "false",
                    Category = "Email",
                    Description = "E-posta servisi aktif mi?",
                    DataType = "bool"
                });
                settings.Add(new SettingDto
                {
                    Key = "EmailSettings:SmtpHost",
                    Value = _configuration["EmailSettings:SmtpHost"] ?? "",
                    Category = "Email",
                    Description = "SMTP sunucu adresi",
                    DataType = "string"
                });
                settings.Add(new SettingDto
                {
                    Key = "EmailSettings:SmtpPort",
                    Value = _configuration["EmailSettings:SmtpPort"] ?? "587",
                    Category = "Email",
                    Description = "SMTP port numarası",
                    DataType = "int"
                });
                settings.Add(new SettingDto
                {
                    Key = "EmailSettings:FromEmail",
                    Value = _configuration["EmailSettings:FromEmail"] ?? "",
                    Category = "Email",
                    Description = "Gönderen e-posta adresi",
                    DataType = "string"
                });

                // PDF ayarları
                settings.Add(new SettingDto
                {
                    Key = "PdfSettings:OutputPath",
                    Value = _configuration["PdfSettings:OutputPath"] ?? "wwwroot/pdfs",
                    Category = "Pdf",
                    Description = "PDF çıktı klasörü",
                    DataType = "string"
                });

                // Para birimi ayarları
                var defaultCurrency = _configuration["CurrencySettings:DefaultCurrency"] ?? "TRY";
                settings.Add(new SettingDto
                {
                    Key = "CurrencySettings:DefaultCurrency",
                    Value = defaultCurrency,
                    Category = "Currency",
                    Description = "Varsayılan para birimi",
                    DataType = "string"
                });

                // JWT ayarları
                settings.Add(new SettingDto
                {
                    Key = "Jwt:ExpireMinutes",
                    Value = _configuration["Jwt:ExpireMinutes"] ?? "45",
                    Category = "System",
                    Description = "JWT token geçerlilik süresi (dakika)",
                    DataType = "int"
                });

                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayarlar getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<List<SettingDto>> GetSettingsByCategoryAsync(string category)
        {
            try
            {
                var allSettings = await GetAllSettingsAsync();
                return allSettings.Where(s => s.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kategoriye göre ayarlar getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<SettingDto?> GetSettingByKeyAsync(string key)
        {
            try
            {
                var allSettings = await GetAllSettingsAsync();
                return allSettings.FirstOrDefault(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayar getirilirken hata: {ex.Message}. Key: {key}");
                throw;
            }
        }

        public async Task<ServiceMessage> UpdateSettingAsync(string key, string value)
        {
            try
            {
                // Bakım modu özel durumu
                if (key.Equals("MaintenanceMode", StringComparison.OrdinalIgnoreCase))
                {
                    var currentState = await GetMaintenanceState();
                    var newState = bool.Parse(value);
                    if (currentState != newState)
                    {
                        return await ToggleMaintenence();
                    }
                    return new ServiceMessage { IsSuccess = true, Message = "Ayar zaten bu değerde." };
                }

                // Diğer ayarlar için Configuration güncellemesi yapılamaz (read-only)
                // Bu durumda appsettings.json dosyasını güncellemek gerekir
                // Bu özellik için ayrı bir servis veya dosya yönetimi gerekir
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Bu ayar şu an için yalnızca okunabilir. Güncelleme için appsettings.json dosyasını düzenleyin."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayar güncellenirken hata: {ex.Message}. Key: {key}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ayar güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<ServiceMessage> UpdateSettingsAsync(Dictionary<string, string> settings)
        {
            try
            {
                var results = new List<string>();
                foreach (var setting in settings)
                {
                    var result = await UpdateSettingAsync(setting.Key, setting.Value);
                    if (result.IsSuccess)
                    {
                        results.Add($"{setting.Key}: Güncellendi");
                    }
                    else
                    {
                        results.Add($"{setting.Key}: {result.Message}");
                    }
                }

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"Ayarlar güncellendi: {string.Join(", ", results)}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayarlar güncellenirken hata: {ex.Message}");
                return new ServiceMessage { IsSuccess = false, Message = $"Ayarlar güncellenirken hata: {ex.Message}" };
            }
        }

        public async Task<List<SettingCategoryDto>> GetSettingCategoriesAsync()
        {
            try
            {
                var allSettings = await GetAllSettingsAsync();
                var categories = allSettings
                    .GroupBy(s => s.Category)
                    .Select(g => new SettingCategoryDto
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Settings = g.ToList()
                    })
                    .ToList();

                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ayar kategorileri getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<SystemSettingsSummaryDto> GetSystemSettingsSummaryAsync()
        {
            try
            {
                var maintenanceMode = await GetMaintenanceState();
                var defaultCurrency = _configuration["CurrencySettings:DefaultCurrency"] ?? "TRY";
                var emailSmtpHost = _configuration["EmailSettings:SmtpHost"];
                var emailEnabled = bool.Parse(_configuration["EmailSettings:Enabled"] ?? "false");
                var pdfOutputPath = _configuration["PdfSettings:OutputPath"] ?? "wwwroot/pdfs";

                var allSettings = await GetAllSettingsAsync();
                var settingsDict = allSettings.ToDictionary(s => s.Key, s => s.Value);

                return new SystemSettingsSummaryDto
                {
                    MaintenanceMode = maintenanceMode,
                    DefaultCurrency = defaultCurrency,
                    EmailSmtpHost = emailSmtpHost,
                    EmailEnabled = emailEnabled,
                    PdfOutputPath = pdfOutputPath,
                    AllSettings = settingsDict
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sistem ayarları özeti getirilirken hata: {ex.Message}");
                throw;
            }
        }
    }
}