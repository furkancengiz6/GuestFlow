using GuestFlow.Domain.Entities.Enum;
using System;

namespace GuestFlow.Application.Operations.TransferRecommendation.Dtos
{
    public class TransferRecommendationDto
    {
        public TransferType RecommendedTransferType { get; set; }
        public string RecommendationReason { get; set; } = string.Empty;
        public string? PickupAddress { get; set; }
        public string? DropoffAddress { get; set; }
        public DateTime? RecommendedDate { get; set; }
        public TimeSpan? RecommendedTime { get; set; }
        public int? HotelId { get; set; }
        public int? RestaurantId { get; set; }
        public int? AirportId { get; set; }
        public decimal? EstimatedPrice { get; set; }
        public int Priority { get; set; } // 1-5 arası öncelik (1 = yüksek öncelik)
    }
}

