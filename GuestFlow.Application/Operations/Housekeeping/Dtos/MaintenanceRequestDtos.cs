// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Domain.Entities.Operations;

namespace GuestFlow.Application.Operations.Housekeeping.Dtos
{
    public class MaintenanceRequestDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public string PriorityDisplay { get; set; } = string.Empty;
        public MaintenanceStatus Status { get; set; }
        public string StatusDisplay { get; set; } = string.Empty;
        public DateTime ReportedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? ResolutionNotes { get; set; }
        public int ReportedByPersonnelId { get; set; }
        public string ReportedByPersonnelName { get; set; } = string.Empty;
        public int? AssignedToPersonnelId { get; set; }
        public string? AssignedToPersonnelName { get; set; }
        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
    }

    public class CreateMaintenanceRequestRequest
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public MaintenancePriority Priority { get; set; }
        public int? AssignedToPersonnelId { get; set; }
        public int? HotelId { get; set; }
    }

    public class UpdateMaintenanceRequestRequest
    {
        public MaintenanceStatus? Status { get; set; }
        public MaintenancePriority? Priority { get; set; }
        public int? AssignedToPersonnelId { get; set; }
        public string? ResolutionNotes { get; set; }
    }

    public class ResolveMaintenanceRequest
    {
        public string ResolutionNotes { get; set; } = string.Empty;
    }
}
