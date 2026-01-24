// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.NotificationRules;
using GuestFlow.Application.Operations.NotificationRules.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Staff")]
    [Tags("Bildirim Kuralları")]
    public class NotificationRulesController : BaseController
    {
        private readonly INotificationRuleService _ruleService;

        public NotificationRulesController(INotificationRuleService ruleService)
        {
            _ruleService = ruleService;
        }

        /// <summary>
        /// Tüm bildirim kurallarını getirir
        /// </summary>
        /// <param name="isActive">Sadece aktif kuralları getir (opsiyonel)</param>
        /// <returns>Bildirim kuralları listesi</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationRuleDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllRules([FromQuery] bool? isActive = null)
        {
            try
            {
                var result = await _ruleService.GetAllRulesAsync(isActive);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralları getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Kural ID'sine göre getirir
        /// </summary>
        /// <param name="id">Kural ID</param>
        /// <returns>Bildirim kuralı</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationRuleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRuleById(int id)
        {
            try
            {
                var result = await _ruleService.GetRuleByIdAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı getirilirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Yeni bildirim kuralı oluşturur
        /// </summary>
        /// <param name="dto">Kural bilgileri</param>
        /// <returns>Oluşturulan kural</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NotificationRuleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateRule([FromBody] UpsertNotificationRuleDto dto)
        {
            try
            {
                var result = await _ruleService.CreateRuleAsync(dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı oluşturulurken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim kuralını günceller
        /// </summary>
        /// <param name="id">Kural ID</param>
        /// <param name="dto">Güncellenecek kural bilgileri</param>
        /// <returns>Güncellenen kural</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationRuleDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateRule(int id, [FromBody] UpsertNotificationRuleDto dto)
        {
            try
            {
                var result = await _ruleService.UpdateRuleAsync(id, dto);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim kuralını siler
        /// </summary>
        /// <param name="id">Kural ID</param>
        /// <returns>Silme sonucu</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteRule(int id)
        {
            try
            {
                var result = await _ruleService.DeleteRuleAsync(id);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı silinirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Bildirim kuralını aktif/pasif yapar
        /// </summary>
        /// <param name="id">Kural ID</param>
        /// <param name="isActive">Aktif mi?</param>
        /// <returns>Güncelleme sonucu</returns>
        [HttpPatch("{id}/toggle")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ToggleRule(int id, [FromQuery] bool isActive)
        {
            try
            {
                var result = await _ruleService.ToggleRuleAsync(id, isActive);
                return result.Success ? Ok(result) : NotFound(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı güncellenirken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir kuralı manuel olarak çalıştırır (test için)
        /// </summary>
        /// <param name="id">Kural ID</param>
        /// <returns>Kural çalıştırma sonucu</returns>
        [HttpPost("{id}/execute")]
        [ProducesResponseType(typeof(ApiResponse<RuleExecutionResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExecuteRule(int id)
        {
            try
            {
                var result = await _ruleService.ExecuteRuleAsync(id);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralı çalıştırılırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Tüm aktif kuralları manuel olarak çalıştırır (test için)
        /// </summary>
        /// <returns>Tüm kuralların çalıştırma sonuçları</returns>
        [HttpPost("execute-all")]
        [ProducesResponseType(typeof(ApiResponse<List<RuleExecutionResult>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExecuteAllRules()
        {
            try
            {
                var result = await _ruleService.ExecuteAllActiveRulesAsync();
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return Error("Bildirim kuralları çalıştırılırken bir hata oluştu.", 500, new { Error = ex.Message });
            }
        }
    }
}
