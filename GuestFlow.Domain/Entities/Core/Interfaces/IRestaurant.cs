namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IRestaurant
    {
        string RestaurantName { get; set; }
        string Address { get; set; }
        string? Phone { get; set; }
        string? Email { get; set; }
    }
}

