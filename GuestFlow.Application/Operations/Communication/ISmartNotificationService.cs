// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Communication
{
    /// <summary>
    /// Smart Notification Service - Concierge için akıllı bildirimler
    /// Pre-Arrival, Arrival, During Stay, Pre-Departure, Special Occasions
    /// </summary>
    public interface ISmartNotificationService
    {
        /// <summary>
        /// Pre-Arrival bildirimleri gönder (check-in öncesi hoş geldin mesajı)
        /// </summary>
        Task<ApiResponse<bool>> SendPreArrivalNotificationsAsync(DateTime? targetDate = null);

        /// <summary>
        /// Arrival bildirimleri gönder (check-in sonrası bilgilendirme)
        /// </summary>
        Task<ApiResponse<bool>> SendArrivalNotificationsAsync(DateTime? targetDate = null);

        /// <summary>
        /// During Stay bildirimleri gönder (hizmet hatırlatmaları)
        /// </summary>
        Task<ApiResponse<bool>> SendDuringStayNotificationsAsync();

        /// <summary>
        /// Pre-Departure bildirimleri gönder (check-out öncesi veda mesajı)
        /// </summary>
        Task<ApiResponse<bool>> SendPreDepartureNotificationsAsync(DateTime? targetDate = null);

        /// <summary>
        /// Special Occasions bildirimleri gönder (doğum günü, yıldönümü)
        /// </summary>
        Task<ApiResponse<bool>> SendSpecialOccasionNotificationsAsync(DateTime? targetDate = null);

        /// <summary>
        /// Belirli bir misafir için özel bildirim gönder
        /// </summary>
        Task<ApiResponse<bool>> SendCustomNotificationAsync(int guestId, string notificationType, string message, string? channel = null);

        /// <summary>
        /// Bildirim şablonlarını getir
        /// </summary>
        Task<ApiResponse<List<NotificationTemplateDto>>> GetNotificationTemplatesAsync(string? notificationType = null);
    }

    /// <summary>
    /// Bildirim şablonu DTO
    /// </summary>
    public class NotificationTemplateDto
    {
        public string TemplateId { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // PreArrival, Arrival, DuringStay, PreDeparture, SpecialOccasion
        public string Title { get; set; } = string.Empty;
        public string MessageTemplate { get; set; } = string.Empty;
        public string? Channel { get; set; } // Email, SMS, WhatsApp
        public Dictionary<string, string>? Placeholders { get; set; } // {GuestName}, {RoomNumber}, {CheckInDate}, vb.
    }
}
