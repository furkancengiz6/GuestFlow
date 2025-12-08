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
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Email
{
    public interface IEmailHistoryService
    {
        Task<ServiceMessage<EmailHistoryDto>> SaveEmailHistoryAsync(string to, string from, string subject, string status, string? errorMessage = null, string? templateName = null, string? relatedEntityType = null, int? relatedEntityId = null, long? emailSize = null, int attachmentCount = 0, string? smtpResponse = null);
        Task<ServiceMessage<List<EmailHistoryDto>>> GetEmailHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null, string? to = null, string? templateName = null, int pageNumber = 1, int pageSize = 50);
        Task<ServiceMessage<EmailHistoryDto?>> GetEmailHistoryByIdAsync(int id);
        Task<ServiceMessage> MarkEmailAsOpenedAsync(int id);
        Task<ServiceMessage> IncrementClickCountAsync(int id);
        Task<ServiceMessage> ClearOldHistoryAsync(int daysOld = 90);
    }

    public class EmailHistoryService : IEmailHistoryService
    {
        private readonly IRepository<EmailHistoryEntity> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmailHistoryService> _logger;
        private readonly IMapper _mapper;

        public EmailHistoryService(IRepository<EmailHistoryEntity> repository, IUnitOfWork unitOfWork, ILogger<EmailHistoryService> logger, IMapper mapper)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ServiceMessage<EmailHistoryDto>> SaveEmailHistoryAsync(string to, string from, string subject, string status, string? errorMessage = null, string? templateName = null, string? relatedEntityType = null, int? relatedEntityId = null, long? emailSize = null, int attachmentCount = 0, string? smtpResponse = null)
        {
            try
            {
                var history = new EmailHistoryEntity
                {
                    To = to,
                    From = from,
                    Subject = subject,
                    Status = status,
                    SentDate = DateTime.UtcNow,
                    ErrorMessage = errorMessage,
                    TemplateName = templateName,
                    RelatedEntityType = relatedEntityType,
                    RelatedEntityId = relatedEntityId,
                    EmailSize = emailSize,
                    AttachmentCount = attachmentCount,
                    SmtpResponse = smtpResponse,
                    CreatedDate = DateTime.UtcNow
                };

                await _repository.AddAsync(history);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta geçmişi kaydedildi: {to}, Durum: {status}");

                var dto = _mapper.Map<EmailHistoryDto>(history);
                return new ServiceMessage<EmailHistoryDto>
                {
                    IsSuccess = true,
                    Message = "E-posta geçmişi başarıyla kaydedildi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta geçmişi kaydedilirken hata: {ex.Message}");
                return new ServiceMessage<EmailHistoryDto>
                {
                    IsSuccess = false,
                    Message = $"E-posta geçmişi kaydedilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage<List<EmailHistoryDto>>> GetEmailHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, string? status = null, string? to = null, string? templateName = null, int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                var query = _repository.GetAll();

                if (startDate.HasValue)
                    query = query.Where(e => e.SentDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(e => e.SentDate <= endDate.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(e => e.Status == status);

                if (!string.IsNullOrEmpty(to))
                    query = query.Where(e => e.To.Contains(to));

                if (!string.IsNullOrEmpty(templateName))
                    query = query.Where(e => e.TemplateName == templateName);

                var totalCount = await query.CountAsync();

                var emails = await query
                    .OrderByDescending(e => e.SentDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<EmailHistoryDto>>(emails);

                return new ServiceMessage<List<EmailHistoryDto>>
                {
                    IsSuccess = true,
                    Message = $"E-posta geçmişi başarıyla getirildi. Toplam: {totalCount}",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta geçmişi getirilirken hata: {ex.Message}");
                return new ServiceMessage<List<EmailHistoryDto>>
                {
                    IsSuccess = false,
                    Message = $"E-posta geçmişi getirilirken hata oluştu: {ex.Message}",
                    Data = new List<EmailHistoryDto>()
                };
            }
        }

        public async Task<ServiceMessage<EmailHistoryDto?>> GetEmailHistoryByIdAsync(int id)
        {
            try
            {
                var history = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (history == null)
                {
                    return new ServiceMessage<EmailHistoryDto?>
                    {
                        IsSuccess = false,
                        Message = "E-posta geçmişi bulunamadı.",
                        Data = null
                    };
                }

                var dto = _mapper.Map<EmailHistoryDto>(history);
                return new ServiceMessage<EmailHistoryDto?>
                {
                    IsSuccess = true,
                    Message = "E-posta geçmişi başarıyla getirildi.",
                    Data = dto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta geçmişi getirilirken hata: {ex.Message}");
                return new ServiceMessage<EmailHistoryDto?>
                {
                    IsSuccess = false,
                    Message = $"E-posta geçmişi getirilirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> MarkEmailAsOpenedAsync(int id)
        {
            try
            {
                var history = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (history == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "E-posta geçmişi bulunamadı."
                    };
                }

                if (!history.IsOpened)
                {
                    history.IsOpened = true;
                    history.OpenedDate = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation($"E-posta açıldı olarak işaretlendi: {id}");
                }

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "E-posta açıldı olarak işaretlendi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta açıldı işaretlenirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"E-posta açıldı işaretlenirken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> IncrementClickCountAsync(int id)
        {
            try
            {
                var history = await _repository
                    .GetAll()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (history == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "E-posta geçmişi bulunamadı."
                    };
                }

                history.ClickCount++;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"E-posta tıklama sayısı artırıldı: {id}");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "E-posta tıklama sayısı artırıldı."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta tıklama sayısı artırılırken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"E-posta tıklama sayısı artırılırken hata oluştu: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> ClearOldHistoryAsync(int daysOld = 90)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
                var oldEmails = await _repository
                    .GetAll()
                    .Where(e => e.SentDate < cutoffDate)
                    .ToListAsync();

                foreach (var email in oldEmails)
                {
                    email.IsDeleted = true;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"{oldEmails.Count} eski e-posta geçmişi kaydı silindi.");

                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = $"{oldEmails.Count} eski e-posta geçmişi kaydı silindi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Eski e-posta geçmişi kayıtları silinirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Eski e-posta geçmişi kayıtları silinirken hata oluştu: {ex.Message}"
                };
            }
        }

    }
}

