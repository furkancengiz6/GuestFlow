using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.CityTour.Dtos
{
    /// <summary>
    /// Şehir turu detay DTO (ilgili veriler ile)
    /// </summary>
    public class CityTourDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public string Language { get; set; } = string.Empty;
        public int DurationHours { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public int? TourId { get; set; } // Nullable - mevcut kayıtlarda null olabilir
        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
        // Şoför ve araç bilgileri
        public int? VehicleId { get; set; }
        public string? DriverName { get; set; }
        
        // Rehber bilgileri
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }

        // Safety & emergency fields
        public string? GroupLeaderName { get; set; }
        public string? GroupLeaderPhone { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }
        
        // Dışarıdan çekilen araç ve şoför bilgileri
        public string? ExternalVehiclePlate { get; set; }
        public string? ExternalDriverName { get; set; }
        public string? ExternalDriverPhone { get; set; }
        
        public DateTime CreatedDate { get; set; }

        // İlişkili Veriler
        public TourGuestDto? Guest { get; set; }
        public TourPersonnelDto? Personnel { get; set; }
        public TourCityDto? City { get; set; }
    }

    /// <summary>
    /// Tur misafir DTO
    /// </summary>
    public class TourGuestDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string GuestCode { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public bool IsSpecialGuest { get; set; }
    }

    /// <summary>
    /// Tur personel DTO
    /// </summary>
    public class TourPersonnelDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string UserType { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tur şehir DTO
    /// </summary>
    public class TourCityDto
    {
        public int Id { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string? Country { get; set; }
    }

    /// <summary>
    /// Tur takvim öğesi DTO
    /// </summary>
    public class TourCalendarItemDto
    {
        public int Id { get; set; }
        public string TourType { get; set; } = string.Empty; // CityTour, YachtTour
        public DateTime TourDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string? PersonnelName { get; set; }
        public string? CityName { get; set; }
        public decimal FinalPrice { get; set; }
        public string? AdditionalInfo { get; set; } // Language + DurationHours for CityTour, YachtName + NumberOfPeople for YachtTour
    }

    /// <summary>
    /// Tur takvim görünümü DTO
    /// </summary>
    public class TourCalendarDto
    {
        public List<TourCalendarItemDto> Today { get; set; } = new List<TourCalendarItemDto>();
        public List<TourCalendarItemDto> ThisWeek { get; set; } = new List<TourCalendarItemDto>();
        public List<TourCalendarItemDto> ThisMonth { get; set; } = new List<TourCalendarItemDto>();
        public int TotalUpcoming { get; set; }
    }

    /// <summary>
    /// Tur istatistikleri DTO
    /// </summary>
    public class TourStatisticsDto
    {
        public int TotalCityTours { get; set; }
        public int TotalYachtTours { get; set; }
        public int TotalTours { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CityTourRevenue { get; set; }
        public decimal YachtTourRevenue { get; set; }
        public decimal AveragePrice { get; set; }
        public int TotalGuests { get; set; }
    }
}

