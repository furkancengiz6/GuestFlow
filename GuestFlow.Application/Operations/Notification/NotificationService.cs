using AutoMapper;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<NotificationEntity> _notificationRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationHubService? _hubService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IRepository<NotificationEntity> notificationRepository,
            IEmailService emailService,
            ILogger<NotificationService> logger,
            IMapper mapper,
            INotificationHubService? hubService = null)
        {
            _unitOfWork = unitOfWork;
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _logger = logger;
            _mapper = mapper;
            _hubService = hubService;
        }

        public async Task<ServiceMessage<NotificationDto>> CreateAndSendNotificationAsync(CreateNotificationDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var notification = new NotificationEntity
                {
                    Title = dto.Title,
                    Content = dto.Content,
                    NotificationType = dto.NotificationType,
                    RecipientEmail = dto.RecipientEmail,
                    RecipientPersonnelId = dto.RecipientPersonnelId,
                    RecipientGuestId = dto.RecipientGuestId,
                    Status = "Pending",
                    TemplateName = dto.TemplateName,
                    RelatedEntityType = dto.RelatedEntityType,
                    RelatedEntityId = dto.RelatedEntityId
                };

                await _notificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                // E-posta gönderimi
                if (dto.NotificationType == "Email" && !string.IsNullOrEmpty(dto.RecipientEmail))
                {
                    try
                    {
                        var emailResult = await _emailService.SendEmailAsync(
                            dto.RecipientEmail,
                            dto.Title,
                            dto.Content);

                        if (emailResult)
                        {
                            notification.Status = "Sent";
                            notification.SentDate = DateTime.UtcNow;
                        }
                        else
                        {
                            notification.Status = "Failed";
                            notification.ErrorMessage = "E-posta gönderilemedi.";
                        }
                    }
                    catch (Exception ex)
                    {
                        notification.Status = "Failed";
                        notification.ErrorMessage = ex.Message;
                        _logger.LogError(ex, $"E-posta gönderilirken hata: {ex.Message}");
                    }
                }
                else
                {
                    notification.Status = "Sent";
                    notification.SentDate = DateTime.UtcNow;
                }

                await _notificationRepository.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var notificationDto = MapToDto(notification);

                // Send via SignalR if hub service is available
                if (_hubService != null)
                {
                    try
                    {
                        if (dto.RecipientPersonnelId.HasValue)
                        {
                            await _hubService.SendNotificationToUserAsync(dto.RecipientPersonnelId.Value, notificationDto);
                        }
                        else
                        {
                            // Send to all users if no specific recipient
                            await _hubService.SendNotificationToAllAsync(notificationDto);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "SignalR notification gönderilirken hata oluştu, ancak bildirim kaydedildi.");
                    }
                }

                return new ServiceMessage<NotificationDto>
                {
                    IsSuccess = true,
                    Message = "Bildirim başarıyla oluşturuldu ve gönderildi.",
                    Data = notificationDto
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Bildirim oluşturulurken hata: {ex.Message}");
                return new ServiceMessage<NotificationDto>
                {
                    IsSuccess = false,
                    Message = $"Bildirim oluşturulurken hata: {ex.Message}"
                };
            }
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(
            string? notificationType = null,
            string? status = null,
            int? recipientPersonnelId = null,
            int? recipientGuestId = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            try
            {
                var query = _notificationRepository.GetAll();

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(n => n.NotificationType == notificationType);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(n => n.Status == status);
                }

                if (recipientPersonnelId.HasValue)
                {
                    query = query.Where(n => n.RecipientPersonnelId == recipientPersonnelId.Value);
                }

                if (recipientGuestId.HasValue)
                {
                    query = query.Where(n => n.RecipientGuestId == recipientGuestId.Value);
                }

                query = query.OrderByDescending(n => n.CreatedDate);

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    query = query.Skip((pageNumber.Value - 1) * pageSize.Value).Take(pageSize.Value);
                }

                var notifications = await query.ToListAsync();
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirimler getirilirken hata: {ex.Message}");
                return new List<NotificationDto>();
            }
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(id);
                return notification != null ? MapToDto(notification) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim getirilirken hata: {ex.Message}. Id: {id}");
                return null;
            }
        }

        public Task<List<NotificationTemplateDto>> GetTemplatesAsync()
        {
            // Varsayılan şablonlar
            var templates = new List<NotificationTemplateDto>
            {
                new NotificationTemplateDto
                {
                    Name = "InvoiceCreated",
                    Subject = "Faturanız Oluşturuldu",
                    Body = "Sayın {{GuestName}}, {{InvoiceNumber}} numaralı faturanız oluşturulmuştur. Toplam tutar: {{TotalAmount}} {{Currency}}",
                    NotificationType = "Email",
                    Description = "Fatura oluşturulduğunda gönderilir",
                    Variables = new Dictionary<string, string>
                    {
                        { "GuestName", "Misafir adı" },
                        { "InvoiceNumber", "Fatura numarası" },
                        { "TotalAmount", "Toplam tutar" },
                        { "Currency", "Para birimi" }
                    }
                },
                new NotificationTemplateDto
                {
                    Name = "TransferConfirmed",
                    Subject = "Transfer Rezervasyonunuz Onaylandı",
                    Body = "Sayın {{GuestName}}, {{TransferDate}} tarihindeki transfer rezervasyonunuz onaylanmıştır.",
                    NotificationType = "Email",
                    Description = "Transfer onaylandığında gönderilir",
                    Variables = new Dictionary<string, string>
                    {
                        { "GuestName", "Misafir adı" },
                        { "TransferDate", "Transfer tarihi" },
                        { "PickupAddress", "Alış adresi" },
                        { "DropoffAddress", "Bırakış adresi" }
                    }
                },
                new NotificationTemplateDto
                {
                    Name = "TourConfirmed",
                    Subject = "Tur Rezervasyonunuz Onaylandı",
                    Body = "Sayın {{GuestName}}, {{TourDate}} tarihindeki tur rezervasyonunuz onaylanmıştır.",
                    NotificationType = "Email",
                    Description = "Tur onaylandığında gönderilir",
                    Variables = new Dictionary<string, string>
                    {
                        { "GuestName", "Misafir adı" },
                        { "TourDate", "Tur tarihi" },
                        { "TourType", "Tur tipi" }
                    }
                },
                new NotificationTemplateDto
                {
                    Name = "PasswordReset",
                    Subject = "Şifre Sıfırlama",
                    Body = "Sayın {{UserName}}, şifre sıfırlama linkiniz: {{ResetLink}}",
                    NotificationType = "Email",
                    Description = "Şifre sıfırlama için gönderilir",
                    Variables = new Dictionary<string, string>
                    {
                        { "UserName", "Kullanıcı adı" },
                        { "ResetLink", "Şifre sıfırlama linki" }
                    }
                }
            };

            return Task.FromResult(templates);
        }

        public async Task<NotificationTemplateDto?> GetTemplateAsync(string templateName)
        {
            var templates = await GetTemplatesAsync();
            return templates.FirstOrDefault(t => t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<ServiceMessage<NotificationDto>> SendNotificationWithTemplateAsync(
            string templateName,
            string recipientEmail,
            Dictionary<string, string> variables,
            string? relatedEntityType = null,
            int? relatedEntityId = null)
        {
            try
            {
                var template = await GetTemplateAsync(templateName);
                if (template == null)
                {
                    return new ServiceMessage<NotificationDto>
                    {
                        IsSuccess = false,
                        Message = $"Şablon bulunamadı: {templateName}"
                    };
                }

                var subject = ReplaceVariables(template.Subject, variables);
                var body = ReplaceVariables(template.Body, variables);

                var createDto = new CreateNotificationDto
                {
                    Title = subject,
                    Content = body,
                    NotificationType = template.NotificationType,
                    RecipientEmail = recipientEmail,
                    TemplateName = templateName,
                    RelatedEntityType = relatedEntityType,
                    RelatedEntityId = relatedEntityId
                };

                return await CreateAndSendNotificationAsync(createDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Şablon ile bildirim gönderilirken hata: {ex.Message}");
                return new ServiceMessage<NotificationDto>
                {
                    IsSuccess = false,
                    Message = $"Bildirim gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> SendTestEmailAsync(SendTestEmailDto dto)
        {
            try
            {
                string subject;
                string body;

                if (!string.IsNullOrEmpty(dto.TemplateName))
                {
                    var template = await GetTemplateAsync(dto.TemplateName);
                    if (template == null)
                    {
                        return new ServiceMessage
                        {
                            IsSuccess = false,
                            Message = $"Şablon bulunamadı: {dto.TemplateName}"
                        };
                    }

                    subject = template.Subject;
                    body = template.Body;
                }
                else
                {
                    subject = dto.Subject ?? "Test E-postası";
                    body = dto.Body ?? "Bu bir test e-postasıdır.";
                }

                var emailResult = await _emailService.SendEmailAsync(dto.ToEmail, subject, body);

                if (emailResult)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = true,
                        Message = "Test e-postası başarıyla gönderildi."
                    };
                }

                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = "Test e-postası gönderilemedi."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Test e-postası gönderilirken hata: {ex.Message}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Test e-postası gönderilirken hata: {ex.Message}"
                };
            }
        }

        public async Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _notificationRepository.GetAll();

                if (startDate.HasValue)
                {
                    query = query.Where(n => n.CreatedDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(n => n.CreatedDate <= endDate.Value);
                }

                var notifications = await query.ToListAsync();

                var statistics = new NotificationStatisticsDto
                {
                    TotalNotifications = notifications.Count,
                    SentNotifications = notifications.Count(n => n.Status == "Sent"),
                    FailedNotifications = notifications.Count(n => n.Status == "Failed"),
                    PendingNotifications = notifications.Count(n => n.Status == "Pending"),
                    NotificationsByType = notifications.GroupBy(n => n.NotificationType)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    NotificationsByStatus = notifications.GroupBy(n => n.Status)
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Bildirim istatistikleri getirilirken hata: {ex.Message}");
                return new NotificationStatisticsDto();
            }
        }

        public async Task<List<NotificationDto>> GetNotificationHistoryAsync(
            string? notificationType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? pageNumber = null,
            int? pageSize = null)
        {
            return await GetNotificationsAsync(notificationType, status, null, null, pageNumber, pageSize);
        }

        public async Task<ServiceMessage> MarkNotificationAsReadAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Bildirim bulunamadı."
                    };
                }

                notification.IsRead = true;
                notification.ReadDate = DateTime.UtcNow;

                await _notificationRepository.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Bildirim okundu olarak işaretlendi: {id}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "Bildirim okundu olarak işaretlendi."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Bildirim okundu işaretlenirken hata: {ex.Message}. Id: {id}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Bildirim okundu işaretlenirken hata: {ex.Message}"
                };
            }
        }

        public async Task<ServiceMessage> DeleteNotificationAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    return new ServiceMessage
                    {
                        IsSuccess = false,
                        Message = "Bildirim bulunamadı."
                    };
                }

                await _notificationRepository.DeleteAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation($"Bildirim silindi: {id}");
                return new ServiceMessage
                {
                    IsSuccess = true,
                    Message = "Bildirim başarıyla silindi."
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, $"Bildirim silinirken hata: {ex.Message}. Id: {id}");
                return new ServiceMessage
                {
                    IsSuccess = false,
                    Message = $"Bildirim silinirken hata: {ex.Message}"
                };
            }
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int? personnelId = null, int? guestId = null, bool? unreadOnly = false)
        {
            try
            {
                var query = _notificationRepository.GetAll();

                if (personnelId.HasValue)
                {
                    query = query.Where(n => n.RecipientPersonnelId == personnelId.Value);
                }

                if (guestId.HasValue)
                {
                    query = query.Where(n => n.RecipientGuestId == guestId.Value);
                }

                if (unreadOnly == true)
                {
                    query = query.Where(n => !n.IsRead);
                }

                query = query.OrderByDescending(n => n.CreatedDate);

                var notifications = await query.ToListAsync();
                return notifications.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Kullanıcı bildirimleri getirilirken hata: {ex.Message}");
                return new List<NotificationDto>();
            }
        }

        public async Task<PagedResult<NotificationDto>> GetNotificationsPagedAsync(
            int pageNumber,
            int pageSize,
            string? notificationType = null,
            string? status = null,
            int? recipientPersonnelId = null,
            int? recipientGuestId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var query = _notificationRepository.GetAll();

                if (!string.IsNullOrEmpty(notificationType))
                {
                    query = query.Where(n => n.NotificationType == notificationType);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(n => n.Status == status);
                }

                if (recipientPersonnelId.HasValue)
                {
                    query = query.Where(n => n.RecipientPersonnelId == recipientPersonnelId.Value);
                }

                if (recipientGuestId.HasValue)
                {
                    query = query.Where(n => n.RecipientGuestId == recipientGuestId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(n => n.CreatedDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(n => n.CreatedDate <= endDate.Value);
                }

                query = query.OrderByDescending(n => n.CreatedDate);

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new PagedResult<NotificationDto>(notificationDtos, totalCount, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Sayfalanmış bildirimler getirilirken hata: {ex.Message}");
                throw;
            }
        }

        private NotificationDto MapToDto(NotificationEntity entity)
        {
                return _mapper.Map<NotificationDto>(entity);
        }

        private string ReplaceVariables(string text, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(text) || variables == null || variables.Count == 0)
                return text;

            var result = text;
            foreach (var variable in variables)
            {
                result = result.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            return result;
        }
    }
}

