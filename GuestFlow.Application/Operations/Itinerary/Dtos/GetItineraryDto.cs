using GuestFlow.Domain.Entities.Enum;
using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class GetItineraryDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public int PersonnelId { get; set; }
        public string PersonnelName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItineraryStatus Status { get; set; }
        public decimal TotalCost { get; set; }
        public string Currency { get; set; } = "TRY";
        public string? Notes { get; set; }
        public string ItineraryNumber { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public List<GetItineraryItemDto> Items { get; set; } = new List<GetItineraryItemDto>();
    }
}

