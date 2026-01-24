// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Dashboard.Dtos
{
    /// <summary>
    /// Günlük operasyon özeti
    /// </summary>
    public class DailyOperationsDto
    {
        public DateTime Date { get; set; }
        public List<ServiceOperationDto> TodayServices { get; set; } = new List<ServiceOperationDto>();
        public List<ServiceOperationDto> UpcomingServices { get; set; } = new List<ServiceOperationDto>();
        public List<RiskFlagDto> RiskFlags { get; set; } = new List<RiskFlagDto>();
        public DailyOperationsQuickStatsDto QuickStats { get; set; } = new DailyOperationsQuickStatsDto();
    }

    /// <summary>
    /// Servis operasyon bilgisi
    /// </summary>
    public class ServiceOperationDto
    {
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public DateTime ServiceTime { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string? CityName { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? AssignedPersonnelId { get; set; }
        public string? AssignedPersonnelName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public bool IsUrgent { get; set; }
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Risk bayrağı
    /// </summary>
    public class RiskFlagDto
    {
        public RiskFlagType Type { get; set; }
        public RiskFlagSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Risk bayrağı tipi
    /// </summary>
    public enum RiskFlagType
    {
        OverduePayment = 1,      // Geciken ödeme
        UnpaidService = 2,       // Ödemesi alınmamış servis
        UnassignedDriver = 3,    // Atanmayan şoför
        UrgentUnconfirmed = 4,   // Acil onay bekleyen
        ConflictingReservation = 5, // Çakışan rezervasyon
        MissingGuestInfo = 6    // Eksik misafir bilgisi
    }

    /// <summary>
    /// Risk bayrağı şiddeti
    /// </summary>
    public enum RiskFlagSeverity
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Günlük operasyon hızlı istatistikler
    /// </summary>
    public class DailyOperationsQuickStatsDto
    {
        public int TodayServiceCount { get; set; }
        public int UpcomingServiceCount { get; set; }
        public int UrgentServiceCount { get; set; }
        public int UnassignedDriverCount { get; set; }
        public int UnpaidServiceCount { get; set; }
        public int OverduePaymentCount { get; set; }
    }
}
