using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Reports
{
    public class RevenueSummaryDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CityTourRevenue { get; set; }
        public decimal YachtTourRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public int TotalBookings { get; set; }
        public int CityTourCount { get; set; }
        public int YachtTourCount { get; set; }
        public int TransferCount { get; set; }
        public decimal AverageBookingValue { get; set; }
    }

    public class GuestStatisticsDto
    {
        public int TotalGuests { get; set; }
        public int ActiveGuests { get; set; }
        public int SpecialGuests { get; set; }
        public int RegularGuests { get; set; }
        public List<TopGuestDto> TopGuests { get; set; } = new List<TopGuestDto>();
        public Dictionary<string, int> GuestsByNationality { get; set; } = new Dictionary<string, int>();
    }

    public class TopGuestDto
    {
        public int GuestId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class TourStatisticsDto
    {
        public string? TourType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTours { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public int CompletedTours { get; set; }
        public int UpcomingTours { get; set; }
        public Dictionary<string, int> ToursByLanguage { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ToursByCity { get; set; } = new Dictionary<string, int>();
    }

    public class TransferStatisticsDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalTransfers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public int FromAirportCount { get; set; }
        public int ToAirportCount { get; set; }
        public int CompletedTransfers { get; set; }
        public int PendingTransfers { get; set; }
        public Dictionary<string, int> TransfersByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class PopularDestinationDto
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DashboardSummaryDto
    {
        public int TotalGuests { get; set; }
        public int TotalPersonnel { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal ThisWeekRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public int ActiveTransfers { get; set; }
        public int UpcomingTours { get; set; }
        public int PendingInvoices { get; set; }
        public int TodayBookings { get; set; }
        public List<RecentBookingDto> RecentBookings { get; set; } = new List<RecentBookingDto>();
    }

    public class RecentBookingDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public string GuestName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CityTourRevenue { get; set; }
        public decimal YachtTourRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class WeeklyRevenueDto
    {
        public int Year { get; set; }
        public int Week { get; set; }
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CityTourRevenue { get; set; }
        public decimal YachtTourRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class YearlyRevenueDto
    {
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CityTourRevenue { get; set; }
        public decimal YachtTourRevenue { get; set; }
        public decimal TransferRevenue { get; set; }
        public int BookingCount { get; set; }
        public int GuestCount { get; set; }
    }

    public class PopularTourDto
    {
        public int TourId { get; set; }
        public string TourType { get; set; } = string.Empty; // CityTour, YachtTour
        public string CityName { get; set; } = string.Empty;
        public string? Language { get; set; } // CityTour için
        public string? YachtName { get; set; } // YachtTour için
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AveragePrice { get; set; }
    }

    public class PersonnelPerformanceDto
    {
        public int PersonnelId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int TransferCount { get; set; }
        public int CityTourCount { get; set; }
        public int YachtTourCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
    }
}

