using GuestFlow.Application.Operations.Email.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public interface IEmailTemplateService
    {
        Task<ServiceMessage<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateDto request);
        Task<ServiceMessage<EmailTemplateDto?>> GetTemplateAsync(int id);
        Task<ServiceMessage<EmailTemplateDto?>> GetTemplateByNameAsync(string name);
        Task<ServiceMessage<List<EmailTemplateDto>>> GetTemplatesAsync(string? category = null, bool? isActive = null);
        Task<ServiceMessage<EmailTemplateDto>> UpdateTemplateAsync(int id, CreateEmailTemplateDto request);
        Task<ServiceMessage> DeleteTemplateAsync(int id);
        Task<ServiceMessage<string>> RenderTemplateAsync(string templateName, Dictionary<string, string> variables);
        Task<ServiceMessage<string>> RenderTemplateBodyAsync(int templateId, Dictionary<string, string> variables);
    }

    public class EmailTemplateService : IEmailTemplateService
    {
        private readonly IRepository<EmailTemplateEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailTemplateService> _logger;

        public EmailTemplateService(IRepository<EmailTemplateEntity> repository, IUnitOfWork unitOfWork, ILogger<EmailTemplateService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceMessage<EmailTemplateDto>> CreateTemplateAsync(CreateEmailTemplateDto request)
        {
            try
            {
                // Aynı isimde şablon var mı kontrol et
                var existing = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t => t.Name == request.Name);

                if (existing != null)
                {
                    return new ServiceMessage<EmailTemplateDto>
                    {
                        IsSuccess = false,
                        Message = "Bu isimde bir şablon zaten mevcut."
                    };
                }

                var template = new EmailTemplateEntity
                {
                    Name = request.Name,
                    Title = request.Title,
                    Subject = request.Subject,
                    Body = request.Body,
                    Category = request.Category,
                    VariablesDescription = request.VariablesDescription != null
                        ? JsonSerializer.Serialize(request.VariablesDescription)
                        : null,
                    IsActive = request.IsActive,
                    IsDefault = false,
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.AddAsync(template);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta şablonu oluşturuldu: {request.Name}");

                var dto = MapToDto(template);
                return new ServiceMessage<EmailTemplateDto>
                {
                    IsSuccess = true,
                    Message = "E-posta şablonu başarıyla oluşturuldu.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonu oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<EmailTemplateDto>
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonu oluşturulurken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<EmailTemplateDto?>> GetTemplateAsync(int id)
        {
            try
            {
                var template = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (template == null)
                {
                    return new ServiceMessage<EmailTemplateDto?>
                    {
                        IsSuccess = false,
                        Message = "E-posta şablonu bulunamadı.",
                        Data = null
                    };
                }

                var dto = MapToDto(template);
                return new ServiceMessage<EmailTemplateDto?>
                {
                    IsSuccess = true,
                    Message = "E-posta şablonu başarıyla getirildi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonu getirilirken hata: {ex.Message}");
                return new ServiceMessage<EmailTemplateDto?>
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonu getirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<EmailTemplateDto?>> GetTemplateByNameAsync(string name)
        {
            try
            {
                var template = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t => t.Name == name && t.IsActive);

                if (template == null)
                {
                    return new ServiceMessage<EmailTemplateDto?>
                    {
                        IsSuccess = false,
                        Message = "E-posta şablonu bulunamadı.",
                        Data = null
                    };
                }

                var dto = MapToDto(template);
                return new ServiceMessage<EmailTemplateDto?>
                {
                    IsSuccess = true,
                    Message = "E-posta şablonu başarıyla getirildi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonu getirilirken hata: {ex.Message}");
                return new ServiceMessage<EmailTemplateDto?>
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonu getirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<List<EmailTemplateDto>>> GetTemplatesAsync(string? category = null, bool? isActive = null)
        {
            try
            {
                var query = _repository.GetAll();

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(t => t.Category == category);

                if (isActive.HasValue)
                    query = query.Where(t => t.IsActive == isActive.Value);

                var templates = await query
                    .OrderBy(t => t.Category)
                    .ThenBy(t => t.Name)
                    .ToListAsync();

                var dtos = templates.Select(MapToDto).ToList();

                return new ServiceMessage<List<EmailTemplateDto>>
                {
                    IsSuccess = true,
                    Message = "E-posta şablonları başarıyla getirildi.",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonları getirilirken hata: {ex.Message}");
                return new ServiceMessage<List<EmailTemplateDto>>
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonları getirilirken hata oluştu: {ex.Message}",
                    Data = new List<EmailTemplateDto>()
                };
            }
        }

        public async Task<ServiceMessage<EmailTemplateDto>> UpdateTemplateAsync(int id, CreateEmailTemplateDto request)
        {
            try
            {
                var template = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (template == null)
                {
                    return new ServiceMessage<EmailTemplateDto>
                    {
                        IsSuccess = false,
                        Message = "E-posta şablonu bulunamadı."
                    };
                }

                if (template.IsDefault)
                {
                    return new ServiceMessage<EmailTemplateDto>
                    {
                        IsSuccess = false,
                        Message = "Varsayılan şablonlar güncellenemez."
                    };
                }

                // İsim değişiyorsa, yeni ismin benzersiz olduğunu kontrol et
                if (template.Name != request.Name)
                {
                    var existing = await _repository
                        .GetAll()
                        .FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != id);

                    if (existing != null)
                    {
                        return new ServiceMessage<EmailTemplateDto>
                        {
                            IsSuccess = false,
                            Message = "Bu isimde bir şablon zaten mevcut."
                        };
                    }
                }

                template.Name = request.Name;
                template.Title = request.Title;
                template.Subject = request.Subject;
                template.Body = request.Body;
                template.Category = request.Category;
                template.VariablesDescription = request.VariablesDescription != null
                    ? JsonSerializer.Serialize(request.VariablesDescription)
                    : null;
                template.IsActive = request.IsActive;
                template.LastModifiedDate = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta şablonu güncellendi: {request.Name}");

                var dto = MapToDto(template);
                return new ServiceMessage<EmailTemplateDto>
                {
                    IsSuccess = true,
                    Message = "E-posta şablonu başarıyla güncellendi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonu güncellenirken hata: {ex.Message}");
                return new ServiceMessage<EmailTemplateDto>
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonu güncellenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> DeleteTemplateAsync(int id)
        {
            try
            {
                var template = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (template == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "E-posta şablonu bulunamadı."
                    };
                }

                if (template.IsDefault)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Varsayılan şablonlar silinemez."
                    };
                }

                template.IsDeleted = true;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta şablonu silindi: {template.Name}");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "E-posta şablonu başarıyla silindi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta şablonu silinirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"E-posta şablonu silinirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<string>> RenderTemplateAsync(string templateName, Dictionary<string, string> variables)
        {
            try
            {
                var templateResult = await GetTemplateByNameAsync(templateName);
                if (!templateResult.IsSuccess || templateResult.Data == null)
                {
                    return new ServiceMessage<string>
                    {
                        IsSuccess = false,
                        Message = "Şablon bulunamadı."
                    };
                }

                var rendered = RenderString(templateResult.Data.Body, variables);
                return new ServiceMessage<string>
                {
                    IsSuccess = true,
                    Message = "Şablon başarıyla render edildi.",
                    Data = rendered
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şablon render edilirken hata: {ex.Message}");
                return new ServiceMessage<string>
                {
                    IsSuccess = false,
                    Message = $"Şablon render edilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<string>> RenderTemplateBodyAsync(int templateId, Dictionary<string, string> variables)
        {
            try
            {
                var templateResult = await GetTemplateAsync(templateId);
                if (!templateResult.IsSuccess || templateResult.Data == null)
                {
                    return new ServiceMessage<string>
                    {
                        IsSuccess = false,
                        Message = "Şablon bulunamadı."
                    };
                }

                var rendered = RenderString(templateResult.Data.Body, variables);
                return new ServiceMessage<string>
                {
                    IsSuccess = true,
                    Message = "Şablon başarıyla render edildi.",
                    Data = rendered
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şablon render edilirken hata: {ex.Message}");
                return new ServiceMessage<string>
                {
                    IsSuccess = false,
                    Message = $"Şablon render edilirken hata oluştu: {ex.Message}"
                };
            }
        }

        private string RenderString(string template, Dictionary<string, string> variables)
        {
            if (variables == null || variables.Count == 0)
                return template;

            var result = template;
            foreach (var variable in variables)
            {
                var pattern = $"{{{{ {variable.Key} }}}}";
                result = result.Replace(pattern, variable.Value);
            }

            return result;
        }

        private EmailTemplateDto MapToDto(EmailTemplateEntity entity)
        {
            return new EmailTemplateDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Title = entity.Title,
                Subject = entity.Subject,
                Body = entity.Body,
                Category = entity.Category,
                VariablesDescription = !string.IsNullOrEmpty(entity.VariablesDescription)
                    ? JsonSerializer.Deserialize<Dictionary<string, string>>(entity.VariablesDescription)
                    : null,
                IsActive = entity.IsActive,
                IsDefault = entity.IsDefault,
                LastModifiedDate = entity.LastModifiedDate,
                ModifiedByPersonnelId = entity.ModifiedByPersonnelId,
                CreatedDate = entity.CreatedDate
            };
        }
    }
}

