// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.Housekeeping.Dtos
{
    public class RoomStatusDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public RoomCleaningStatus CleaningStatus { get; set; }
        public string CleaningStatusDisplay { get; set; } = string.Empty;
        public RoomOccupancyStatus OccupancyStatus { get; set; }
        public string OccupancyStatusDisplay { get; set; } = string.Empty;
        public DateTime LastCleaned { get; set; }
        public DateTime? NextInspection { get; set; }
        public int? AssignedHousekeeperId { get; set; }
        public string? AssignedHousekeeperName { get; set; }
        public string? Notes { get; set; }
        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
    }

    public class CreateRoomStatusRequest
    {
        public string RoomNumber { get; set; } = string.Empty;
        public RoomCleaningStatus CleaningStatus { get; set; }
        public RoomOccupancyStatus OccupancyStatus { get; set; }
        public DateTime LastCleaned { get; set; }
        public DateTime? NextInspection { get; set; }
        public int? AssignedHousekeeperId { get; set; }
        public string? Notes { get; set; }
        public int? HotelId { get; set; }
    }

    public class UpdateRoomStatusRequest
    {
        public RoomCleaningStatus? CleaningStatus { get; set; }
        public RoomOccupancyStatus? OccupancyStatus { get; set; }
        public DateTime? LastCleaned { get; set; }
        public DateTime? NextInspection { get; set; }
        public int? AssignedHousekeeperId { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignRoomRequest
    {
        public int HousekeeperId { get; set; }
    }
}
