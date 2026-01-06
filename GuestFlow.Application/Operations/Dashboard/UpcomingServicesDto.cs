using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Dashboard
{
    public class UpcomingServiceItemDto
    {
        public string ServiceType { get; set; } = string.Empty; // Transfer, CityTour, YachtTour
        public int ServiceId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? CityName { get; set; }
        public string? Status { get; set; }
        public bool IsUrgent { get; set; } // Transfer için 3 saat kala, turlar için 1 gün kala
    }

    public class UpcomingServicesDto
    {
        public IList<UpcomingServiceItemDto> Items { get; set; } = new List<UpcomingServiceItemDto>();
    }
}
