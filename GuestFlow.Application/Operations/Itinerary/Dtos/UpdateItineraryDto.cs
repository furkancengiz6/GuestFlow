using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.Itinerary.Dtos
{
    public class UpdateItineraryDto
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ItineraryStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}

