using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Dashboard
{
    /// <summary>
    /// Dashboard genel bakış DTO
    /// </summary>
    public class DashboardOverviewDto
    {
        // Toplam Sayılar
        public int TotalGuests { get; set; }
        public int TotalPersonnel { get; set; }
        public int TotalCities { get; set; }
        public int TotalVehicles { get; set; }

        // Gelir Bilgileri
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public decimal LastMonthRevenue { get; set; }
        public decimal YearToDateRevenue { get; set; }

        // Aktif Rezervasyonlar
        public int ActiveTransfers { get; set; }
        public int UpcomingTours { get; set; }
        public int PendingInvoices { get; set; }
        public int TodayBookings { get; set; }

        // İstatistikler
        public decimal AverageBookingValue { get; set; }
        public int TotalBookingsThisMonth { get; set; }
        public int TotalBookingsLastMonth { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }

        // Son Rezervasyonlar
        public List<RecentBookingDto> RecentBookings { get; set; } = new List<RecentBookingDto>();

        // En Popüler Hizmetler
        public List<PopularServiceDto> PopularServices { get; set; } = new List<PopularServiceDto>();
    }

    /// <summary>
    /// Hızlı istatistikler DTO
    /// </summary>
    public class QuickStatsDto
    {
        public int TotalGuests { get; set; }
        public int ActiveGuests { get; set; }
        public int TotalPersonnel { get; set; }
        public int TotalTransfers { get; set; }
        public int TotalCityTours { get; set; }
        public int TotalYachtTours { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>
    /// Son aktiviteler DTO
    /// </summary>
    public class RecentActivityDto
    {
        public List<RecentBookingDto> RecentBookings { get; set; } = new List<RecentBookingDto>();
        public List<RecentGuestDto> RecentGuests { get; set; } = new List<RecentGuestDto>();
        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new List<RecentInvoiceDto>();
    }

    /// <summary>
    /// Son rezervasyon DTO
    /// </summary>
    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Son misafir DTO
    /// </summary>
    public class RecentGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Son fatura DTO
    /// </summary>
    public class RecentInvoiceDto
    {
        public int Id { get; set; }
        public int InvoiceNumber { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public bool HasPdf { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// Popüler hizmet DTO
    /// </summary>
    public class PopularServiceDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
    }

    /// <summary>
    /// Gelir grafik verisi DTO
    /// </summary>
    public class RevenueChartDataDto
    {
        public string Period { get; set; } = string.Empty; // Gün, Hafta, Ay
        public List<RevenueChartItemDto> Data { get; set; } = new List<RevenueChartItemDto>();
    }

    /// <summary>
    /// Gelir grafik öğesi DTO
    /// </summary>
    public class RevenueChartItemDto
    {
        public string Label { get; set; } = string.Empty; // Tarih veya dönem etiketi
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Yaklaşan rezervasyon DTO
    /// </summary>
    public class UpcomingBookingDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public string GuestName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime? StartTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? PersonnelId { get; set; }
        public string PersonnelName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Yaklaşan rezervasyonlar DTO
    /// </summary>
    public class UpcomingBookingsDto
    {
        public List<UpcomingBookingDto> Today { get; set; } = new List<UpcomingBookingDto>();
        public List<UpcomingBookingDto> ThisWeek { get; set; } = new List<UpcomingBookingDto>();
        public List<UpcomingBookingDto> ThisMonth { get; set; } = new List<UpcomingBookingDto>();
        public int TotalUpcoming { get; set; }
    }

    /// <summary>
    /// Misafir istatistik kartı DTO
    /// </summary>
    public class GuestStatisticsCardDto
    {
        public int TotalGuests { get; set; }
        public int ActiveGuests { get; set; }
        public int SpecialGuests { get; set; }
        public int NewGuestsThisMonth { get; set; }
        public int NewGuestsLastMonth { get; set; }
        public decimal GuestGrowthPercentage { get; set; }
        public List<TopGuestDto> TopGuests { get; set; } = new List<TopGuestDto>();
    }

    /// <summary>
    /// En çok rezervasyon yapan misafir DTO
    /// </summary>
    public class TopGuestDto
    {
        public int GuestId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
