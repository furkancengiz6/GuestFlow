using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class AddItineraryItemDto
    {
        public ItineraryItemType ItemType { get; set; }
        public int ServiceId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int Order { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}

