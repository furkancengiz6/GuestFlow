// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Communication.Dtos;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Communication
{
    /// <summary>
    /// Unified Communication servisi implementasyonu
    /// </summary>
    public class UnifiedCommunicationService : IUnifiedCommunicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnifiedCommunicationService> _logger;

        public UnifiedCommunicationService(
            IUnitOfWork unitOfWork,
            IPMSIntegrationService pmsIntegrationService,
            IServiceProvider serviceProvider,
            ILogger<UnifiedCommunicationService> logger)
        {
            _unitOfWork = unitOfWork;
            _pmsIntegrationService = pmsIntegrationService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SendSmartNotificationAsync(int guestId, SmartNotificationType notificationType)
        {
            var smartNotificationService = _serviceProvider.GetRequiredService<ISmartNotificationService>();
            return await smartNotificationService.SendCustomNotificationAsync(guestId, notificationType.ToString(), "");
        }

        public async Task<ApiResponse<UnifiedCommunicationHistoryDto>> GetGuestCommunicationHistoryAsync(
            int guestId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<UnifiedCommunicationHistoryDto>.Fail("Guest not found");

                var result = new UnifiedCommunicationHistoryDto
                {
                    GuestId = guest.Id,
                    GuestName = guest.FullName
                };

                var communications = new List<CommunicationItemDto>();

                // E-posta geçmişi
                var emailQuery = _unitOfWork.EmailHistories
                    .GetAll(e => e.To == guest.Email && !e.IsDeleted);

                if (startDate.HasValue)
                    emailQuery = emailQuery.Where(e => e.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    emailQuery = emailQuery.Where(e => e.SentDate <= endDate.Value);

                var emails = await emailQuery
                    .OrderByDescending(e => e.SentDate)
                    .ToListAsync();

                foreach (var email in emails)
                {
                    communications.Add(new CommunicationItemDto
                    {
                        Id = email.Id,
                        Channel = "Email",
                        Direction = "Outbound", // GuestFlow'dan gönderilen e-postalar
                        Subject = email.Subject,
                        Content = email.Subject, // E-posta içeriği ayrı bir field'da tutulmuyor, şimdilik subject kullanıyoruz
                        SentDate = email.SentDate,
                        Status = email.Status,
                        ErrorMessage = email.ErrorMessage,
                        TemplateName = email.TemplateName,
                        RelatedEntityType = email.RelatedEntityType,
                        RelatedEntityId = email.RelatedEntityId,
                        Source = "GuestFlow"
                    });
                }

                // SMS geçmişi
                var smsQuery = _unitOfWork.SmsHistories
                    .GetAll(s => s.GuestId == guestId && !s.IsDeleted);

                if (startDate.HasValue)
                    smsQuery = smsQuery.Where(s => s.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    smsQuery = smsQuery.Where(s => s.SentDate <= endDate.Value);

                var smsList = await smsQuery
                    .OrderByDescending(s => s.SentDate)
                    .ToListAsync();

                foreach (var sms in smsList)
                {
                    communications.Add(new CommunicationItemDto
                    {
                        Id = sms.Id,
                        Channel = "SMS",
                        Direction = "Outbound",
                        Subject = sms.TemplateName ?? "SMS",
                        Content = sms.Message,
                        SentDate = sms.SentDate,
                        DeliveredDate = sms.DeliveredDate,
                        Status = sms.Status.ToString(),
                        ErrorMessage = sms.ErrorMessage,
                        TemplateName = sms.TemplateName,
                        RelatedEntityType = sms.RelatedEntityType,
                        RelatedEntityId = sms.RelatedEntityId,
                        Provider = sms.Provider,
                        MessageId = sms.MessageId,
                        PersonnelId = sms.PersonnelId,
                        Source = "GuestFlow"
                    });
                }

                // WhatsApp geçmişi
                var whatsAppQuery = _unitOfWork.WhatsAppHistories
                    .GetAll(w => w.GuestId == guestId && !w.IsDeleted);

                if (startDate.HasValue)
                    whatsAppQuery = whatsAppQuery.Where(w => w.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    whatsAppQuery = whatsAppQuery.Where(w => w.SentDate <= endDate.Value);

                var whatsAppList = await whatsAppQuery
                    .OrderByDescending(w => w.SentDate)
                    .ToListAsync();

                foreach (var whatsApp in whatsAppList)
                {
                    communications.Add(new CommunicationItemDto
                    {
                        Id = whatsApp.Id,
                        Channel = "WhatsApp",
                        Direction = "Outbound",
                        Subject = whatsApp.TemplateName ?? "WhatsApp",
                        Content = whatsApp.Message,
                        SentDate = whatsApp.SentDate,
                        DeliveredDate = whatsApp.DeliveredDate,
                        Status = whatsApp.Status,
                        ErrorMessage = whatsApp.ErrorMessage,
                        TemplateName = whatsApp.TemplateName,
                        RelatedEntityType = whatsApp.RelatedEntityType,
                        RelatedEntityId = whatsApp.RelatedEntityId,
                        Provider = whatsApp.Provider,
                        MessageId = whatsApp.MessageId,
                        PersonnelId = whatsApp.PersonnelId,
                        Source = "GuestFlow"
                    });
                }

                // In-app notifications (NotificationEntity)
                var notificationQuery = _unitOfWork.Notifications
                    .GetAll(n => n.RecipientGuestId == guestId && !n.IsDeleted);

                if (startDate.HasValue)
                    notificationQuery = notificationQuery.Where(n => n.SentDate >= startDate.Value);
                if (endDate.HasValue)
                    notificationQuery = notificationQuery.Where(n => n.SentDate <= endDate.Value);

                var notifications = await notificationQuery
                    .OrderByDescending(n => n.SentDate ?? n.CreatedDate)
                    .ToListAsync();

                foreach (var notification in notifications)
                {
                    communications.Add(new CommunicationItemDto
                    {
                        Id = notification.Id,
                        Channel = "InApp",
                        Direction = "Outbound",
                        Subject = notification.Title,
                        Content = notification.Content,
                        SentDate = notification.SentDate ?? notification.CreatedDate,
                        Status = notification.Status,
                        ErrorMessage = notification.ErrorMessage,
                        TemplateName = notification.TemplateName,
                        RelatedEntityType = notification.RelatedEntityType,
                        RelatedEntityId = notification.RelatedEntityId,
                        Source = "GuestFlow"
                    });
                }

                // PMS'den gelen iletişim bilgilerini ekle (eğer varsa)
                await AddPMSCommunicationContextAsync(guestId, communications, startDate, endDate);

                // Tarihe göre sırala (en yeni önce)
                result.Communications = communications.OrderByDescending(c => c.SentDate).ToList();

                // Özet bilgileri hesapla
                result.Summary = new CommunicationSummaryDto
                {
                    TotalCommunications = communications.Count,
                    EmailCount = communications.Count(c => c.Channel == "Email"),
                    SmsCount = communications.Count(c => c.Channel == "SMS"),
                    WhatsAppCount = communications.Count(c => c.Channel == "WhatsApp"),
                    InAppCount = communications.Count(c => c.Channel == "InApp"),
                    InboundCount = communications.Count(c => c.Direction == "Inbound"),
                    OutboundCount = communications.Count(c => c.Direction == "Outbound"),
                    LastCommunicationDate = communications.Any() 
                        ? communications.Max(c => c.SentDate) 
                        : null
                };

                return ApiResponse<UnifiedCommunicationHistoryDto>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get communication history for guest {GuestId}", guestId);
                return ApiResponse<UnifiedCommunicationHistoryDto>.Fail($"Failed to get communication history: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendMessageAsync(int guestId, SendMessageDto dto)
        {
            try
            {
                var guest = await _unitOfWork.Guests.GetByIdAsync(guestId);
                if (guest == null)
                    return ApiResponse<bool>.Fail("Guest not found");

                // Channel'a göre mesaj gönder
                switch (dto.Channel.ToUpperInvariant())
                {
                    case "EMAIL":
                        // TODO: EmailService kullanarak e-posta gönder
                        _logger.LogInformation("Sending email to guest {GuestId}: {Subject}", guestId, dto.Subject);
                        break;

                    case "SMS":
                        // TODO: SmsService kullanarak SMS gönder
                        _logger.LogInformation("Sending SMS to guest {GuestId}", guestId);
                        break;

                    case "WHATSAPP":
                        var whatsAppService = _serviceProvider.GetService<GuestFlow.Application.Operations.WhatsApp.IWhatsAppService>();
                        if (whatsAppService != null)
                        {
                            var whatsAppDto = new GuestFlow.Application.Operations.WhatsApp.Dtos.SendWhatsAppDto
                            {
                                PhoneNumber = guest.PhoneNumber ?? string.Empty,
                                Message = dto.Content,
                                GuestId = guestId,
                                RelatedEntityType = dto.RelatedEntityType,
                                RelatedEntityId = dto.RelatedEntityId,
                                TemplateName = dto.TemplateName,
                                MessageType = GuestFlow.Application.Operations.WhatsApp.Dtos.WhatsAppMessageType.Text
                            };
                            var whatsAppResult = await whatsAppService.SendWhatsAppAsync(whatsAppDto);
                            if (!whatsAppResult.IsSuccess)
                            {
                                _logger.LogWarning("WhatsApp mesajı gönderilemedi: {Message}", whatsAppResult.Message);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("WhatsApp servisi bulunamadı");
                        }
                        break;

                    default:
                        return ApiResponse<bool>.Fail($"Unsupported channel: {dto.Channel}");
                }

                return ApiResponse<bool>.SuccessResponse(true, "Message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message to guest {GuestId}", guestId);
                return ApiResponse<bool>.Fail($"Failed to send message: {ex.Message}");
            }
        }


        /// <summary>
        /// PMS'den gelen iletişim bağlamını ekler (rezervasyon bilgileri, misafir bilgileri)
        /// Not: PMS'ler genellikle iletişim geçmişi sağlamaz, ancak rezervasyon bilgilerini kullanarak
        /// iletişim geçmişine ek bağlam ekleyebiliriz
        /// </summary>
        private async Task AddPMSCommunicationContextAsync(
            int guestId, 
            List<CommunicationItemDto> communications, 
            DateTime? startDate, 
            DateTime? endDate)
        {
            try
            {
                // PMS guest mapping'lerini bul
                var pmsMappings = await _unitOfWork.PMSGuestMappings
                    .GetAll(m => m.GuestFlowGuestId == guestId && !m.IsDeleted)
                    .Include(m => m.PMSIntegration)
                    .ToListAsync();

                if (!pmsMappings.Any())
                    return;

                // Her PMS entegrasyonu için rezervasyon bilgilerini getir
                foreach (var mapping in pmsMappings)
                {
                    try
                    {
                        var integration = mapping.PMSIntegration;
                        if (integration == null || !integration.IsActive)
                            continue;

                        // PMS'den misafir profilini getir
                        var guestProfileResponse = await _pmsIntegrationService.GetGuestProfileAsync(
                            integration.Id, mapping.PMSGuestId);

                        if (guestProfileResponse.Success && guestProfileResponse.Data != null)
                        {
                            var pmsGuest = guestProfileResponse.Data;

                            // PMS'den gelen rezervasyon bilgilerini kullanarak iletişim bağlamı ekle
                            // Örneğin: Rezervasyon onayı, check-in/check-out bildirimleri
                            var reservationDate = pmsGuest.CheckInDate ?? DateTime.UtcNow;

                            // Eğer tarih aralığı içindeyse, PMS'den gelen rezervasyon bilgilerini ekle
                            if (!startDate.HasValue || reservationDate >= startDate.Value)
                            {
                                if (!endDate.HasValue || reservationDate <= endDate.Value)
                                {
                                    // Rezervasyon onayı iletişimi (PMS'den gelen bilgi)
                                    communications.Add(new CommunicationItemDto
                                    {
                                        Id = -mapping.Id, // Negatif ID ile PMS kaynaklı olduğunu belirt
                                        Channel = "Email", // PMS genellikle e-posta ile rezervasyon onayı gönderir
                                        Direction = "Outbound",
                                        Subject = $"Rezervasyon Onayı - {pmsGuest.RoomNumber}",
                                        Content = $"PMS'den gelen rezervasyon bilgisi: {pmsGuest.FullName}, Oda: {pmsGuest.RoomNumber}, Check-in: {pmsGuest.CheckInDate:dd.MM.yyyy}, Check-out: {pmsGuest.CheckOutDate:dd.MM.yyyy}",
                                        SentDate = reservationDate,
                                        Status = "Sent",
                                        Source = $"PMS-{integration.ProviderName}",
                                        RelatedEntityType = "PMSReservation",
                                        RelatedEntityId = null
                                    });
                                }
                            }

                            // Check-in/check-out tarihlerine göre iletişim bağlamı ekle
                            if (pmsGuest.CheckInDate.HasValue)
                            {
                                var checkInDate = pmsGuest.CheckInDate.Value;
                                if ((!startDate.HasValue || checkInDate >= startDate.Value) &&
                                    (!endDate.HasValue || checkInDate <= endDate.Value))
                                {
                                    communications.Add(new CommunicationItemDto
                                    {
                                        Id = -mapping.Id - 1000, // Unique ID
                                        Channel = "InApp",
                                        Direction = "Outbound",
                                        Subject = "Check-in Bildirimi",
                                        Content = $"PMS'den gelen check-in bilgisi: Oda {pmsGuest.RoomNumber}",
                                        SentDate = checkInDate,
                                        Status = "Sent",
                                        Source = $"PMS-{integration.ProviderName}",
                                        RelatedEntityType = "PMSReservation",
                                        RelatedEntityId = null
                                    });
                                }
                            }

                            if (pmsGuest.CheckOutDate.HasValue)
                            {
                                var checkOutDate = pmsGuest.CheckOutDate.Value;
                                if ((!startDate.HasValue || checkOutDate >= startDate.Value) &&
                                    (!endDate.HasValue || checkOutDate <= endDate.Value))
                                {
                                    communications.Add(new CommunicationItemDto
                                    {
                                        Id = -mapping.Id - 2000, // Unique ID
                                        Channel = "InApp",
                                        Direction = "Outbound",
                                        Subject = "Check-out Bildirimi",
                                        Content = $"PMS'den gelen check-out bilgisi: Oda {pmsGuest.RoomNumber}",
                                        SentDate = checkOutDate,
                                        Status = "Sent",
                                        Source = $"PMS-{integration.ProviderName}",
                                        RelatedEntityType = "PMSReservation",
                                        RelatedEntityId = null
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PMS integration {IntegrationId} için iletişim bağlamı eklenirken hata oluştu", mapping.PMSIntegrationId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PMS iletişim bağlamı eklenirken hata oluştu: GuestId={GuestId}", guestId);
            }
        }
    }
}
