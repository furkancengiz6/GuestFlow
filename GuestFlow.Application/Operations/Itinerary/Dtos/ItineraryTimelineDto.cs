using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class ItineraryTimelineDto
    {
        public int ItineraryId { get; set; }
        public string ItineraryNumber { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "TRY";
        public List<TimelineItemDto> Items { get; set; } = new List<TimelineItemDto>();
    }

    public class TimelineItemDto
    {
        public int Id { get; set; }
        public string ItemType { get; set; } = string.Empty;
        public string ItemTypeTurkish { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int Order { get; set; }
        public string? Status { get; set; }
        public string? ServiceName { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? PickupLocation { get; set; }
        public string? DropoffLocation { get; set; }
        public string? Icon { get; set; }
        public decimal? Price { get; set; }
        public string? Currency { get; set; }
        public string? Duration { get; set; }
        public string? Notes { get; set; }
        public Dictionary<string, object>? AdditionalInfo { get; set; }
    }
}

