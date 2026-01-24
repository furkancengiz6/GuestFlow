// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.Communication.Dtos;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Communication
{
    /// <summary>
    /// Smart Notification Service implementation
    /// </summary>
    public class SmartNotificationService : ISmartNotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConciergeDashboardService _conciergeDashboardService;
        private readonly IPMSIntegrationService _pmsIntegrationService;
        private readonly IUnifiedCommunicationService _communicationService;
        private readonly ILogger<SmartNotificationService> _logger;

        public SmartNotificationService(
            IUnitOfWork unitOfWork,
            IConciergeDashboardService conciergeDashboardService,
            IPMSIntegrationService pmsIntegrationService,
            IUnifiedCommunicationService communicationService,
            ILogger<SmartNotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _conciergeDashboardService = conciergeDashboardService;
            _pmsIntegrationService = pmsIntegrationService;
            _communicationService = communicationService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> SendPreArrivalNotificationsAsync(DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.UtcNow.Date;
                var checkIns = await _conciergeDashboardService.GetTodayCheckInsAsync();
                
                int sent = 0;
                int failed = 0;

                foreach (var checkIn in checkIns.Items)
                {
                    try
                    {
                        // Pre-arrival mesajı hazırla
                        var message = BuildPreArrivalMessage(checkIn);
                        
                        // Misafirin iletişim tercihlerine göre kanal seç
                        var channel = DeterminePreferredChannel(checkIn.GuestId);
                        
                        // Bildirim gönder
                        var sendDto = new SendMessageDto
                        {
                            Channel = channel,
                            Subject = "Pre-Arrival Welcome",
                            Content = message
                        };
                        var result = await _communicationService.SendMessageAsync(checkIn.GuestId, sendDto);

                        if (result.Success)
                            sent++;
                        else
                            failed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send pre-arrival notification for guest {GuestId}", checkIn.GuestId);
                        failed++;
                    }
                }

                _logger.LogInformation("Pre-arrival notifications sent: {Sent}, Failed: {Failed}", sent, failed);
                return ApiResponse<bool>.SuccessResponse(true, $"Sent {sent} pre-arrival notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send pre-arrival notifications");
                return ApiResponse<bool>.Fail($"Failed to send pre-arrival notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendArrivalNotificationsAsync(DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.UtcNow.Date;
                var activeGuests = await _conciergeDashboardService.GetActiveGuestsAsync();
                
                // Bugün check-in olan misafirleri filtrele
                var todayArrivals = activeGuests
                    .Where(g => g.CheckInDate.HasValue && g.CheckInDate.Value.Date == date)
                    .ToList();

                int sent = 0;
                int failed = 0;

                foreach (var guest in todayArrivals)
                {
                    try
                    {
                        var message = BuildArrivalMessage(guest);
                        var channel = DeterminePreferredChannel(guest.GuestId);
                        
                        var sendDto = new SendMessageDto
                        {
                            Channel = channel,
                            Subject = "Welcome to Our Hotel",
                            Content = message
                        };
                        var result = await _communicationService.SendMessageAsync(guest.GuestId, sendDto);

                        if (result.Success)
                            sent++;
                        else
                            failed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send arrival notification for guest {GuestId}", guest.GuestId);
                        failed++;
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Sent {sent} arrival notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send arrival notifications");
                return ApiResponse<bool>.Fail($"Failed to send arrival notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendDuringStayNotificationsAsync()
        {
            try
            {
                var activeGuests = await _conciergeDashboardService.GetActiveGuestsAsync();
                var upcomingServices = await _conciergeDashboardService.GetUpcomingServicesForTodayAsync();
                
                int sent = 0;
                int failed = 0;

                // Her aktif misafir için yaklaşan servisleri kontrol et
                foreach (var guest in activeGuests)
                {
                    try
                    {
                        var guestServices = upcomingServices.Items
                            .Where(s => s.GuestId == guest.GuestId)
                            .ToList();

                        if (guestServices.Any())
                        {
                            var message = BuildDuringStayMessage(guest, guestServices);
                            var channel = DeterminePreferredChannel(guest.GuestId);
                            
                            var sendDto = new SendMessageDto
                            {
                                Channel = channel,
                                Subject = "Service Reminder",
                                Content = message
                            };
                            var result = await _communicationService.SendMessageAsync(guest.GuestId, sendDto);

                            if (result.Success)
                                sent++;
                            else
                                failed++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send during-stay notification for guest {GuestId}", guest.GuestId);
                        failed++;
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Sent {sent} during-stay notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send during-stay notifications");
                return ApiResponse<bool>.Fail($"Failed to send during-stay notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendPreDepartureNotificationsAsync(DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.UtcNow.Date;
                var checkOuts = await _conciergeDashboardService.GetTodayCheckOutsAsync();
                
                int sent = 0;
                int failed = 0;

                foreach (var checkOut in checkOuts.Items)
                {
                    try
                    {
                        var message = BuildPreDepartureMessage(checkOut);
                        var channel = DeterminePreferredChannel(checkOut.GuestId);
                        
                        var sendDto = new SendMessageDto
                        {
                            Channel = channel,
                            Subject = "Thank You for Staying With Us",
                            Content = message
                        };
                        var result = await _communicationService.SendMessageAsync(checkOut.GuestId, sendDto);

                        if (result.Success)
                            sent++;
                        else
                            failed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send pre-departure notification for guest {GuestId}", checkOut.GuestId);
                        failed++;
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Sent {sent} pre-departure notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send pre-departure notifications");
                return ApiResponse<bool>.Fail($"Failed to send pre-departure notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendSpecialOccasionNotificationsAsync(DateTime? targetDate = null)
        {
            try
            {
                var date = targetDate ?? DateTime.UtcNow.Date;
                
                // Doğum günü ve yıldönümü kontrolü için PMS'den misafir bilgilerini çek
                var activeGuests = await _conciergeDashboardService.GetActiveGuestsAsync();
                
                int sent = 0;
                int failed = 0;

                foreach (var guest in activeGuests)
                {
                    try
                    {
                        // PMS'den misafir detaylarını çek (doğum günü, yıldönümü bilgisi için)
                        var unifiedProfile = await _conciergeDashboardService.GetUnifiedGuestProfileAsync(guest.GuestId);
                        
                        // TODO: PMS'den doğum günü ve yıldönümü bilgilerini al
                        // Şimdilik placeholder - gerçek implementasyon PMS API'sine bağlı
                        
                        // Örnek: Doğum günü kontrolü
                        // if (unifiedProfile.BirthDate?.Month == date.Month && unifiedProfile.BirthDate?.Day == date.Day)
                        // {
                        //     var message = BuildBirthdayMessage(guest);
                        //     var channel = DeterminePreferredChannel(guest.GuestId);
                        //     await _communicationService.SendMessageAsync(guest.GuestId, "Happy Birthday!", message, channel);
                        //     sent++;
                        // }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send special occasion notification for guest {GuestId}", guest.GuestId);
                        failed++;
                    }
                }

                return ApiResponse<bool>.SuccessResponse(true, $"Sent {sent} special occasion notifications");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send special occasion notifications");
                return ApiResponse<bool>.Fail($"Failed to send special occasion notifications: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> SendCustomNotificationAsync(int guestId, string notificationType, string message, string? channel = null)
        {
            try
            {
                var selectedChannel = channel ?? DeterminePreferredChannel(guestId);
                
                var sendDto = new SendMessageDto
                {
                    Channel = selectedChannel,
                    Subject = notificationType,
                    Content = message
                };
                var result = await _communicationService.SendMessageAsync(guestId, sendDto);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send custom notification for guest {GuestId}", guestId);
                return ApiResponse<bool>.Fail($"Failed to send custom notification: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<NotificationTemplateDto>>> GetNotificationTemplatesAsync(string? notificationType = null)
        {
            try
            {
                var templates = new List<NotificationTemplateDto>
                {
                    new NotificationTemplateDto
                    {
                        TemplateId = "pre-arrival-1",
                        NotificationType = "PreArrival",
                        Title = "Pre-Arrival Welcome",
                        MessageTemplate = "Dear {GuestName}, we are excited to welcome you to our hotel on {CheckInDate}. Your room {RoomNumber} is ready for you. We look forward to making your stay memorable!",
                        Channel = "Email",
                        Placeholders = new Dictionary<string, string>
                        {
                            { "GuestName", "Misafir adı" },
                            { "CheckInDate", "Check-in tarihi" },
                            { "RoomNumber", "Oda numarası" }
                        }
                    },
                    new NotificationTemplateDto
                    {
                        TemplateId = "arrival-1",
                        NotificationType = "Arrival",
                        Title = "Welcome Message",
                        MessageTemplate = "Welcome {GuestName}! We hope you enjoy your stay in room {RoomNumber}. If you need anything, please don't hesitate to contact our concierge.",
                        Channel = "SMS",
                        Placeholders = new Dictionary<string, string>
                        {
                            { "GuestName", "Misafir adı" },
                            { "RoomNumber", "Oda numarası" }
                        }
                    },
                    new NotificationTemplateDto
                    {
                        TemplateId = "pre-departure-1",
                        NotificationType = "PreDeparture",
                        Title = "Thank You Message",
                        MessageTemplate = "Dear {GuestName}, thank you for staying with us! We hope you had a wonderful experience. Check-out time is {CheckOutTime}. We look forward to welcoming you back!",
                        Channel = "Email",
                        Placeholders = new Dictionary<string, string>
                        {
                            { "GuestName", "Misafir adı" },
                            { "CheckOutTime", "Check-out saati" }
                        }
                    }
                };

                if (!string.IsNullOrEmpty(notificationType))
                {
                    templates = templates.Where(t => t.NotificationType == notificationType).ToList();
                }

                return ApiResponse<List<NotificationTemplateDto>>.SuccessResponse(templates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification templates");
                return ApiResponse<List<NotificationTemplateDto>>.Fail($"Failed to get notification templates: {ex.Message}");
            }
        }

        // Helper methods
        private string BuildPreArrivalMessage(Application.Operations.Dashboard.CheckInOutItemDto checkIn)
        {
            return $"Dear {checkIn.GuestName}, we are excited to welcome you to our hotel on {checkIn.CheckInDate:dd MMMM yyyy}. " +
                   $"Your room {checkIn.RoomNumber} is ready for you. We look forward to making your stay memorable!";
        }

        private string BuildArrivalMessage(Application.Operations.Dashboard.ActiveGuestDto guest)
        {
            return $"Welcome {guest.GuestName}! We hope you enjoy your stay in room {guest.RoomNumber}. " +
                   $"If you need anything, please don't hesitate to contact our concierge.";
        }

        private string BuildDuringStayMessage(Application.Operations.Dashboard.ActiveGuestDto guest, List<Application.Operations.Dashboard.UpcomingServiceItemDto> services)
        {
            var serviceList = string.Join(", ", services.Select(s => $"{s.ServiceType} at {s.ServiceDate:HH:mm}"));
            return $"Dear {guest.GuestName}, this is a friendly reminder about your upcoming services: {serviceList}. " +
                   $"We look forward to serving you!";
        }

        private string BuildPreDepartureMessage(Application.Operations.Dashboard.CheckInOutItemDto checkOut)
        {
            return $"Dear {checkOut.GuestName}, thank you for staying with us! We hope you had a wonderful experience. " +
                   $"Check-out time is {checkOut.CheckOutDate:HH:mm}. We look forward to welcoming you back!";
        }

        private string DeterminePreferredChannel(int guestId)
        {
            // Misafirin iletişim tercihlerine göre kanal belirle
            // Şimdilik default olarak Email döndür
            // TODO: Guest preferences'dan iletişim tercihini al
            return "Email";
        }
    }
}
