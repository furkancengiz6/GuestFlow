namespace GuestFlow.Application.Operations.Restaurant.Dtos
{
    public class GetRestaurantDto
    {
        public int Id { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public string? CuisineType { get; set; }
        public int? Capacity { get; set; }
        public string? OperatingHours { get; set; }
        public bool ReservationRequired { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

