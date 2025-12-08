using AutoMapper;
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
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public interface IEmailQueueService
    {
        Task<ServiceMessage<EmailQueueDto>> AddToQueueAsync(CreateEmailQueueDto request);
        Task<ServiceMessage<List<EmailQueueDto>>> GetQueueAsync(string? status = null, int? priority = null);
        Task<ServiceMessage<EmailQueueDto?>> GetNextPendingEmailAsync();
        Task<ServiceMessage> UpdateQueueStatusAsync(int queueId, string status, string? errorMessage = null);
        Task<ServiceMessage> RetryFailedEmailsAsync(int maxRetryCount = 3);
        Task<ServiceMessage> ClearOldQueueItemsAsync(int daysOld = 30);
    }

    public class EmailQueueService : IEmailQueueService
    {
        private readonly IRepository<EmailQueueEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailQueueService> _logger;
        private readonly IMapper _mapper;

        public EmailQueueService(IRepository<EmailQueueEntity> repository, IUnitOfWork unitOfWork, ILogger<EmailQueueService> logger, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ServiceMessage<EmailQueueDto>> AddToQueueAsync(CreateEmailQueueDto request)
        {
            try
            {
                var emailQueue = new EmailQueueEntity
                {
                    To = request.To,
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = request.IsHtml,
                    Priority = request.Priority,
                    TemplateName = request.TemplateName,
                    TemplateVariables = request.TemplateVariables != null 
                        ? JsonSerializer.Serialize(request.TemplateVariables) 
                        : null,
                    Attachments = request.Attachments != null 
                        ? string.Join(",", request.Attachments) 
                        : null,
                    RelatedEntityType = request.RelatedEntityType,
                    RelatedEntityId = request.RelatedEntityId,
                    ScheduledDate = request.ScheduledDate,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.AddAsync(emailQueue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta kuyruğa eklendi: {request.To}, Konu: {request.Subject}");

                var dto = _mapper.Map<EmailQueueDto>(emailQueue);
                return new ServiceMessage<EmailQueueDto>
                {
                    IsSuccess = true,
                    Message = "E-posta kuyruğa başarıyla eklendi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta kuyruğa eklenirken hata: {ex.Message}");
                return new ServiceMessage<EmailQueueDto>
                {
                    IsSuccess = false,
                    Message = $"E-posta kuyruğa eklenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<List<EmailQueueDto>>> GetQueueAsync(string? status = null, int? priority = null)
        {
            try
            {
                var query = _repository.GetAll();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(e => e.Status == status);

                if (priority.HasValue)
                    query = query.Where(e => e.Priority == priority.Value);

                var emails = await query
                    .OrderByDescending(e => e.Priority)
                    .ThenBy(e => e.CreatedDate)
                    .ToListAsync();

                var dtos = _mapper.Map<List<EmailQueueDto>>(emails);

                return new ServiceMessage<List<EmailQueueDto>>
                {
                    IsSuccess = true,
                    Message = "E-posta kuyruğu başarıyla getirildi.",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta kuyruğu getirilirken hata: {ex.Message}");
                return new ServiceMessage<List<EmailQueueDto>>
                {
                    IsSuccess = false,
                    Message = $"E-posta kuyruğu getirilirken hata oluştu: {ex.Message}",
                    Data = new List<EmailQueueDto>()
                };
            }
        }

        public async Task<ServiceMessage<EmailQueueDto?>> GetNextPendingEmailAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var email = await _repository
                    .GetAll()
                    .Where(e => e.Status == "Pending" 
                        && (!e.ScheduledDate.HasValue || e.ScheduledDate.Value <= now))
                    .OrderByDescending(e => e.Priority)
                    .ThenBy(e => e.CreatedDate)
                    .FirstOrDefaultAsync();

                if (email == null)
                {
                    return new ServiceMessage<EmailQueueDto?>
                    {
                        IsSuccess = true,
                        Message = "Bekleyen e-posta bulunamadı.",
                        Data = null
                    };
                }

                // Durumu Processing olarak güncelle
                email.Status = "Processing";
                email.LastAttemptDate = now;
                await _unitOfWork.SaveChangesAsync();

                var dto = _mapper.Map<EmailQueueDto>(email);
                return new ServiceMessage<EmailQueueDto?>
                {
                    IsSuccess = true,
                    Message = "Bekleyen e-posta getirildi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bekleyen e-posta getirilirken hata: {ex.Message}");
                return new ServiceMessage<EmailQueueDto?>
                {
                    IsSuccess = false,
                    Message = $"Bekleyen e-posta getirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> UpdateQueueStatusAsync(int queueId, string status, string? errorMessage = null)
        {
            try
            {
                var email = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == queueId);

                if (email == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "E-posta kuyruk kaydı bulunamadı."
                    };
                }

                email.Status = status;
                email.LastAttemptDate = DateTime.UtcNow;

                if (status == "Sent")
                {
                    email.SentDate = DateTime.UtcNow;
                }
                else if (status == "Failed")
                {
                    email.RetryCount++;
                    email.ErrorMessage = errorMessage;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta kuyruk durumu güncellendi: {queueId}, Durum: {status}");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "E-posta kuyruk durumu başarıyla güncellendi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta kuyruk durumu güncellenirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"E-posta kuyruk durumu güncellenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> RetryFailedEmailsAsync(int maxRetryCount = 3)
        {
            try
            {
                var failedEmails = await _repository
                    .GetAll()
                    .Where(e => e.Status == "Failed" 
                        && e.RetryCount < e.MaxRetryCount
                        && e.RetryCount < maxRetryCount)
                    .ToListAsync();

                foreach (var email in failedEmails)
                {
                    email.Status = "Pending";
                    email.ErrorMessage = null;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"{failedEmails.Count} başarısız e-posta tekrar deneme için kuyruğa eklendi.");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"{failedEmails.Count} başarısız e-posta tekrar deneme için kuyruğa eklendi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Başarısız e-postalar tekrar deneme için eklenirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Başarısız e-postalar tekrar deneme için eklenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> ClearOldQueueItemsAsync(int daysOld = 30)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var oldEmails = await _repository
                    .GetAll()
                    .Where(e => (e.Status == "Sent" || e.Status == "Failed") 
                        && e.SentDate.HasValue 
                        && e.SentDate.Value < cutoffDate)
                    .ToListAsync();

                foreach (var email in oldEmails)
                {
                    email.IsDeleted = true;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"{oldEmails.Count} eski e-posta kuyruk kaydı silindi.");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"{oldEmails.Count} eski e-posta kuyruk kaydı silindi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eski e-posta kuyruk kayıtları silinirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Eski e-posta kuyruk kayıtları silinirken hata oluştu: {ex.Message}"
                };
            }
        }

    }
}

