
namespace GuestFlow.Application.Models.Responses.PMS
{
    /// <summary>
    /// PMS'den gelen oda tipi bilgisi
    /// </summary>
    public class PMSRoomType
    {
        public string RoomTypeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public int TotalInventory { get; set; }
    }
}
