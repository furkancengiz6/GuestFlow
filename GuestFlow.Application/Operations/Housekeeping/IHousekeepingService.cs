// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Housekeeping.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.Housekeeping
{
    public interface IHousekeepingService
    {
        // Room Status Management
        Task<ServiceMessage<List<RoomStatusDto>>> GetRoomStatusesAsync(int? hotelId = null, RoomCleaningStatus? cleaningStatus = null, RoomOccupancyStatus? occupancyStatus = null);
        Task<ServiceMessage<RoomStatusDto>> GetRoomStatusByIdAsync(int id);
        Task<ServiceMessage<RoomStatusDto>> GetRoomStatusByRoomNumberAsync(string roomNumber, int? hotelId = null);
        Task<ServiceMessage<RoomStatusDto>> CreateRoomStatusAsync(CreateRoomStatusRequest request, int createdByPersonnelId);
        Task<ServiceMessage<RoomStatusDto>> UpdateRoomStatusAsync(int id, UpdateRoomStatusRequest request, int updatedByPersonnelId);
        Task<ServiceMessage> AssignRoomToHousekeeperAsync(int roomStatusId, int housekeeperId, int assignedByPersonnelId);
        Task<ServiceMessage> MarkRoomAsCleanedAsync(int roomStatusId, int housekeeperId);
        Task<ServiceMessage> DeleteRoomStatusAsync(int id);

        // Maintenance Request Management
        Task<ServiceMessage<List<MaintenanceRequestDto>>> GetMaintenanceRequestsAsync(MaintenanceStatus? status = null, MaintenancePriority? priority = null, int? hotelId = null);
        Task<ServiceMessage<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id);
        Task<ServiceMessage<MaintenanceRequestDto>> CreateMaintenanceRequestAsync(CreateMaintenanceRequestRequest request, int reportedByPersonnelId);
        Task<ServiceMessage<MaintenanceRequestDto>> UpdateMaintenanceRequestAsync(int id, UpdateMaintenanceRequestRequest request, int updatedByPersonnelId);
        Task<ServiceMessage<MaintenanceRequestDto>> ResolveMaintenanceRequestAsync(int id, ResolveMaintenanceRequest request, int resolvedByPersonnelId);
        Task<ServiceMessage> CancelMaintenanceRequestAsync(int id, int cancelledByPersonnelId);
        Task<ServiceMessage> DeleteMaintenanceRequestAsync(int id);

        // Lost and Found Management
        Task<ServiceMessage<List<LostAndFoundDto>>> GetLostAndFoundItemsAsync(bool? isReturned = null, int? hotelId = null);
        Task<ServiceMessage<LostAndFoundDto>> GetLostAndFoundItemByIdAsync(int id);
        Task<ServiceMessage<LostAndFoundDto>> CreateLostAndFoundItemAsync(CreateLostAndFoundRequest request, int foundByPersonnelId);
        Task<ServiceMessage<LostAndFoundDto>> UpdateLostAndFoundItemAsync(int id, UpdateLostAndFoundRequest request, int updatedByPersonnelId);
        Task<ServiceMessage<LostAndFoundDto>> ReturnLostAndFoundItemAsync(int id, ReturnLostAndFoundRequest request, int returnedByPersonnelId);
        Task<ServiceMessage> DeleteLostAndFoundItemAsync(int id);
    }
}
