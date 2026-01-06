namespace GuestFlow.Application.Operations.Hotel.Dtos
{
    public class UpdateHotelDto
    {
        public int Id { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int CityId { get; set; }
        public int? StarRating { get; set; }
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public string? RoomTypes { get; set; }
        public string? Amenities { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

