using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Notification
{
    public interface INotificationService
    {
        /// <summary>
        /// Bildirim oluşturur ve gönderir
        /// </summary>
        Task<ServiceMessage<NotificationDto>> CreateAndSendNotificationAsync(CreateNotificationDto dto);

        /// <summary>
        /// Bildirim listesini getirir
        /// </summary>
        Task<List<NotificationDto>> GetNotificationsAsync(
            string? notificationType = null,
            string? status = null,
            int? recipientPersonnelId = null,
            int? recipientGuestId = null,
            int? pageNumber = null,
            int? pageSize = null);

        /// <summary>
        /// Bildirim detayını getirir
        /// </summary>
        Task<NotificationDto?> GetNotificationByIdAsync(int id);

        /// <summary>
        /// Bildirim şablonlarını getirir
        /// </summary>
        Task<List<NotificationTemplateDto>> GetTemplatesAsync();

        /// <summary>
        /// Bildirim şablonunu getirir
        /// </summary>
        Task<NotificationTemplateDto?> GetTemplateAsync(string templateName);

        /// <summary>
        /// Şablon kullanarak bildirim gönderir
        /// </summary>
        Task<ServiceMessage<NotificationDto>> SendNotificationWithTemplateAsync(
            string templateName,
            string recipientEmail,
            Dictionary<string, string> variables,
            string? relatedEntityType = null,
            int? relatedEntityId = null);

        /// <summary>
        /// Test e-postası gönderir
        /// </summary>
        Task<ServiceMessage> SendTestEmailAsync(SendTestEmailDto dto);

        /// <summary>
        /// Bildirim istatistiklerini getirir
        /// </summary>
        Task<NotificationStatisticsDto> GetNotificationStatisticsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Bildirim geçmişini getirir
        /// </summary>
        Task<List<NotificationDto>> GetNotificationHistoryAsync(
            string? notificationType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? pageNumber = null,
            int? pageSize = null);
        
        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        Task<ServiceMessage> MarkNotificationAsReadAsync(int id);
        
        /// <summary>
        /// Bildirimi siler
        /// </summary>
        Task<ServiceMessage> DeleteNotificationAsync(int id);
        
        /// <summary>
        /// Kullanıcının bildirimlerini getirir (okunmamış öncelikli)
        /// </summary>
        Task<List<NotificationDto>> GetUserNotificationsAsync(int? personnelId = null, int? guestId = null, bool? unreadOnly = false);
        
        /// <summary>
        /// Sayfalanmış bildirim listesi getirir
        /// </summary>
        Task<PagedResult<NotificationDto>> GetNotificationsPagedAsync(
            int pageNumber,
            int pageSize,
            string? notificationType = null,
            string? status = null,
            int? recipientPersonnelId = null,
            int? recipientGuestId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}

