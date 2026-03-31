// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Housekeeping.Dtos;
using GuestFlow.Application.Types;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.Housekeeping
{
    public class HousekeepingService : IHousekeepingService
    {
        private readonly GuestFlowDbContext _context;
        private readonly ILogger<HousekeepingService> _logger;

        public HousekeepingService(
            GuestFlowDbContext context,
            ILogger<HousekeepingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Room Status Management

        public async Task<ServiceMessage<List<RoomStatusDto>>> GetRoomStatusesAsync(
            int? hotelId = null,
            RoomCleaningStatus? cleaningStatus = null,
            RoomOccupancyStatus? occupancyStatus = null)
        {
            try
            {
                var query = _context.RoomStatuses
                    .Include(r => r.AssignedHousekeeper)
                    .Include(r => r.Hotel)
                    .AsQueryable();

                if (hotelId.HasValue)
                    query = query.Where(r => r.HotelId == hotelId.Value);

                if (cleaningStatus.HasValue)
                    query = query.Where(r => r.CleaningStatus == cleaningStatus.Value);

                if (occupancyStatus.HasValue)
                    query = query.Where(r => r.OccupancyStatus == occupancyStatus.Value);

                var roomStatuses = await query
                    .OrderBy(r => r.RoomNumber)
                    .ToListAsync();

                var dtos = roomStatuses.Select(MapToDto).ToList();

                return new ServiceMessage<List<RoomStatusDto>>
                {
                    IsSuccess = true,
                    Message = "Oda durumları başarıyla getirildi",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room statuses");
                return new ServiceMessage<List<RoomStatusDto>>
                {
                    IsSuccess = false,
                    Message = "Oda durumları getirilirken hata oluştu"
                };
            }
        }

        public async Task<ServiceMessage<RoomStatusDto>> GetRoomStatusByIdAsync(int id)
        {
            try
            {
                var roomStatus = await _context.RoomStatuses
                    .Include(r => r.AssignedHousekeeper)
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (roomStatus == null)
                    return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                return new ServiceMessage<RoomStatusDto>
                {
                    IsSuccess = true,
                    Message = "Oda durumu başarıyla getirildi",
                    Data = MapToDto(roomStatus)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room status by id: {Id}", id);
                return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<RoomStatusDto>> GetRoomStatusByRoomNumberAsync(string roomNumber, int? hotelId = null)
        {
            try
            {
                var query = _context.RoomStatuses
                    .Include(r => r.AssignedHousekeeper)
                    .Include(r => r.Hotel)
                    .Where(r => r.RoomNumber == roomNumber);

                if (hotelId.HasValue)
                    query = query.Where(r => r.HotelId == hotelId.Value);

                var roomStatus = await query.FirstOrDefaultAsync();

                if (roomStatus == null)
                    return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                return new ServiceMessage<RoomStatusDto>
                {
                    IsSuccess = true,
                    Message = "Oda durumu başarıyla getirildi",
                    Data = MapToDto(roomStatus)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room status by room number: {RoomNumber}", roomNumber);
                return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<RoomStatusDto>> CreateRoomStatusAsync(CreateRoomStatusRequest request, int createdByPersonnelId)
        {
            try
            {
                var existing = await _context.RoomStatuses
                    .FirstOrDefaultAsync(r => r.RoomNumber == request.RoomNumber && r.HotelId == request.HotelId);

                if (existing != null)
                    return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Bu oda numarası için zaten bir kayıt mevcut" };

                var roomStatus = new RoomStatusEntity
                {
                    RoomNumber = request.RoomNumber,
                    CleaningStatus = request.CleaningStatus,
                    OccupancyStatus = request.OccupancyStatus,
                    LastCleaned = request.LastCleaned,
                    NextInspection = request.NextInspection,
                    AssignedHousekeeperId = request.AssignedHousekeeperId,
                    Notes = request.Notes,
                    HotelId = request.HotelId
                };

                _context.RoomStatuses.Add(roomStatus);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Room status created for room {RoomNumber} by personnel {PersonnelId}",
                    request.RoomNumber, createdByPersonnelId);

                var created = await _context.RoomStatuses
                    .Include(r => r.AssignedHousekeeper)
                    .Include(r => r.Hotel)
                    .FirstAsync(r => r.Id == roomStatus.Id);

                return new ServiceMessage<RoomStatusDto>
                {
                    IsSuccess = true,
                    Message = "Oda durumu başarıyla oluşturuldu",
                    Data = MapToDto(created)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room status");
                return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu oluşturulurken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<RoomStatusDto>> UpdateRoomStatusAsync(int id, UpdateRoomStatusRequest request, int updatedByPersonnelId)
        {
            try
            {
                var roomStatus = await _context.RoomStatuses.FindAsync(id);
                if (roomStatus == null)
                    return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                if (request.CleaningStatus.HasValue)
                    roomStatus.CleaningStatus = request.CleaningStatus.Value;

                if (request.OccupancyStatus.HasValue)
                    roomStatus.OccupancyStatus = request.OccupancyStatus.Value;

                if (request.LastCleaned.HasValue)
                    roomStatus.LastCleaned = request.LastCleaned.Value;

                if (request.NextInspection.HasValue)
                    roomStatus.NextInspection = request.NextInspection.Value;

                if (request.AssignedHousekeeperId.HasValue)
                    roomStatus.AssignedHousekeeperId = request.AssignedHousekeeperId.Value;

                if (request.Notes != null)
                    roomStatus.Notes = request.Notes;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Room status updated for room {RoomNumber} by personnel {PersonnelId}",
                    roomStatus.RoomNumber, updatedByPersonnelId);

                var updated = await _context.RoomStatuses
                    .Include(r => r.AssignedHousekeeper)
                    .Include(r => r.Hotel)
                    .FirstAsync(r => r.Id == id);

                return new ServiceMessage<RoomStatusDto>
                {
                    IsSuccess = true,
                    Message = "Oda durumu başarıyla güncellendi",
                    Data = MapToDto(updated)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room status");
                return new ServiceMessage<RoomStatusDto> { IsSuccess = false, Message = "Oda durumu güncellenirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> AssignRoomToHousekeeperAsync(int roomStatusId, int housekeeperId, int assignedByPersonnelId)
        {
            try
            {
                var roomStatus = await _context.RoomStatuses.FindAsync(roomStatusId);
                if (roomStatus == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                var housekeeper = await _context.Personnels.FindAsync(housekeeperId);
                if (housekeeper == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Personel bulunamadı" };

                roomStatus.AssignedHousekeeperId = housekeeperId;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {RoomNumber} assigned to housekeeper {HousekeeperId} by personnel {PersonnelId}",
                    roomStatus.RoomNumber, housekeeperId, assignedByPersonnelId);

                return new ServiceMessage { IsSuccess = true, Message = "Oda personele başarıyla atandı" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning room to housekeeper");
                return new ServiceMessage { IsSuccess = false, Message = "Oda atanırken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> MarkRoomAsCleanedAsync(int roomStatusId, int housekeeperId)
        {
            try
            {
                var roomStatus = await _context.RoomStatuses.FindAsync(roomStatusId);
                if (roomStatus == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                roomStatus.CleaningStatus = RoomCleaningStatus.Clean;
                roomStatus.LastCleaned = DateTime.UtcNow;
                roomStatus.NextInspection = DateTime.UtcNow.AddHours(24);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Room {RoomNumber} marked as cleaned by housekeeper {HousekeeperId}",
                    roomStatus.RoomNumber, housekeeperId);

                return new ServiceMessage { IsSuccess = true, Message = "Oda temizlendi olarak işaretlendi" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking room as cleaned");
                return new ServiceMessage { IsSuccess = false, Message = "Oda temizlendi olarak işaretlenirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> DeleteRoomStatusAsync(int id)
        {
            try
            {
                var roomStatus = await _context.RoomStatuses.FindAsync(id);
                if (roomStatus == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Oda durumu bulunamadı" };

                _context.RoomStatuses.Remove(roomStatus);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Room status deleted for room {RoomNumber}", roomStatus.RoomNumber);

                return new ServiceMessage { IsSuccess = true, Message = "Oda durumu başarıyla silindi" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room status");
                return new ServiceMessage { IsSuccess = false, Message = "Oda durumu silinirken hata oluştu" };
            }
        }

        #endregion

        #region Maintenance Request Management

        public async Task<ServiceMessage<List<MaintenanceRequestDto>>> GetMaintenanceRequestsAsync(
            MaintenanceStatus? status = null,
            MaintenancePriority? priority = null,
            int? hotelId = null)
        {
            try
            {
                var query = _context.MaintenanceRequests
                    .Include(m => m.ReportedByPersonnel)
                    .Include(m => m.AssignedToPersonnel)
                    .Include(m => m.Hotel)
                    .AsQueryable();

                if (status.HasValue)
                    query = query.Where(m => m.Status == status.Value);

                if (priority.HasValue)
                    query = query.Where(m => m.Priority == priority.Value);

                if (hotelId.HasValue)
                    query = query.Where(m => m.HotelId == hotelId.Value);

                var requests = await query
                    .OrderByDescending(m => m.Priority)
                    .ThenBy(m => m.ReportedDate)
                    .ToListAsync();

                var dtos = requests.Select(MapToDto).ToList();

                return new ServiceMessage<List<MaintenanceRequestDto>>
                {
                    IsSuccess = true,
                    Message = "Bakım talepleri başarıyla getirildi",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance requests");
                return new ServiceMessage<List<MaintenanceRequestDto>> { IsSuccess = false, Message = "Bakım talepleri getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<MaintenanceRequestDto>> GetMaintenanceRequestByIdAsync(int id)
        {
            try
            {
                var request = await _context.MaintenanceRequests
                    .Include(m => m.ReportedByPersonnel)
                    .Include(m => m.AssignedToPersonnel)
                    .Include(m => m.Hotel)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (request == null)
                    return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi bulunamadı" };

                return new ServiceMessage<MaintenanceRequestDto>
                {
                    IsSuccess = true,
                    Message = "Bakım talebi başarıyla getirildi",
                    Data = MapToDto(request)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance request by id: {Id}", id);
                return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<MaintenanceRequestDto>> CreateMaintenanceRequestAsync(
            CreateMaintenanceRequestRequest request,
            int reportedByPersonnelId)
        {
            try
            {
                var maintenanceRequest = new MaintenanceRequestEntity
                {
                    RoomNumber = request.RoomNumber,
                    IssueDescription = request.IssueDescription,
                    Priority = request.Priority,
                    Status = MaintenanceStatus.Pending,
                    ReportedDate = DateTime.UtcNow,
                    ReportedByPersonnelId = reportedByPersonnelId,
                    AssignedToPersonnelId = request.AssignedToPersonnelId,
                    HotelId = request.HotelId
                };

                _context.MaintenanceRequests.Add(maintenanceRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Maintenance request created for room {RoomNumber} by personnel {PersonnelId}",
                    request.RoomNumber, reportedByPersonnelId);

                var created = await _context.MaintenanceRequests
                    .Include(m => m.ReportedByPersonnel)
                    .Include(m => m.AssignedToPersonnel)
                    .Include(m => m.Hotel)
                    .FirstAsync(m => m.Id == maintenanceRequest.Id);

                return new ServiceMessage<MaintenanceRequestDto>
                {
                    IsSuccess = true,
                    Message = "Bakım talebi başarıyla oluşturuldu",
                    Data = MapToDto(created)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating maintenance request");
                return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi oluşturulurken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<MaintenanceRequestDto>> UpdateMaintenanceRequestAsync(
            int id,
            UpdateMaintenanceRequestRequest request,
            int updatedByPersonnelId)
        {
            try
            {
                var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
                if (maintenanceRequest == null)
                    return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi bulunamadı" };

                if (request.Status.HasValue)
                {
                    maintenanceRequest.Status = request.Status.Value;
                    if (request.Status.Value == MaintenanceStatus.InProgress && maintenanceRequest.Status == MaintenanceStatus.Pending)
                    {
                        _logger.LogInformation("Maintenance request {Id} status changed to InProgress", id);
                    }
                }

                if (request.Priority.HasValue)
                    maintenanceRequest.Priority = request.Priority.Value;

                if (request.AssignedToPersonnelId.HasValue)
                    maintenanceRequest.AssignedToPersonnelId = request.AssignedToPersonnelId.Value;

                if (request.ResolutionNotes != null)
                    maintenanceRequest.ResolutionNotes = request.ResolutionNotes;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Maintenance request {Id} updated by personnel {PersonnelId}", id, updatedByPersonnelId);

                var updated = await _context.MaintenanceRequests
                    .Include(m => m.ReportedByPersonnel)
                    .Include(m => m.AssignedToPersonnel)
                    .Include(m => m.Hotel)
                    .FirstAsync(m => m.Id == id);

                return new ServiceMessage<MaintenanceRequestDto>
                {
                    IsSuccess = true,
                    Message = "Bakım talebi başarıyla güncellendi",
                    Data = MapToDto(updated)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating maintenance request");
                return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi güncellenirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<MaintenanceRequestDto>> ResolveMaintenanceRequestAsync(
            int id,
            ResolveMaintenanceRequest request,
            int resolvedByPersonnelId)
        {
            try
            {
                var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
                if (maintenanceRequest == null)
                    return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi bulunamadı" };

                maintenanceRequest.Status = MaintenanceStatus.Resolved;
                maintenanceRequest.ResolvedDate = DateTime.UtcNow;
                maintenanceRequest.ResolutionNotes = request.ResolutionNotes;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Maintenance request {Id} resolved by personnel {PersonnelId}", id, resolvedByPersonnelId);

                var resolved = await _context.MaintenanceRequests
                    .Include(m => m.ReportedByPersonnel)
                    .Include(m => m.AssignedToPersonnel)
                    .Include(m => m.Hotel)
                    .FirstAsync(m => m.Id == id);

                return new ServiceMessage<MaintenanceRequestDto>
                {
                    IsSuccess = true,
                    Message = "Bakım talebi başarıyla çözüldü",
                    Data = MapToDto(resolved)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving maintenance request");
                return new ServiceMessage<MaintenanceRequestDto> { IsSuccess = false, Message = "Bakım talebi çözülürken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> CancelMaintenanceRequestAsync(int id, int cancelledByPersonnelId)
        {
            try
            {
                var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
                if (maintenanceRequest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bakım talebi bulunamadı" };

                maintenanceRequest.Status = MaintenanceStatus.Cancelled;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Maintenance request {Id} cancelled by personnel {PersonnelId}", id, cancelledByPersonnelId);

                return new ServiceMessage { IsSuccess = true, Message = "Bakım talebi başarıyla iptal edildi" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling maintenance request");
                return new ServiceMessage { IsSuccess = false, Message = "Bakım talebi iptal edilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> DeleteMaintenanceRequestAsync(int id)
        {
            try
            {
                var maintenanceRequest = await _context.MaintenanceRequests.FindAsync(id);
                if (maintenanceRequest == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Bakım talebi bulunamadı" };

                _context.MaintenanceRequests.Remove(maintenanceRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Maintenance request {Id} deleted", id);

                return new ServiceMessage { IsSuccess = true, Message = "Bakım talebi başarıyla silindi" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting maintenance request");
                return new ServiceMessage { IsSuccess = false, Message = "Bakım talebi silinirken hata oluştu" };
            }
        }

        #endregion

        #region Lost and Found Management

        public async Task<ServiceMessage<List<LostAndFoundDto>>> GetLostAndFoundItemsAsync(bool? isReturned = null, int? hotelId = null)
        {
            try
            {
                var query = _context.LostAndFoundItems
                    .Include(l => l.FoundByPersonnel)
                    .Include(l => l.Guest)
                    .Include(l => l.Hotel)
                    .AsQueryable();

                if (isReturned.HasValue)
                    query = query.Where(l => l.IsReturned == isReturned.Value);

                if (hotelId.HasValue)
                    query = query.Where(l => l.HotelId == hotelId.Value);

                var items = await query
                    .OrderByDescending(l => l.FoundDate)
                    .ToListAsync();

                var dtos = items.Select(MapToDto).ToList();

                return new ServiceMessage<List<LostAndFoundDto>>
                {
                    IsSuccess = true,
                    Message = "Kayıp eşyalar başarıyla getirildi",
                    Data = dtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lost and found items");
                return new ServiceMessage<List<LostAndFoundDto>> { IsSuccess = false, Message = "Kayıp eşyalar getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<LostAndFoundDto>> GetLostAndFoundItemByIdAsync(int id)
        {
            try
            {
                var item = await _context.LostAndFoundItems
                    .Include(l => l.FoundByPersonnel)
                    .Include(l => l.Guest)
                    .Include(l => l.Hotel)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (item == null)
                    return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya bulunamadı" };

                return new ServiceMessage<LostAndFoundDto>
                {
                    IsSuccess = true,
                    Message = "Kayıp eşya başarıyla getirildi",
                    Data = MapToDto(item)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lost and found item by id: {Id}", id);
                return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya getirilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<LostAndFoundDto>> CreateLostAndFoundItemAsync(
            CreateLostAndFoundRequest request,
            int foundByPersonnelId)
        {
            try
            {
                var item = new LostAndFoundEntity
                {
                    ItemDescription = request.ItemDescription,
                    RoomNumber = request.RoomNumber,
                    FoundDate = request.FoundDate,
                    StorageLocation = request.StorageLocation,
                    ItemCategory = request.ItemCategory,
                    FoundByPersonnelId = foundByPersonnelId,
                    GuestId = request.GuestId,
                    HotelId = request.HotelId,
                    IsReturned = false
                };

                _context.LostAndFoundItems.Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lost and found item created for room {RoomNumber} by personnel {PersonnelId}",
                    request.RoomNumber, foundByPersonnelId);

                var created = await _context.LostAndFoundItems
                    .Include(l => l.FoundByPersonnel)
                    .Include(l => l.Guest)
                    .Include(l => l.Hotel)
                    .FirstAsync(l => l.Id == item.Id);

                return new ServiceMessage<LostAndFoundDto>
                {
                    IsSuccess = true,
                    Message = "Kayıp eşya başarıyla oluşturuldu",
                    Data = MapToDto(created)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lost and found item");
                return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya oluşturulurken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<LostAndFoundDto>> UpdateLostAndFoundItemAsync(
            int id,
            UpdateLostAndFoundRequest request,
            int updatedByPersonnelId)
        {
            try
            {
                var item = await _context.LostAndFoundItems.FindAsync(id);
                if (item == null)
                    return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya bulunamadı" };

                if (request.ItemDescription != null)
                    item.ItemDescription = request.ItemDescription;

                if (request.StorageLocation != null)
                    item.StorageLocation = request.StorageLocation;

                if (request.ItemCategory != null)
                    item.ItemCategory = request.ItemCategory;

                if (request.GuestId.HasValue)
                    item.GuestId = request.GuestId.Value;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Lost and found item {Id} updated by personnel {PersonnelId}", id, updatedByPersonnelId);

                var updated = await _context.LostAndFoundItems
                    .Include(l => l.FoundByPersonnel)
                    .Include(l => l.Guest)
                    .Include(l => l.Hotel)
                    .FirstAsync(l => l.Id == id);

                return new ServiceMessage<LostAndFoundDto>
                {
                    IsSuccess = true,
                    Message = "Kayıp eşya başarıyla güncellendi",
                    Data = MapToDto(updated)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lost and found item");
                return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya güncellenirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage<LostAndFoundDto>> ReturnLostAndFoundItemAsync(
            int id,
            ReturnLostAndFoundRequest request,
            int returnedByPersonnelId)
        {
            try
            {
                var item = await _context.LostAndFoundItems.FindAsync(id);
                if (item == null)
                    return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya bulunamadı" };

                item.IsReturned = true;
                item.ReturnedDate = DateTime.UtcNow;
                item.GuestId = request.GuestId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Lost and found item {Id} returned to guest {GuestId} by personnel {PersonnelId}",
                    id, request.GuestId, returnedByPersonnelId);

                var returned = await _context.LostAndFoundItems
                    .Include(l => l.FoundByPersonnel)
                    .Include(l => l.Guest)
                    .Include(l => l.Hotel)
                    .FirstAsync(l => l.Id == id);

                return new ServiceMessage<LostAndFoundDto>
                {
                    IsSuccess = true,
                    Message = "Kayıp eşya başarıyla iade edildi",
                    Data = MapToDto(returned)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning lost and found item");
                return new ServiceMessage<LostAndFoundDto> { IsSuccess = false, Message = "Kayıp eşya iade edilirken hata oluştu" };
            }
        }

        public async Task<ServiceMessage> DeleteLostAndFoundItemAsync(int id)
        {
            try
            {
                var item = await _context.LostAndFoundItems.FindAsync(id);
                if (item == null)
                    return new ServiceMessage { IsSuccess = false, Message = "Kayıp eşya bulunamadı" };

                _context.LostAndFoundItems.Remove(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lost and found item {Id} deleted", id);

                return new ServiceMessage { IsSuccess = true, Message = "Kayıp eşya başarıyla silindi" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lost and found item");
                return new ServiceMessage { IsSuccess = false, Message = "Kayıp eşya silinirken hata oluştu" };
            }
        }

        #endregion

        #region Mapping Methods

        private static RoomStatusDto MapToDto(RoomStatusEntity entity)
        {
            return new RoomStatusDto
            {
                Id = entity.Id,
                RoomNumber = entity.RoomNumber,
                CleaningStatus = entity.CleaningStatus,
                CleaningStatusDisplay = entity.CleaningStatus.ToString(),
                OccupancyStatus = entity.OccupancyStatus,
                OccupancyStatusDisplay = entity.OccupancyStatus.ToString(),
                LastCleaned = entity.LastCleaned,
                NextInspection = entity.NextInspection,
                AssignedHousekeeperId = entity.AssignedHousekeeperId,
                AssignedHousekeeperName = entity.AssignedHousekeeper?.FullName,
                Notes = entity.Notes,
                HotelId = entity.HotelId,
                HotelName = entity.Hotel?.HotelName
            };
        }

        private static MaintenanceRequestDto MapToDto(MaintenanceRequestEntity entity)
        {
            return new MaintenanceRequestDto
            {
                Id = entity.Id,
                RoomNumber = entity.RoomNumber,
                IssueDescription = entity.IssueDescription,
                Priority = entity.Priority,
                PriorityDisplay = entity.Priority.ToString(),
                Status = entity.Status,
                StatusDisplay = entity.Status.ToString(),
                ReportedDate = entity.ReportedDate,
                ResolvedDate = entity.ResolvedDate,
                ResolutionNotes = entity.ResolutionNotes,
                ReportedByPersonnelId = entity.ReportedByPersonnelId,
                ReportedByPersonnelName = entity.ReportedByPersonnel.FullName,
                AssignedToPersonnelId = entity.AssignedToPersonnelId,
                AssignedToPersonnelName = entity.AssignedToPersonnel?.FullName,
                HotelId = entity.HotelId,
                HotelName = entity.Hotel?.HotelName
            };
        }

        private static LostAndFoundDto MapToDto(LostAndFoundEntity entity)
        {
            return new LostAndFoundDto
            {
                Id = entity.Id,
                ItemDescription = entity.ItemDescription,
                RoomNumber = entity.RoomNumber,
                FoundDate = entity.FoundDate,
                IsReturned = entity.IsReturned,
                ReturnedDate = entity.ReturnedDate,
                StorageLocation = entity.StorageLocation,
                ItemCategory = entity.ItemCategory,
                FoundByPersonnelId = entity.FoundByPersonnelId,
                FoundByPersonnelName = entity.FoundByPersonnel.FullName,
                GuestId = entity.GuestId,
                GuestName = entity.Guest?.FullName,
                HotelId = entity.HotelId,
                HotelName = entity.Hotel?.HotelName
            };
        }

        #endregion
    }
}
