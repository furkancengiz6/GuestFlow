// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Map.Dtos
{
    /// <summary>
    /// Harita görünümü için servis lokasyonu DTO
    /// </summary>
    public class MapServiceLocationDto
    {
        public int ServiceId { get; set; }
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public string ServiceName { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public string Status { get; set; } = string.Empty; // Confirmed, InProgress, Completed, Cancelled
        
        // Lokasyon bilgileri
        public MapLocationDto? PickupLocation { get; set; }
        public MapLocationDto? DropoffLocation { get; set; }
        public List<MapLocationDto>? RoutePoints { get; set; } // Tur güzergahı için
        
        // Misafir bilgileri
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        
        // Personel bilgileri
        public int? PersonnelId { get; set; }
        public string? PersonnelName { get; set; }
        
        // Durum bilgileri
        public bool IsUrgent { get; set; }
        public bool IsDelayed { get; set; }
        public string? ColorCode { get; set; } // Marker rengi için (green, yellow, red, gray)
        
        // Ek bilgiler
        public decimal? Amount { get; set; }
        public string? Currency { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Harita lokasyon DTO (lat/lng)
    /// </summary>
    public class MapLocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? CityName { get; set; }
        public string? Label { get; set; } // "Pickup", "Dropoff", "Hotel", vb.
    }

    /// <summary>
    /// Harita görünümü DTO (tüm servisler)
    /// </summary>
    public class MapViewDto
    {
        public DateTime Date { get; set; }
        public List<MapServiceLocationDto> Services { get; set; } = new List<MapServiceLocationDto>();
        public MapBoundsDto? Bounds { get; set; } // Harita görünüm sınırları
        public MapStatisticsDto Statistics { get; set; } = new MapStatisticsDto();
    }

    /// <summary>
    /// Harita sınırları DTO
    /// </summary>
    public class MapBoundsDto
    {
        public double North { get; set; }
        public double South { get; set; }
        public double East { get; set; }
        public double West { get; set; }
    }

    /// <summary>
    /// Harita istatistikleri DTO
    /// </summary>
    public class MapStatisticsDto
    {
        public int TotalServices { get; set; }
        public int ConfirmedServices { get; set; }
        public int InProgressServices { get; set; }
        public int CompletedServices { get; set; }
        public int UrgentServices { get; set; }
        public int DelayedServices { get; set; }
    }

    /// <summary>
    /// Harita filtreleme parametreleri
    /// </summary>
    public class MapFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? ServiceTypes { get; set; } // Transfer, CityTour, YachtTour
        public List<string>? Statuses { get; set; } // Confirmed, InProgress, Completed
        public int? CityId { get; set; }
        public int? PersonnelId { get; set; }
        public bool? ShowUrgentOnly { get; set; }
        public bool? ShowDelayedOnly { get; set; }
    }
}
