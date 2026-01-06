using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.YachtTour.Dtos
{
    /// <summary>
    /// Yat turu detay DTO (ilgili veriler ile)
    /// </summary>
    public class YachtTourDetailDto
    {
        // Temel Bilgiler
        public int Id { get; set; }
        public DateTime TourDate { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Price { get; set; }
        public decimal FinalPrice { get; set; }
        public string SpecialRequest { get; set; } = string.Empty;
        public string YachtName { get; set; } = string.Empty;

        // Group coordination fields
        public string? GroupLeaderName { get; set; }
        public string? GroupLeaderPhone { get; set; }
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }
        public string? EmergencyContactRelation { get; set; }

        public decimal? DiscountPercentage { get; set; }
        public string? Currency { get; set; }
        public int? PickupHotelId { get; set; } // Otelden alınacaksa otel ID
        
        // İskele bilgileri
        public string? PickupPier { get; set; }
        public string? DropoffPier { get; set; }
        
        // Zaman bilgileri
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        
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
}

