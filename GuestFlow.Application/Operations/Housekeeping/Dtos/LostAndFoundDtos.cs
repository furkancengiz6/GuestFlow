// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Operations.Housekeeping.Dtos
{
    public class LostAndFoundDto
    {
        public int Id { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime FoundDate { get; set; }
        public bool IsReturned { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public string? StorageLocation { get; set; }
        public string? ItemCategory { get; set; }
        public int FoundByPersonnelId { get; set; }
        public string FoundByPersonnelName { get; set; } = string.Empty;
        public int? GuestId { get; set; }
        public string? GuestName { get; set; }
        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
    }

    public class CreateLostAndFoundRequest
    {
        public string ItemDescription { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime FoundDate { get; set; }
        public string? StorageLocation { get; set; }
        public string? ItemCategory { get; set; }
        public int? GuestId { get; set; }
        public int? HotelId { get; set; }
    }

    public class UpdateLostAndFoundRequest
    {
        public string? ItemDescription { get; set; }
        public string? StorageLocation { get; set; }
        public string? ItemCategory { get; set; }
        public int? GuestId { get; set; }
    }

    public class ReturnLostAndFoundRequest
    {
        public int GuestId { get; set; }
    }
}
