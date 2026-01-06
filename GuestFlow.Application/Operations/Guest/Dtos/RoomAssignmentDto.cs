using System;

namespace GuestFlow.Application.Operations.Guest.Dtos
{
    public class RoomAssignmentDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public int? HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
    }

    public class CreateRoomAssignmentDto
    {
        public int GuestId { get; set; }
        public int? HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public int? PersonnelId { get; set; }
    }

    public class UpdateRoomAssignmentDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public int? PersonnelId { get; set; }
    }

    public class CloseRoomAssignmentDto
    {
        public DateTime EndDate { get; set; }
        public string? Notes { get; set; }
        public int? PersonnelId { get; set; }
    }

    public class RoomContextRequestDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? HotelId { get; set; }
    }

    public class RoomContextDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public DateTime SearchStartDate { get; set; }
        public DateTime SearchEndDate { get; set; }

        // Guests assigned to this room during the date range
        public List<GuestAssignmentDto> Guests { get; set; } = new List<GuestAssignmentDto>();

        // Services provided during the date range
        public List<ServiceSummaryDto> Transfers { get; set; } = new List<ServiceSummaryDto>();
        public List<ServiceSummaryDto> CityTours { get; set; } = new List<ServiceSummaryDto>();
        public List<ServiceSummaryDto> YachtTours { get; set; } = new List<ServiceSummaryDto>();

        // Financial summary
        public RoomFinancialSummaryDto FinancialSummary { get; set; } = new RoomFinancialSummaryDto();
    }

    public class GuestAssignmentDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public DateTime AssignmentStart { get; set; }
        public DateTime? AssignmentEnd { get; set; }
        public string? Notes { get; set; }
    }

    public class ServiceSummaryDto
    {
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string GuestName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class RoomFinancialSummaryDto
    {
        public int TotalInvoices { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalInvoicedAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public string Currency { get; set; } = "TRY";
    }
}
