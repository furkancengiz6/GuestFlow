using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class AddItineraryDto
    {
        public int GuestId { get; set; }
        public int PersonnelId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItineraryStatus Status { get; set; } = ItineraryStatus.Draft;
        public string? Notes { get; set; }
        public List<AddItineraryItemDto> Items { get; set; } = new List<AddItineraryItemDto>();
    }
}

