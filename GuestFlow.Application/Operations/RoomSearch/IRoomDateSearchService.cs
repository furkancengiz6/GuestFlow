using GuestFlow.Application.Models;
using GuestFlow.Application.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.RoomSearch
{
    /// <summary>
    /// Room-Date Context Search Service
    /// 
    /// ROOM NUMBER & ROOM-DATE CONTEXT REALITY (LOCKED PRODUCT DECISION):
    /// - Enable search by Room + Date Range showing:
    ///   - Guests
    ///   - Transfers
    ///   - Tours
    ///   - Invoices
    ///   - Payments
    /// </summary>
    public interface IRoomDateSearchService
    {
        /// <summary>
        /// Search all operations by room number and date range
        /// </summary>
        Task<RoomDateSearchResultDto> SearchByRoomAndDateAsync(
            string roomNumber, 
            DateTime startDate, 
            DateTime endDate);

        /// <summary>
        /// Get room assignment history for a guest
        /// </summary>
        Task<List<RoomAssignmentDto>> GetGuestRoomHistoryAsync(int guestId);

        /// <summary>
        /// Record a new room assignment
        /// </summary>
        Task<RoomAssignmentDto> AddRoomAssignmentAsync(int guestId, string roomNumber, DateTime assignedDate, string source = "Manual");

        /// <summary>
        /// End a room assignment (guest left room)
        /// </summary>
        Task EndRoomAssignmentAsync(int assignmentId, DateTime endDate);
    }

    public class RoomDateSearchResultDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public List<RoomGuestDto> Guests { get; set; } = new();
        public List<RoomServiceDto> Transfers { get; set; } = new();
        public List<RoomServiceDto> CityTours { get; set; } = new();
        public List<RoomServiceDto> YachtTours { get; set; } = new();
        public List<RoomInvoiceDto> Invoices { get; set; } = new();
        public List<RoomPaymentDto> Payments { get; set; } = new();
    }

    public class RoomGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime RoomAssignedDate { get; set; }
        public DateTime? RoomEndDate { get; set; }
    }

    public class RoomServiceDto
    {
        public int Id { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ServiceAmount { get; set; }
        public string Currency { get; set; } = "TRY";
    }

    public class RoomInvoiceDto
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "TRY";
    }

    public class RoomPaymentDto
    {
        public int Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class RoomAssignmentDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsCurrent { get; set; }
    }
}

