// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application.Operations.Housekeeping;
using GuestFlow.Application.Operations.Housekeeping.Dtos;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Kat Hizmetleri (Housekeeping), Bakım ve Kayıp Eşya yönetimi için API endpoint'leri
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize] // Temel yetkilendirme gerekli
    [Tags("Kat Hizmetleri & Bakım")]
    public class HousekeepingController : BaseController
    {
        private readonly IHousekeepingService _housekeepingService;
        private readonly ILogger<HousekeepingController> _logger;

        public HousekeepingController(
            IHousekeepingService housekeepingService,
            ILogger<HousekeepingController> logger)
        {
            _housekeepingService = housekeepingService;
            _logger = logger;
        }

        #region Room Status Endpoints

        /// <summary>
        /// Tüm oda durumlarını getirir
        /// </summary>
        [HttpGet("rooms")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> GetRoomStatuses([FromQuery] int? hotelId = null, [FromQuery] RoomCleaningStatus? cleaningStatus = null, [FromQuery] RoomOccupancyStatus? occupancyStatus = null)
        {
            var result = await _housekeepingService.GetRoomStatusesAsync(hotelId, cleaningStatus, occupancyStatus);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Belirli bir oda durumunu ID'ye göre getirir
        /// </summary>
        [HttpGet("rooms/{id}")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> GetRoomStatus(int id)
        {
            var result = await _housekeepingService.GetRoomStatusByIdAsync(id);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Yeni bir oda durumu kaydı oluşturur
        /// </summary>
        [HttpPost("rooms")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateRoomStatus([FromBody] CreateRoomStatusRequest request)
        {
            var result = await _housekeepingService.CreateRoomStatusAsync(request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Oda durumunu günceller
        /// </summary>
        [HttpPut("rooms/{id}")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> UpdateRoomStatus(int id, [FromBody] UpdateRoomStatusRequest request)
        {
            var result = await _housekeepingService.UpdateRoomStatusAsync(id, request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Odayı bir kat görevlisine atar
        /// </summary>
        [HttpPost("rooms/{id}/assign")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> AssignRoom(int id, [FromBody] AssignRoomRequest request)
        {
            var result = await _housekeepingService.AssignRoomToHousekeeperAsync(id, request.HousekeeperId, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Odayı temizlendi olarak işaretler
        /// </summary>
        [HttpPost("rooms/{id}/cleaned")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> MarkAsCleaned(int id)
        {
            var result = await _housekeepingService.MarkRoomAsCleanedAsync(id, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Oda durumu kaydını siler
        /// </summary>
        [HttpDelete("rooms/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoomStatus(int id)
        {
            var result = await _housekeepingService.DeleteRoomStatusAsync(id);
            return FromServiceMessage(result);
        }

        #endregion

        #region Maintenance Endpoints

        /// <summary>
        /// Bakım taleplerini getirir
        /// </summary>
        [HttpGet("maintenance")]
        [Authorize(Roles = "Admin,Staff,Housekeeper,Technician")]
        public async Task<IActionResult> GetMaintenanceRequests([FromQuery] MaintenanceStatus? status = null, [FromQuery] MaintenancePriority? priority = null, [FromQuery] int? hotelId = null)
        {
            var result = await _housekeepingService.GetMaintenanceRequestsAsync(status, priority, hotelId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Yeni bir bakım talebi oluşturur
        /// </summary>
        [HttpPost("maintenance")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> CreateMaintenanceRequest([FromBody] CreateMaintenanceRequestRequest request)
        {
            var result = await _housekeepingService.CreateMaintenanceRequestAsync(request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bakım talebini günceller
        /// </summary>
        [HttpPut("maintenance/{id}")]
        [Authorize(Roles = "Admin,Staff,Technician")]
        public async Task<IActionResult> UpdateMaintenanceRequest(int id, [FromBody] UpdateMaintenanceRequestRequest request)
        {
            var result = await _housekeepingService.UpdateMaintenanceRequestAsync(id, request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bakım talebini çözüldü olarak işaretler
        /// </summary>
        [HttpPost("maintenance/{id}/resolve")]
        [Authorize(Roles = "Admin,Staff,Technician")]
        public async Task<IActionResult> ResolveMaintenanceRequest(int id, [FromBody] ResolveMaintenanceRequest request)
        {
            var result = await _housekeepingService.ResolveMaintenanceRequestAsync(id, request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Bakım talebini iptal eder
        /// </summary>
        [HttpPost("maintenance/{id}/cancel")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CancelMaintenanceRequest(int id)
        {
            var result = await _housekeepingService.CancelMaintenanceRequestAsync(id, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        #endregion

        #region Lost and Found Endpoints

        /// <summary>
        /// Kayıp eşyaları getirir
        /// </summary>
        [HttpGet("lost-found")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> GetLostAndFoundItems([FromQuery] bool? isReturned = null, [FromQuery] int? hotelId = null)
        {
            var result = await _housekeepingService.GetLostAndFoundItemsAsync(isReturned, hotelId);
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Yeni bir kayıp eşya kaydı oluşturur
        /// </summary>
        [HttpPost("lost-found")]
        [Authorize(Roles = "Admin,Staff,Housekeeper")]
        public async Task<IActionResult> CreateLostAndFoundItem([FromBody] CreateLostAndFoundRequest request)
        {
            var result = await _housekeepingService.CreateLostAndFoundItemAsync(request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        /// <summary>
        /// Eşyayı misafire iade edildi olarak işaretler
        /// </summary>
        [HttpPost("lost-found/{id}/return")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ReturnLostItem(int id, [FromBody] ReturnLostAndFoundRequest request)
        {
            var result = await _housekeepingService.ReturnLostAndFoundItemAsync(id, request, GetCurrentPersonnelId());
            return FromServiceMessage(result);
        }

        #endregion

        #region Helper Methods

        private int GetCurrentPersonnelId()
        {
            var personnelIdClaim = User.FindFirst("PersonnelId");
            if (personnelIdClaim != null && int.TryParse(personnelIdClaim.Value, out var personnelId))
            {
                return personnelId;
            }
            return 1; // Default to admin if not found in token
        }

        #endregion
    }
}
