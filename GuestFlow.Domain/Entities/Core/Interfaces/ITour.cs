namespace GuestFlow.Domain.Entities.Core.Interfaces
{
    public interface ITour
    {
        string Name { get; set; }
        string? Description { get; set; }
        int CityId { get; set; }
        bool IsActive { get; set; }
    }
}

