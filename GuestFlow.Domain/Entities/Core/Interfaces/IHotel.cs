namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface IHotel
    {
        string HotelName { get; set; }
        string Address { get; set; }
        string? Phone { get; set; }
        string? Email { get; set; }
    }
}

