// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Communication.Dtos;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Communication
{
    /// <summary>
    /// Unified Communication servisi - tüm iletişim kanallarını birleştirir
    /// </summary>
    public interface IUnifiedCommunicationService
    {
        /// <summary>
        /// Misafir için tüm iletişim geçmişini getirir (e-posta, SMS, WhatsApp, in-app)
        /// </summary>
        Task<ApiResponse<UnifiedCommunicationHistoryDto>> GetGuestCommunicationHistoryAsync(int guestId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Misafire mesaj gönderir (e-posta, SMS, WhatsApp)
        /// </summary>
        Task<ApiResponse<bool>> SendMessageAsync(int guestId, SendMessageDto dto);

        /// <summary>
        /// Smart notification gönderir (Pre-Arrival, Arrival, During Stay, Pre-Departure, Special Occasions)
        /// </summary>
        Task<ApiResponse<bool>> SendSmartNotificationAsync(int guestId, SmartNotificationType notificationType);
    }

    /// <summary>
    /// Mesaj gönderme DTO
    /// </summary>
    public class SendMessageDto
    {
        public string Channel { get; set; } = string.Empty; // Email, SMS, WhatsApp
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }

    /// <summary>
    /// Smart notification tipi
    /// </summary>
    public enum SmartNotificationType
    {
        PreArrival,      // Check-in öncesi hoş geldin mesajı
        Arrival,         // Check-in sonrası bilgilendirme
        DuringStay,      // Hizmet hatırlatmaları
        PreDeparture,    // Check-out öncesi veda mesajı
        SpecialOccasion  // Doğum günü, yıldönümü
    }
}
