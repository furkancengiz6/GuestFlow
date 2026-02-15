// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.WhatsApp;
using GuestFlow.Application.Operations.WhatsApp.Dtos;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Notification.Dtos;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using GuestFlow.Domain.Entities.Intelligence;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Intelligence.Proactive
{
    public class AutomaticActionService : IAutomaticActionService
    {
        private readonly IWhatsAppService _whatsAppService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AutomaticActionService> _logger;

        public AutomaticActionService(
            IWhatsAppService whatsAppService,
            INotificationService notificationService,
            IUnitOfWork unitOfWork,
            ILogger<AutomaticActionService> logger)
        {
            _whatsAppService = whatsAppService;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> ExecuteActionAsync(AutomaticAction action)
        {
            bool success = false;
            string? details = null;

            try
            {
                _logger.LogInformation("Executing automatic action: {ActionType} for GuestId={GuestId}", action.ActionType, action.GuestId);

                switch (action.ActionType.ToLower())
                {
                    case "message":
                        success = await ExecuteMessageActionAsync(action);
                        break;
                    case "service":
                        success = await ExecuteServiceActionAsync(action);
                        break;
                    case "upgrade":
                    case "discount":
                        success = await ExecutePromotionActionAsync(action);
                        break;
                    default:
                        _logger.LogWarning("Unknown action type: {ActionType}", action.ActionType);
                        details = "Unknown action type";
                        return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing automatic action: {ActionType}", action.ActionType);
                details = ex.Message;
                return false;
            }
            finally
            {
                // Log action to history
                await LogActionHistoryAsync(action, success, details);
            }

            return success;
        }

        private async Task LogActionHistoryAsync(AutomaticAction action, bool success, string? details)
        {
            try
            {
                var historyEntry = new GuestIntelligenceActionEntity
                {
                    GuestId = action.GuestId,
                    ActionType = action.ActionType,
                    Title = action.Title,
                    Description = action.Description,
                    IsAutomatic = action.CanExecuteAutomatically,
                    Status = success ? "Success" : "Failed",
                    Confidence = action.Confidence,
                    ExecutionDetails = details ?? (success ? "Executed successfully" : "Execution failed"),
                    ExecutionDate = DateTime.UtcNow
                };

                await _unitOfWork.GuestIntelligenceActions.AddAsync(historyEntry);
                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log automatic action history for GuestId={GuestId}", action.GuestId);
            }
        }

        private async Task<bool> ExecuteMessageActionAsync(AutomaticAction action)
        {
            var guest = await _unitOfWork.Guests.GetByIdAsync(action.GuestId);
            if (guest == null || string.IsNullOrEmpty(guest.PhoneNumber))
            {
                _logger.LogWarning("Cannot execute message action: Guest not found or no phone number.");
                return false;
            }

            var result = await _whatsAppService.SendWhatsAppAsync(new SendWhatsAppDto
            {
                GuestId = action.GuestId,
                PhoneNumber = guest.PhoneNumber,
                Message = action.Description,
                MessageType = WhatsAppMessageType.Text,
                TemplateName = "AIIntervention"
            });

            return result.IsSuccess;
        }

        private async Task<bool> ExecuteServiceActionAsync(AutomaticAction action)
        {
            // Create a notification for staff to handle the service request
            var result = await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
            {
                Title = $"AI Service Request: {action.Title}",
                Content = $"System recommends: {action.Description}. Action context: {action.ExecutionDetails}",
                NotificationType = "Push",
                RelatedEntityType = "Guest",
                RelatedEntityId = action.GuestId
            });

            return true; // Notification sent
        }

        private async Task<bool> ExecutePromotionActionAsync(AutomaticAction action)
        {
            // For now, create a high-priority notification for management to approve the upgrade/discount
            await _notificationService.CreateAndSendNotificationAsync(new CreateNotificationDto
            {
                Title = $"⭐ Priority Recommendation: {action.Title}",
                Content = $"AI recommends {action.ActionType}: {action.Description}. Confidence: {action.Confidence:P}",
                NotificationType = "Push",
                RelatedEntityType = "Guest",
                RelatedEntityId = action.GuestId
            });

            return true;
        }
    }
}
