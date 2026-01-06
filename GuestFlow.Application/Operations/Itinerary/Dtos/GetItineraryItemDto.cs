using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class GetItineraryItemDto
    {
        public int Id { get; set; }
        public int ItineraryId { get; set; }
        public ItineraryItemType ItemType { get; set; }
        public int ServiceId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int Order { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public string? ServiceName { get; set; } // Transfer adresi, Tur adı, Restoran adı vb.
    }
}

