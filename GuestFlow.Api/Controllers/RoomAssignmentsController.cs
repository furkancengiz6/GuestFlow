using GuestFlow.Api.Models;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Oda atamaları ve oda bağlamı yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Staff,Admin")]
    [Tags("Oda Atamaları")]
    public class RoomAssignmentsController : BaseController
    {
        private readonly IRoomAssignmentService _roomAssignmentService;
        private readonly ILogger<RoomAssignmentsController> _logger;

        public RoomAssignmentsController(IRoomAssignmentService roomAssignmentService, ILogger<RoomAssignmentsController> logger)
        {
            _roomAssignmentService = roomAssignmentService;
            _logger = logger;
        }

        /// <summary>
        /// Oda atamasını günceller
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<RoomAssignmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateRoomAssignment(int id, [FromBody] UpdateRoomAssignmentRequest request)
        {
            try
            {
                var currentPersonnelId = GetCurrentPersonnelId();

                var dto = new UpdateRoomAssignmentDto
                {
                    Id = id,
                    RoomNumber = request.RoomNumber,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    PersonnelId = currentPersonnelId
                };

                var result = await _roomAssignmentService.UpdateRoomAssignmentAsync(dto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması güncellenirken hata: {ex.Message}");
                return Error("Oda ataması güncellenirken hata oluştu.", 500);
            }
        }

        /// <summary>
        /// Oda atamasını kapatır (bitiş tarihi ekler)
        /// </summary>
        [HttpPost("{id}/close")]
        [ProducesResponseType(typeof(ApiResponse<RoomAssignmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CloseRoomAssignment(int id, [FromBody] CloseRoomAssignmentRequest request)
        {
            try
            {
                var currentPersonnelId = GetCurrentPersonnelId();

                var dto = new CloseRoomAssignmentDto
                {
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    PersonnelId = currentPersonnelId
                };

                var result = await _roomAssignmentService.CloseRoomAssignmentAsync(id, dto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda ataması kapatılırken hata: {ex.Message}");
                return Error("Oda ataması kapatılırken hata oluştu.", 500);
            }
        }

        /// <summary>
        /// Oda + tarih aralığına göre oda bağlamını getirir
        /// </summary>
        [HttpPost("context")]
        [ProducesResponseType(typeof(ApiResponse<RoomContextDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRoomContext([FromBody] RoomContextRequest request)
        {
            try
            {
                var dto = new RoomContextRequestDto
                {
                    RoomNumber = request.RoomNumber,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    HotelId = request.HotelId
                };

                var result = await _roomAssignmentService.GetRoomContextAsync(dto);
                return FromServiceMessage(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Oda bağlamı getirilirken hata: {ex.Message}");
                return Error("Oda bağlamı getirilirken hata oluştu.", 500);
            }
        }

        private int GetCurrentPersonnelId()
        {
            var personnelIdClaim = User.FindFirst("PersonnelId");
            if (personnelIdClaim != null && int.TryParse(personnelIdClaim.Value, out var personnelId))
            {
                return personnelId;
            }
            return 1; // Default to admin if not found
        }
    }
}