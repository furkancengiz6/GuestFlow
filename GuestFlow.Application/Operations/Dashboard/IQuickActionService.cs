// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Quick Action Service - Concierge için hızlı aksiyonlar (One-Click Actions)
    /// </summary>
    public interface IQuickActionService
    {
        /// <summary>
        /// Transfer rezervasyonu oluştur (misafir bilgisi otomatik doldurulur)
        /// </summary>
        Task<ApiResponse<QuickActionTransferResult>> CreateTransferReservationAsync(int guestId, QuickActionTransferRequest request);

        /// <summary>
        /// Tur rezervasyonu oluştur (misafir bilgisi otomatik doldurulur)
        /// </summary>
        Task<ApiResponse<QuickActionTourResult>> CreateTourReservationAsync(int guestId, QuickActionTourRequest request);

        /// <summary>
        /// Restoran rezervasyonu oluştur (misafir bilgisi otomatik doldurulur)
        /// </summary>
        Task<ApiResponse<QuickActionRestaurantResult>> CreateRestaurantReservationAsync(int guestId, QuickActionRestaurantRequest request);

        /// <summary>
        /// Oda servisi talebi oluştur
        /// </summary>
        Task<ApiResponse<QuickActionRoomServiceResult>> CreateRoomServiceRequestAsync(int guestId, QuickActionRoomServiceRequest request);

        /// <summary>
        /// Mesaj gönder (e-posta/SMS/WhatsApp)
        /// </summary>
        Task<ApiResponse<QuickActionMessageResult>> SendMessageAsync(int guestId, QuickActionMessageRequest request);

        /// <summary>
        /// PMS folio (fatura) görüntüle
        /// </summary>
        Task<ApiResponse<QuickActionFolioResult>> GetFolioAsync(int guestId);
    }

    /// <summary>
    /// Quick Action Transfer Request
    /// </summary>
    public class QuickActionTransferRequest
    {
        public int? AirportId { get; set; }
        public int? HotelId { get; set; }
        public DateTime TransferDate { get; set; }
        public string? TransferTime { get; set; }
        public string TransferType { get; set; } = "Pickup"; // Pickup, Dropoff
        public int? VehicleId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Quick Action Transfer Result
    /// </summary>
    public class QuickActionTransferResult
    {
        public int TransferId { get; set; }
        public string TransferNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quick Action Tour Request
    /// </summary>
    public class QuickActionTourRequest
    {
        public string TourType { get; set; } = "CityTour"; // CityTour, YachtTour
        public int? TourId { get; set; }
        public DateTime TourDate { get; set; }
        public string? TourTime { get; set; }
        public int GuestCount { get; set; } = 1;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Quick Action Tour Result
    /// </summary>
    public class QuickActionTourResult
    {
        public int ReservationId { get; set; }
        public string ReservationNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quick Action Restaurant Request
    /// </summary>
    public class QuickActionRestaurantRequest
    {
        public int RestaurantId { get; set; }
        public DateTime ReservationDate { get; set; }
        public string ReservationTime { get; set; } = string.Empty;
        public int GuestCount { get; set; } = 1;
        public string? SpecialRequests { get; set; }
    }

    /// <summary>
    /// Quick Action Restaurant Result
    /// </summary>
    public class QuickActionRestaurantResult
    {
        public int ReservationId { get; set; }
        public string ReservationNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quick Action Room Service Request
    /// </summary>
    public class QuickActionRoomServiceRequest
    {
        public string ServiceType { get; set; } = string.Empty; // RoomService, Laundry, Housekeeping, vb.
        public string Description { get; set; } = string.Empty;
        public DateTime? RequestedTime { get; set; }
        public string? Priority { get; set; } // Low, Medium, High, Urgent
    }

    /// <summary>
    /// Quick Action Room Service Result
    /// </summary>
    public class QuickActionRoomServiceResult
    {
        public int RequestId { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quick Action Message Request
    /// </summary>
    public class QuickActionMessageRequest
    {
        public string Channel { get; set; } = "Email"; // Email, SMS, WhatsApp
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TemplateName { get; set; }
    }

    /// <summary>
    /// Quick Action Message Result
    /// </summary>
    public class QuickActionMessageResult
    {
        public bool Sent { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quick Action Folio Result
    /// </summary>
    public class QuickActionFolioResult
    {
        public string FolioId { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "TRY";
        public string? FolioUrl { get; set; }
        public List<FolioItemDto> Items { get; set; } = new List<FolioItemDto>();
    }

    /// <summary>
    /// Folio Item DTO
    /// </summary>
    public class FolioItemDto
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Category { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
