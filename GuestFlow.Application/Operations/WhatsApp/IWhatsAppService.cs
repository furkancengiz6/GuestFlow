// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.WhatsApp.Dtos;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.WhatsApp
{
    /// <summary>
    /// WhatsApp servisi interface'i
    /// </summary>
    public interface IWhatsAppService
    {
        /// <summary>
        /// WhatsApp mesajı gönderir
        /// </summary>
        Task<ServiceMessage<GetWhatsAppHistoryDto>> SendWhatsAppAsync(SendWhatsAppDto whatsAppDto);

        /// <summary>
        /// Transfer hatırlatma WhatsApp mesajı gönderir
        /// </summary>
        Task<ServiceMessage<GetWhatsAppHistoryDto>> SendTransferReminderAsync(int transferId, int hoursBefore = 24);

        /// <summary>
        /// Tur hatırlatma WhatsApp mesajı gönderir
        /// </summary>
        Task<ServiceMessage<GetWhatsAppHistoryDto>> SendTourReminderAsync(string tourType, int tourId, int hoursBefore = 24);

        /// <summary>
        /// Rezervasyon onay WhatsApp mesajı gönderir
        /// </summary>
        Task<ServiceMessage<GetWhatsAppHistoryDto>> SendReservationConfirmationAsync(int reservationId);

        /// <summary>
        /// WhatsApp geçmişini getirir
        /// </summary>
        Task<GetWhatsAppHistoryDto?> GetWhatsAppHistoryByIdAsync(int id);

        /// <summary>
        /// Sayfalanmış WhatsApp geçmişini getirir
        /// </summary>
        Task<PagedResult<GetWhatsAppHistoryDto>> GetWhatsAppHistoryPagedAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters = null, SortingParameters? sorting = null);

        /// <summary>
        /// Misafire gönderilen WhatsApp mesajlarını getirir
        /// </summary>
        Task<List<GetWhatsAppHistoryDto>> GetWhatsAppHistoryByGuestIdAsync(int guestId);

        /// <summary>
        /// Duruma göre WhatsApp mesajlarını getirir
        /// </summary>
        Task<List<GetWhatsAppHistoryDto>> GetWhatsAppHistoryByStatusAsync(string status);

        /// <summary>
        /// WhatsApp durumunu günceller (webhook callback için)
        /// </summary>
        Task<ServiceMessage> UpdateWhatsAppStatusAsync(int whatsAppId, string status, string? messageId = null, string? gatewayResponse = null);

        /// <summary>
        /// WhatsApp istatistiklerini getirir
        /// </summary>
        Task<WhatsAppStatisticsDto> GetWhatsAppStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
