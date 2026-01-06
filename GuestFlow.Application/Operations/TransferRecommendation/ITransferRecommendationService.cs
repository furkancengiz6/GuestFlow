using GuestFlow.Application.Operations.TransferRecommendation.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.TransferRecommendation
{
    public interface ITransferRecommendationService
    {
        /// <summary>
        /// Misafir için otomatik transfer önerileri oluşturur
        /// </summary>
        Task<List<TransferRecommendationDto>> GetRecommendationsForGuest(int guestId);

        /// <summary>
        /// Check-in tarihine göre havalimanı→otel transfer önerisi
        /// </summary>
        Task<TransferRecommendationDto?> RecommendAirportToHotelTransfer(int guestId);

        /// <summary>
        /// Check-out tarihine göre otel→havalimanı transfer önerisi
        /// </summary>
        Task<TransferRecommendationDto?> RecommendHotelToAirportTransfer(int guestId);

        /// <summary>
        /// Şehir turu rezervasyonuna göre otel→tur başlangıç noktası transfer önerisi
        /// </summary>
        Task<TransferRecommendationDto?> RecommendTransferForCityTour(int guestId, int cityTourId);

        /// <summary>
        /// Yat turu rezervasyonuna göre otel→iskele transfer önerisi
        /// </summary>
        Task<TransferRecommendationDto?> RecommendTransferForYachtTour(int guestId, int yachtTourId);

        /// <summary>
        /// Restoran rezervasyonuna göre otel→restoran transfer önerisi
        /// </summary>
        Task<TransferRecommendationDto?> RecommendTransferForRestaurantReservation(int guestId, int restaurantReservationId);
    }
}

