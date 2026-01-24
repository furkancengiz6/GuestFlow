// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using GuestFlow.Application.Operations.Dashboard.Dtos;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Check-in/Check-out DTO
    /// </summary>
    public class ConciergeCheckInOutDto
    {
        public List<CheckInOutItemDto> Items { get; set; } = new List<CheckInOutItemDto>();
        public int TotalCount { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Check-in/Check-out item DTO
    /// </summary>
    public class CheckInOutItemDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? NumberOfGuests { get; set; }
        public string? SpecialRequests { get; set; }
        public string? Notes { get; set; }
        public string Source { get; set; } = "GuestFlow"; // "GuestFlow" veya "PMS"
        public string? PMSReservationId { get; set; }
        public string? PMSProviderName { get; set; }
        public bool IsVIP { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Aktif misafir DTO
    /// </summary>
    public class ActiveGuestDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int? NumberOfNights { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVIP { get; set; }
        public string Source { get; set; } = "GuestFlow"; // "GuestFlow" veya "PMS"
        public string? PMSReservationId { get; set; }
        public string? PMSProviderName { get; set; }
        public List<UpcomingServiceItemDto> UpcomingServices { get; set; } = new List<UpcomingServiceItemDto>();
    }

    /// <summary>
    /// Unified guest profile DTO (PMS + GuestFlow birleşik)
    /// </summary>
    public class UnifiedGuestProfileDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        
        // GuestFlow verileri
        public GuestFlowDataDto? GuestFlowData { get; set; }
        
        // PMS verileri
        public List<PMSDataDto> PMSData { get; set; } = new List<PMSDataDto>();
        
        // Birleşik görünüm (en güncel veriler)
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVIP { get; set; }
    }

    /// <summary>
    /// GuestFlow veri DTO
    /// </summary>
    public class GuestFlowDataDto
    {
        public int GuestId { get; set; }
        public string? RoomNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVIP { get; set; }
        public List<ServiceHistoryDto> ServiceHistory { get; set; } = new List<ServiceHistoryDto>();
        public List<RoomAssignmentHistoryDto> RoomAssignmentHistory { get; set; } = new List<RoomAssignmentHistoryDto>();
        public GuestPreferencesDto? Preferences { get; set; }
        public List<InvoiceSummaryDto> InvoiceHistory { get; set; } = new List<InvoiceSummaryDto>();
    }

    /// <summary>
    /// Room assignment history DTO
    /// </summary>
    public class RoomAssignmentHistoryDto
    {
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Source { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Guest preferences DTO
    /// </summary>
    public class GuestPreferencesDto
    {
        public string? PreferredRoomType { get; set; }
        public string? RoomSpecialRequests { get; set; }
        public string? BedPreference { get; set; }
        public string? SmokingPreference { get; set; }
        public string? DietaryPreferences { get; set; }
        public string? FoodAllergies { get; set; }
        public string? ActivityPreferences { get; set; }
        public string? Interests { get; set; }
        public bool PrefersEmail { get; set; }
        public bool PrefersSMS { get; set; }
        public bool PrefersWhatsApp { get; set; }
        public bool PrefersPhone { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? Notes { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    /// <summary>
    /// Invoice summary DTO
    /// </summary>
    public class InvoiceSummaryDto
    {
        public int InvoiceId { get; set; }
        public int InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// PMS veri DTO
    /// </summary>
    public class PMSDataDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderCode { get; set; } = string.Empty;
        public string? PMSReservationId { get; set; }
        public string? PMSGuestId { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsVIP { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public List<PMSReservationHistoryDto> ReservationHistory { get; set; } = new List<PMSReservationHistoryDto>();
    }

    /// <summary>
    /// PMS reservation history DTO
    /// </summary>
    public class PMSReservationHistoryDto
    {
        public string PMSReservationId { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }

    /// <summary>
    /// Servis geçmişi DTO
    /// </summary>
    public class ServiceHistoryDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour, Restaurant
        public DateTime ServiceDate { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
    }

    /// <summary>
    /// Guest History Dashboard DTO (önceki konaklamalar, hizmet geçmişi, harcama analizi)
    /// </summary>
    public class GuestHistoryDashboardDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        
        // Önceki konaklamalar (PMS'den)
        public List<PreviousStayDto> PreviousStays { get; set; } = new List<PreviousStayDto>();
        
        // Hizmet geçmişi (GuestFlow'dan)
        public List<ServiceHistoryDto> ServiceHistory { get; set; } = new List<ServiceHistoryDto>();
        
        // Harcama analizi
        public SpendingAnalysisDto SpendingAnalysis { get; set; } = new SpendingAnalysisDto();
        
        // Tercih analizi
        public PreferenceAnalysisDto PreferenceAnalysis { get; set; } = new PreferenceAnalysisDto();
    }

    /// <summary>
    /// Önceki konaklama DTO (PMS'den)
    /// </summary>
    public class PreviousStayDto
    {
        public string? PMSReservationId { get; set; }
        public string? PMSProviderName { get; set; }
        public string? RoomNumber { get; set; }
        public string? RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfNights { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? Currency { get; set; }
        public DateTime? LastSyncedAt { get; set; }
    }

    /// <summary>
    /// Harcama analizi DTO
    /// </summary>
    public class SpendingAnalysisDto
    {
        public decimal TotalSpending { get; set; }
        public string Currency { get; set; } = "TRY";
        public decimal? PMSSpending { get; set; } // PMS'den gelen konaklama harcamaları
        public decimal GuestFlowSpending { get; set; } // GuestFlow'dan gelen hizmet harcamaları
        public int TotalStays { get; set; }
        public int TotalServices { get; set; }
        public decimal AverageSpendingPerStay { get; set; }
        public decimal AverageSpendingPerService { get; set; }
        public List<SpendingByCategoryDto> SpendingByCategory { get; set; } = new List<SpendingByCategoryDto>();
    }

    /// <summary>
    /// Kategori bazlı harcama DTO
    /// </summary>
    public class SpendingByCategoryDto
    {
        public string Category { get; set; } = string.Empty; // Accommodation, Transfer, Tour, Restaurant
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Tercih analizi DTO
    /// </summary>
    public class PreferenceAnalysisDto
    {
        public List<RoomPreferenceDto> RoomPreferences { get; set; } = new List<RoomPreferenceDto>();
        public List<ServicePreferenceDto> ServicePreferences { get; set; } = new List<ServicePreferenceDto>();
        public string? PreferredCheckInTime { get; set; }
        public string? PreferredCheckOutTime { get; set; }
    }

    /// <summary>
    /// Oda tercihi DTO
    /// </summary>
    public class RoomPreferenceDto
    {
        public string RoomType { get; set; } = string.Empty;
        public int StayCount { get; set; }
        public string? SpecialRequests { get; set; } // high floor, sea view, vb.
    }

    /// <summary>
    /// Servis tercihi DTO
    /// </summary>
    public class ServicePreferenceDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour, Restaurant
        public int UsageCount { get; set; }
        public decimal? TotalSpending { get; set; }
    }

    /// <summary>
    /// Concierge Dashboard özet DTO (tüm bilgileri birleştirir)
    /// </summary>
    public class ConciergeDashboardSummaryDto
    {
        public ConciergeCheckInOutDto TodayCheckIns { get; set; } = new ConciergeCheckInOutDto();
        public ConciergeCheckInOutDto TodayCheckOuts { get; set; } = new ConciergeCheckInOutDto();
        public List<ActiveGuestDto> ActiveGuests { get; set; } = new List<ActiveGuestDto>();
        public UpcomingServicesDto UpcomingServices { get; set; } = new UpcomingServicesDto();
        public List<GuestStatusIndicatorDto> GuestStatusIndicators { get; set; } = new List<GuestStatusIndicatorDto>();
        public DailyOperationsDto? DailyOperations { get; set; }
        public DailyOperationsQuickStatsDto QuickStats { get; set; } = new DailyOperationsQuickStatsDto();
    }

    /// <summary>
    /// Misafir durumu göstergesi DTO
    /// </summary>
    public class GuestStatusIndicatorDto
    {
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public GuestStatusType StatusType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? EventDate { get; set; } // Doğum günü, yıldönümü tarihi
    }

    /// <summary>
    /// Misafir durumu tipi
    /// </summary>
    public enum GuestStatusType
    {
        VIP = 1,
        SpecialRequests = 2,
        ProblemGuest = 3,
        RepeatGuest = 4,
        Birthday = 5,
        Anniversary = 6
    }

    /// <summary>
    /// Yaklaşan servisler DTO
    /// </summary>
    public class UpcomingServicesDto
    {
        public List<UpcomingServiceItemDto> Items { get; set; } = new List<UpcomingServiceItemDto>();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Yaklaşan servis item DTO
    /// </summary>
    public class UpcomingServiceItemDto
    {
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour, Restaurant
        public DateTime ServiceDate { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? CityName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsUrgent { get; set; }
    }

}
