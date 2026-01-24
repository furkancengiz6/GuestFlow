// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AutoMapper;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.NotificationRules.Dtos;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Operations.Email;
using GuestFlow.Application.Operations.Sms;
using GuestFlow.Application.Operations.Sms.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.NotificationRules
{
    /// <summary>
    /// Notification Rule servisi implementasyonu
    /// </summary>
    public class NotificationRuleService : INotificationRuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILogger<NotificationRuleService> _logger;

        public NotificationRuleService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationService notificationService,
            IEmailService emailService,
            ISmsService smsService,
            ILogger<NotificationRuleService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<ApiResponse<List<NotificationRuleDto>>> GetAllRulesAsync(bool? isActive = null)
        {
            try
            {
                var query = _unitOfWork.NotificationRules.GetAll(r => !r.IsDeleted);

                if (isActive.HasValue)
                    query = query.Where(r => r.IsActive == isActive.Value);

                var rules = await query
                    .OrderByDescending(r => r.Priority)
                    .ThenBy(r => r.Name)
                    .ToListAsync();

                var dtos = _mapper.Map<List<NotificationRuleDto>>(rules);
                return ApiResponse<List<NotificationRuleDto>>.SuccessResponse(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification rules");
                return ApiResponse<List<NotificationRuleDto>>.Fail($"Failed to get notification rules: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationRuleDto>> GetRuleByIdAsync(int ruleId)
        {
            try
            {
                var rule = await _unitOfWork.NotificationRules.GetByIdAsync(ruleId);
                if (rule == null || rule.IsDeleted)
                    return ApiResponse<NotificationRuleDto>.Fail("Notification rule not found");

                var dto = _mapper.Map<NotificationRuleDto>(rule);
                return ApiResponse<NotificationRuleDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get notification rule {RuleId}", ruleId);
                return ApiResponse<NotificationRuleDto>.Fail($"Failed to get notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationRuleDto>> CreateRuleAsync(UpsertNotificationRuleDto dto)
        {
            try
            {
                var rule = _mapper.Map<NotificationRuleEntity>(dto);
                rule.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.NotificationRules.AddAsync(rule);
                await _unitOfWork.CommitAsync();

                var result = _mapper.Map<NotificationRuleDto>(rule);
                _logger.LogInformation("Notification rule created: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);
                return ApiResponse<NotificationRuleDto>.SuccessResponse(result, "Notification rule created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create notification rule");
                return ApiResponse<NotificationRuleDto>.Fail($"Failed to create notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<NotificationRuleDto>> UpdateRuleAsync(int ruleId, UpsertNotificationRuleDto dto)
        {
            try
            {
                var rule = await _unitOfWork.NotificationRules.GetByIdAsync(ruleId);
                if (rule == null || rule.IsDeleted)
                    return ApiResponse<NotificationRuleDto>.Fail("Notification rule not found");

                _mapper.Map(dto, rule);

                _unitOfWork.NotificationRules.Update(rule);
                await _unitOfWork.CommitAsync();

                var result = _mapper.Map<NotificationRuleDto>(rule);
                _logger.LogInformation("Notification rule updated: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);
                return ApiResponse<NotificationRuleDto>.SuccessResponse(result, "Notification rule updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update notification rule {RuleId}", ruleId);
                return ApiResponse<NotificationRuleDto>.Fail($"Failed to update notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _unitOfWork.NotificationRules.GetByIdAsync(ruleId);
                if (rule == null || rule.IsDeleted)
                    return ApiResponse<bool>.Fail("Notification rule not found");

                rule.IsDeleted = true;
                _unitOfWork.NotificationRules.Update(rule);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Notification rule deleted: {RuleName} (ID: {RuleId})", rule.Name, rule.Id);
                return ApiResponse<bool>.SuccessResponse(true, "Notification rule deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete notification rule {RuleId}", ruleId);
                return ApiResponse<bool>.Fail($"Failed to delete notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> ToggleRuleAsync(int ruleId, bool isActive)
        {
            try
            {
                var rule = await _unitOfWork.NotificationRules.GetByIdAsync(ruleId);
                if (rule == null || rule.IsDeleted)
                    return ApiResponse<bool>.Fail("Notification rule not found");

                rule.IsActive = isActive;
                rule.MarkAsUpdated();

                _unitOfWork.NotificationRules.Update(rule);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Notification rule {RuleId} toggled to {IsActive}", ruleId, isActive);
                return ApiResponse<bool>.SuccessResponse(true, $"Notification rule {(isActive ? "activated" : "deactivated")} successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle notification rule {RuleId}", ruleId);
                return ApiResponse<bool>.Fail($"Failed to toggle notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RuleExecutionResult>> ExecuteRuleAsync(int ruleId)
        {
            try
            {
                var rule = await _unitOfWork.NotificationRules.GetByIdAsync(ruleId);
                if (rule == null || rule.IsDeleted || !rule.IsActive)
                    return ApiResponse<RuleExecutionResult>.Fail("Notification rule not found or inactive");

                var result = await EvaluateAndExecuteRuleAsync(rule);
                rule.LastCheckedAt = DateTime.UtcNow;
                if (result.Triggered)
                {
                    rule.LastTriggeredAt = DateTime.UtcNow;
                    rule.TriggerCount++;
                }
                _unitOfWork.NotificationRules.Update(rule);
                await _unitOfWork.CommitAsync();

                return ApiResponse<RuleExecutionResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute notification rule {RuleId}", ruleId);
                return ApiResponse<RuleExecutionResult>.Fail($"Failed to execute notification rule: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<RuleExecutionResult>>> ExecuteAllActiveRulesAsync()
        {
            try
            {
                var activeRules = await _unitOfWork.NotificationRules
                    .GetAll(r => r.IsActive && !r.IsDeleted)
                    .OrderByDescending(r => r.Priority)
                    .ToListAsync();

                var results = new List<RuleExecutionResult>();

                foreach (var rule in activeRules)
                {
                    try
                    {
                        // Check if rule should be executed based on interval
                        if (rule.LastCheckedAt.HasValue)
                        {
                            var timeSinceLastCheck = DateTime.UtcNow - rule.LastCheckedAt.Value;
                            if (timeSinceLastCheck.TotalMinutes < rule.CheckIntervalMinutes)
                                continue; // Skip if not enough time has passed
                        }

                        var result = await EvaluateAndExecuteRuleAsync(rule);
                        rule.LastCheckedAt = DateTime.UtcNow;
                        if (result.Triggered)
                        {
                            rule.LastTriggeredAt = DateTime.UtcNow;
                            rule.TriggerCount++;
                        }
                        _unitOfWork.NotificationRules.Update(rule);
                        results.Add(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing rule {RuleId}: {RuleName}", rule.Id, rule.Name);
                        results.Add(new RuleExecutionResult
                        {
                            RuleId = rule.Id,
                            RuleName = rule.Name,
                            Triggered = false,
                            ErrorMessage = ex.Message,
                            ExecutedAt = DateTime.UtcNow
                        });
                    }
                }

                await _unitOfWork.CommitAsync();
                return ApiResponse<List<RuleExecutionResult>>.SuccessResponse(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute all active rules");
                return ApiResponse<List<RuleExecutionResult>>.Fail($"Failed to execute all active rules: {ex.Message}");
            }
        }

        private async Task<RuleExecutionResult> EvaluateAndExecuteRuleAsync(NotificationRuleEntity rule)
        {
            var result = new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                // Parse conditions
                var condition = JsonSerializer.Deserialize<RuleCondition>(rule.Conditions);
                if (condition == null)
                {
                    result.ErrorMessage = "Invalid condition format";
                    return result;
                }

                // Evaluate rule based on type
                switch (rule.RuleType)
                {
                    case "OverduePayment":
                        result = await EvaluateOverduePaymentRuleAsync(rule, condition);
                        break;

                    case "UpcomingService":
                        result = await EvaluateUpcomingServiceRuleAsync(rule, condition);
                        break;

                    case "UnassignedDriver":
                        result = await EvaluateUnassignedDriverRuleAsync(rule, condition);
                        break;

                    case "LowInventory":
                        result = await EvaluateLowInventoryRuleAsync(rule, condition);
                        break;

                    default:
                        result.ErrorMessage = $"Unknown rule type: {rule.RuleType}";
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error evaluating rule {RuleId}: {RuleName}", rule.Id, rule.Name);
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<RuleExecutionResult> EvaluateOverduePaymentRuleAsync(NotificationRuleEntity rule, RuleCondition condition)
        {
            var result = new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                // Parse condition parameters
                var parameters = !string.IsNullOrEmpty(rule.Parameters)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Parameters)
                    : new Dictionary<string, object>();

                var daysOverdue = parameters.ContainsKey("DaysOverdue") 
                    ? Convert.ToInt32(parameters["DaysOverdue"]) 
                    : 3;

                var minAmount = parameters.ContainsKey("MinAmount")
                    ? Convert.ToDecimal(parameters["MinAmount"])
                    : 0;

                // Find overdue invoices (based on IssueDate + payment status)
                // Note: InvoicesEntity doesn't have DueDate, so we use IssueDate + daysOverdue
                var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysOverdue);
                var overdueInvoices = await _unitOfWork.Invoices
                    .GetAll(i => !i.IsDeleted &&
                                 i.IssueDate.Date <= cutoffDate &&
                                 i.TotalAmount >= minAmount &&
                                 i.Status == InvoiceStatus.Generated) // Only generated invoices that need payment
                    .Include(i => i.Guest)
                    .ToListAsync();

                // Filter invoices that haven't been fully paid
                // Simplified check: For now, we'll consider all Generated invoices as potentially unpaid
                // In production, you'd use PaymentStatusService to check actual payment status
                var unpaidInvoices = overdueInvoices.ToList();

                result.MatchedEntitiesCount = unpaidInvoices.Count();
                result.Triggered = overdueInvoices.Any();

                if (result.Triggered)
                {
                    foreach (var invoice in unpaidInvoices)
                    {
                        if (invoice.Guest != null)
                        {
                            await SendNotificationForRuleAsync(rule, invoice.Guest, invoice, "Invoice");
                            result.NotificationsSent++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<RuleExecutionResult> EvaluateUpcomingServiceRuleAsync(NotificationRuleEntity rule, RuleCondition condition)
        {
            var result = new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                var parameters = !string.IsNullOrEmpty(rule.Parameters)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Parameters)
                    : new Dictionary<string, object>();

                var hoursBefore = parameters.ContainsKey("HoursBefore")
                    ? Convert.ToInt32(parameters["HoursBefore"])
                    : 24;

                var targetDate = DateTime.UtcNow.AddHours(hoursBefore);

                // Find upcoming transfers
                var upcomingTransfers = await _unitOfWork.Transfers
                    .GetAll(t => !t.IsDeleted &&
                                 t.TransferDate <= targetDate &&
                                 t.TransferDate > DateTime.UtcNow &&
                                 t.Status == "Confirmed")
                    .Include(t => t.Guest)
                    .ToListAsync();

                result.MatchedEntitiesCount = upcomingTransfers.Count;
                result.Triggered = upcomingTransfers.Any();

                if (result.Triggered)
                {
                    foreach (var transfer in upcomingTransfers)
                    {
                        if (transfer.Guest != null)
                        {
                            await SendNotificationForRuleAsync(rule, transfer.Guest, transfer, "Transfer");
                            result.NotificationsSent++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<RuleExecutionResult> EvaluateUnassignedDriverRuleAsync(NotificationRuleEntity rule, RuleCondition condition)
        {
            var result = new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                ExecutedAt = DateTime.UtcNow
            };

            try
            {
                var parameters = !string.IsNullOrEmpty(rule.Parameters)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(rule.Parameters)
                    : new Dictionary<string, object>();

                var hoursBefore = parameters.ContainsKey("HoursBefore")
                    ? Convert.ToInt32(parameters["HoursBefore"])
                    : 2;

                var targetDate = DateTime.UtcNow.AddHours(hoursBefore);

                // Find transfers without assigned driver
                var unassignedTransfers = await _unitOfWork.Transfers
                    .GetAll(t => !t.IsDeleted &&
                                 t.TransferDate <= targetDate &&
                                 t.TransferDate > DateTime.UtcNow &&
                                 t.Status == "Confirmed" &&
                                 (t.DriverId == null || t.DriverId == 0))
                    .ToListAsync();

                result.MatchedEntitiesCount = unassignedTransfers.Count;
                result.Triggered = unassignedTransfers.Any();

                if (result.Triggered)
                {
                    // Send notification to admin/personnel
                    var notification = new Notification.Dtos.CreateNotificationDto
                    {
                        Title = $"Uyarı: {unassignedTransfers.Count} transfer için şoför atanmamış",
                        Content = $"{unassignedTransfers.Count} transfer için şoför atanmamış. Lütfen kontrol edin.",
                        NotificationType = rule.NotificationChannel,
                        RecipientPersonnelId = rule.RecipientId, // Use RecipientId for personnel
                        RelatedEntityType = "Transfer",
                        TemplateName = rule.TemplateName
                    };

                    await _notificationService.CreateAndSendNotificationAsync(notification);
                    result.NotificationsSent++;
                }
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private async Task<RuleExecutionResult> EvaluateLowInventoryRuleAsync(NotificationRuleEntity rule, RuleCondition condition)
        {
            // Placeholder for future implementation
            return new RuleExecutionResult
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                Triggered = false,
                ExecutedAt = DateTime.UtcNow
            };
        }

        private async Task SendNotificationForRuleAsync(NotificationRuleEntity rule, GuestEntity guest, object relatedEntity, string entityType)
        {
            try
            {
                var title = rule.Name;
                var content = rule.Description ?? $"Kural tetiklendi: {rule.Name}";

                // Template kullanılıyorsa render et
                if (!string.IsNullOrEmpty(rule.TemplateName))
                {
                    // TODO: Template rendering logic
                }

                // Notification gönder
                if (rule.NotificationChannel.Contains("Email") && !string.IsNullOrEmpty(guest.Email))
                {
                    await _emailService.SendEmailAsync(
                        to: guest.Email,
                        subject: title,
                        body: content,
                        isHtml: true
                    );
                }

                if (rule.NotificationChannel.Contains("SMS") && !string.IsNullOrEmpty(guest.PhoneNumber))
                {
                    var smsMessage = content.Length > 160 ? content.Substring(0, 157) + "..." : content;
                    await _smsService.SendSmsAsync(new SendSmsDto
                    {
                        PhoneNumber = guest.PhoneNumber,
                        Message = smsMessage,
                        GuestId = guest.Id,
                        TemplateName = rule.TemplateName
                    });
                }

                // In-app notification
                await _notificationService.CreateAndSendNotificationAsync(new Notification.Dtos.CreateNotificationDto
                {
                    Title = title,
                    Content = content,
                    NotificationType = "InApp",
                    RecipientGuestId = guest.Id,
                    RelatedEntityType = entityType,
                    TemplateName = rule.TemplateName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification for rule {RuleId} to guest {GuestId}", rule.Id, guest.Id);
            }
        }
    }
}
