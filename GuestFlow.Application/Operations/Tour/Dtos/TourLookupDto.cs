using System;

namespace GuestFlow.Application.Operations.Tour
{
    public class TourLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CityId { get; set; }
        public bool IsActive { get; set; }
    }
}
