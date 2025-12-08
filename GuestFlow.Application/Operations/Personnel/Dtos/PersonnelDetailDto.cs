using System;
using System.Collections.Generic;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Application.Operations.Personnel.Dtos
{
    /// <summary>
    /// Personel detay DTO (ilgili veriler ile)
    /// </summary>
    public class PersonnelDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public DateTime? CreatedDate { get; set; }

        // İstatistikler
        public PersonnelStatisticsDto Statistics { get; set; } = new PersonnelStatisticsDto();

        // Son Aktiviteler
        public List<PersonnelActivityDto> RecentActivities { get; set; } = new List<PersonnelActivityDto>();
    }

    /// <summary>
    /// Personel istatistikleri DTO
    /// </summary>
    public class PersonnelStatisticsDto
    {
        public int TotalTransfers { get; set; }
        public int TotalCityTours { get; set; }
        public int TotalYachtTours { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageBookingValue { get; set; }
        public int CompletedBookings { get; set; }
        public int PendingBookings { get; set; }
    }

    /// <summary>
    /// Personel aktivite DTO
    /// </summary>
    public class PersonnelActivityDto
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public string Description { get; set; } = string.Empty;
        public DateTime ActivityDate { get; set; }
        public string? GuestName { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
    }
}

