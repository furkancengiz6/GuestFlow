// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace GuestFlow.Application.Models.Responses.PMS
{
    /// <summary>
    /// PMS'den gelen misafir profili
    /// </summary>
    public class PMSGuestProfile
    {
        public string PMSGuestId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Nationality { get; set; }
        public string? GuestCode { get; set; }
        public bool IsVIP { get; set; }
        public string? RoomNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? SpecialRequests { get; set; }
        public string? Preferences { get; set; } // JSON formatında tercihler
        public DateTime? LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// PMS'den gelen rezervasyon bilgisi
    /// </summary>
    public class PMSReservation
    {
        public string PMSReservationId { get; set; } = string.Empty;
        public string PMSGuestId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public int GuestCount { get; set; }
        public string Status { get; set; } = string.Empty; // Confirmed, CheckedIn, CheckedOut, Cancelled
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    /// <summary>
    /// PMS'den gelen oda durumu
    /// </summary>
    public class PMSRoomStatus
    {
        public string RoomNumber { get; set; } = string.Empty;
        public string? RoomType { get; set; }
        public string Status { get; set; } = string.Empty; // Available, Occupied, OutOfOrder, Maintenance
        public string? GuestName { get; set; }
        public string? PMSGuestId { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }

    /// <summary>
    /// PMS'den gelen folio (fatura) bilgisi
    /// </summary>
    public class PMSFolio
    {
        public string FolioId { get; set; } = string.Empty;
        public string ReservationId { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Open, Closed, Settled
        public DateTime? FolioDate { get; set; }
        public List<PMSFolioItem> Items { get; set; } = new();
    }

    /// <summary>
    /// Folio item (fatura kalemi)
    /// </summary>
    public class PMSFolioItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    /// <summary>
    /// PMS senkronizasyon geçmişi response
    /// </summary>
    public class PMSSyncHistoryResponse
    {
        public int Id { get; set; }
        public int PMSIntegrationId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string SyncType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SyncStartTime { get; set; }
        public DateTime? SyncEndTime { get; set; }
        public int? RecordsProcessed { get; set; }
        public int? RecordsSucceeded { get; set; }
        public int? RecordsFailed { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan? Duration => SyncEndTime.HasValue ? SyncEndTime.Value - SyncStartTime : null;
    }
}
