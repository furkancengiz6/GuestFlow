// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.NotificationRules.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.NotificationRules
{
    /// <summary>
    /// Notification Rule servisi - Bildirim kurallarını yönetir
    /// </summary>
    public interface INotificationRuleService
    {
        /// <summary>
        /// Tüm aktif kuralları getirir
        /// </summary>
        Task<ApiResponse<List<NotificationRuleDto>>> GetAllRulesAsync(bool? isActive = null);

        /// <summary>
        /// Kural ID'sine göre getirir
        /// </summary>
        Task<ApiResponse<NotificationRuleDto>> GetRuleByIdAsync(int ruleId);

        /// <summary>
        /// Yeni kural oluşturur
        /// </summary>
        Task<ApiResponse<NotificationRuleDto>> CreateRuleAsync(UpsertNotificationRuleDto dto);

        /// <summary>
        /// Kural günceller
        /// </summary>
        Task<ApiResponse<NotificationRuleDto>> UpdateRuleAsync(int ruleId, UpsertNotificationRuleDto dto);

        /// <summary>
        /// Kural siler
        /// </summary>
        Task<ApiResponse<bool>> DeleteRuleAsync(int ruleId);

        /// <summary>
        /// Kuralı aktif/pasif yapar
        /// </summary>
        Task<ApiResponse<bool>> ToggleRuleAsync(int ruleId, bool isActive);

        /// <summary>
        /// Belirli bir kuralı manuel olarak çalıştırır (test için)
        /// </summary>
        Task<ApiResponse<RuleExecutionResult>> ExecuteRuleAsync(int ruleId);

        /// <summary>
        /// Tüm aktif kuralları çalıştırır (background service için)
        /// </summary>
        Task<ApiResponse<List<RuleExecutionResult>>> ExecuteAllActiveRulesAsync();
    }
}
