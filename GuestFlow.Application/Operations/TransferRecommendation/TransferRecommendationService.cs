using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.TransferRecommendation.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Application.Operations.TransferRecommendation
{
    public class TransferRecommendationService : ITransferRecommendationService
    {
        private readonly IRepository<GuestEntity> _guestRepository;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<RestaurantReservationEntity> _restaurantReservationRepository;
        private readonly IRepository<HotelEntity> _hotelRepository;
        private readonly IRepository<AirportEntity> _airportRepository;
        private readonly ILogger<TransferRecommendationService> _logger;

        public TransferRecommendationService(
            IRepository<GuestEntity> guestRepository,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<RestaurantReservationEntity> restaurantReservationRepository,
            IRepository<HotelEntity> hotelRepository,
            IRepository<AirportEntity> airportRepository,
            ILogger<TransferRecommendationService> logger)
        {
            _guestRepository = guestRepository;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _restaurantReservationRepository = restaurantReservationRepository;
            _hotelRepository = hotelRepository;
            _airportRepository = airportRepository;
            _logger = logger;
        }

        public async Task<List<TransferRecommendationDto>> GetRecommendationsForGuest(int guestId)
        {
            var recommendations = new List<TransferRecommendationDto>();

            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null)
                return recommendations;

            // 1. Check-in tarihine göre havalimanı→otel transfer önerisi
            if (guest.CheckInDate.HasValue && guest.HotelId.HasValue)
            {
                var airportTransfer = await RecommendAirportToHotelTransfer(guestId);
                if (airportTransfer != null)
                {
                    recommendations.Add(airportTransfer);
                }
            }

            // 2. Check-out tarihine göre otel→havalimanı transfer önerisi
            if (guest.CheckOutDate.HasValue && guest.HotelId.HasValue)
            {
                var hotelToAirport = await RecommendHotelToAirportTransfer(guestId);
                if (hotelToAirport != null)
                {
                    recommendations.Add(hotelToAirport);
                }
            }

            // 3. Şehir turları için transfer önerileri
            var cityTours = await _cityTourRepository.GetAll(x => x.OwnerGuestId == guestId && x.TourDate >= DateTime.Today)
                .OrderBy(x => x.TourDate)
                .ToListAsync();

            foreach (var cityTour in cityTours)
            {
                var cityTourTransfer = await RecommendTransferForCityTour(guestId, cityTour.Id);
                if (cityTourTransfer != null)
                {
                    recommendations.Add(cityTourTransfer);
                }
            }

            // 4. Yat turları için transfer önerileri
            var yachtTours = await _yachtTourRepository.GetAll(x => x.OwnerGuestId == guestId && x.TourDate >= DateTime.Today)
                .OrderBy(x => x.TourDate)
                .ToListAsync();

            foreach (var yachtTour in yachtTours)
            {
                var yachtTourTransfer = await RecommendTransferForYachtTour(guestId, yachtTour.Id);
                if (yachtTourTransfer != null)
                {
                    recommendations.Add(yachtTourTransfer);
                }
            }

            // 5. Restoran rezervasyonları için transfer önerileri
            var restaurantReservations = await _restaurantReservationRepository.GetAll(
                x => x.GuestId == guestId && x.ReservationDate >= DateTime.Today)
                .OrderBy(x => x.ReservationDate)
                .ToListAsync();

            foreach (var reservation in restaurantReservations)
            {
                var restaurantTransfer = await RecommendTransferForRestaurantReservation(guestId, reservation.Id);
                if (restaurantTransfer != null)
                {
                    recommendations.Add(restaurantTransfer);
                }
            }

            return recommendations.OrderBy(r => r.Priority).ThenBy(r => r.RecommendedDate).ToList();
        }

        public async Task<TransferRecommendationDto?> RecommendAirportToHotelTransfer(int guestId)
        {
            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null || !guest.CheckInDate.HasValue || !guest.HotelId.HasValue)
                return null;

            var hotel = await _hotelRepository.GetByIdAsync(guest.HotelId.Value);
            if (hotel == null)
                return null;

            // Zaten bu transfer var mı kontrol et
            var existingTransfer = await _transferRepository.GetAll(
                x => x.GuestId == guestId &&
                     x.TransferType == TransferType.AirportToHotel &&
                     x.TransferDate.Date == guest.CheckInDate.Value.Date)
                .FirstOrDefaultAsync();

            if (existingTransfer != null)
                return null; // Zaten transfer var

            // Şehirdeki havalimanlarını bul
            var airports = await _airportRepository.GetAll(x => x.CityId == hotel.CityId).ToListAsync();
            if (!airports.Any())
                return null;

            var airport = airports.First(); // İlk havalimanını kullan (veya en yakın olanı seçilebilir)

            return new TransferRecommendationDto
            {
                RecommendedTransferType = TransferType.AirportToHotel,
                RecommendationReason = $"Check-in tarihi: {guest.CheckInDate.Value:dd.MM.yyyy} - Havalimanından otele transfer önerilir",
                PickupAddress = $"{airport.Name} ({airport.Code})",
                DropoffAddress = hotel.Address,
                RecommendedDate = guest.CheckInDate.Value.Date,
                RecommendedTime = hotel.CheckInTime ?? new TimeSpan(14, 0, 0), // Varsayılan check-in saati
                HotelId = hotel.Id,
                AirportId = airport.Id,
                Priority = 1 // Yüksek öncelik
            };
        }

        public async Task<TransferRecommendationDto?> RecommendHotelToAirportTransfer(int guestId)
        {
            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null || !guest.CheckOutDate.HasValue || !guest.HotelId.HasValue)
                return null;

            var hotel = await _hotelRepository.GetByIdAsync(guest.HotelId.Value);
            if (hotel == null)
                return null;

            // Zaten bu transfer var mı kontrol et
            var existingTransfer = await _transferRepository.GetAll(
                x => x.GuestId == guestId &&
                     x.TransferType == TransferType.HotelToAirport &&
                     x.TransferDate.Date == guest.CheckOutDate.Value.Date)
                .FirstOrDefaultAsync();

            if (existingTransfer != null)
                return null;

            // Şehirdeki havalimanlarını bul
            var airports = await _airportRepository.GetAll(x => x.CityId == hotel.CityId).ToListAsync();
            if (!airports.Any())
                return null;

            var airport = airports.First();

            return new TransferRecommendationDto
            {
                RecommendedTransferType = TransferType.HotelToAirport,
                RecommendationReason = $"Check-out tarihi: {guest.CheckOutDate.Value:dd.MM.yyyy} - Otelden havalimanına transfer önerilir",
                PickupAddress = hotel.Address,
                DropoffAddress = $"{airport.Name} ({airport.Code})",
                RecommendedDate = guest.CheckOutDate.Value.Date,
                RecommendedTime = hotel.CheckOutTime ?? new TimeSpan(11, 0, 0), // Varsayılan check-out saati
                HotelId = hotel.Id,
                AirportId = airport.Id,
                Priority = 1 // Yüksek öncelik
            };
        }

        public async Task<TransferRecommendationDto?> RecommendTransferForCityTour(int guestId, int cityTourId)
        {
            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null || !guest.HotelId.HasValue)
                return null;

            var cityTour = await _cityTourRepository.GetAll(
                x => x.Id == cityTourId,
                x => x.City,
                x => x.PickupHotel)
                .FirstOrDefaultAsync();

            if (cityTour == null)
                return null;

            var hotel = await _hotelRepository.GetByIdAsync(guest.HotelId.Value);
            if (hotel == null)
                return null;

            // Zaten bu transfer var mı kontrol et
            var existingTransfer = await _transferRepository.GetAll(
                x => x.GuestId == guestId &&
                     x.TransferDate.Date == cityTour.TourDate.Date &&
                     x.TransferType == TransferType.HotelToCity)
                .FirstOrDefaultAsync();

            if (existingTransfer != null)
                return null;

            var pickupLocation = cityTour.PickupLocation ?? cityTour.PickupHotel?.Address ?? "Tur Başlangıç Noktası";

            return new TransferRecommendationDto
            {
                RecommendedTransferType = TransferType.HotelToCity,
                RecommendationReason = $"Şehir turu için otelden tur başlangıç noktasına transfer önerilir",
                PickupAddress = hotel.Address,
                DropoffAddress = pickupLocation,
                RecommendedDate = cityTour.TourDate.Date,
                RecommendedTime = cityTour.StartTime ?? new TimeSpan(9, 0, 0),
                HotelId = hotel.Id,
                EstimatedPrice = cityTour.FinalPrice * 0.1m, // Tur fiyatının %10'u kadar tahmini transfer ücreti
                Priority = 2 // Orta öncelik
            };
        }

        public async Task<TransferRecommendationDto?> RecommendTransferForYachtTour(int guestId, int yachtTourId)
        {
            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null || !guest.HotelId.HasValue)
                return null;

            var yachtTour = await _yachtTourRepository.GetAll(
                x => x.Id == yachtTourId,
                x => x.City,
                x => x.PickupHotel)
                .FirstOrDefaultAsync();

            if (yachtTour == null)
                return null;

            var hotel = await _hotelRepository.GetByIdAsync(guest.HotelId.Value);
            if (hotel == null)
                return null;

            // Zaten bu transfer var mı kontrol et
            var existingTransfer = await _transferRepository.GetAll(
                x => x.GuestId == guestId &&
                     x.TransferDate.Date == yachtTour.TourDate.Date &&
                     x.TransferType == TransferType.HotelToCity)
                .FirstOrDefaultAsync();

            if (existingTransfer != null)
                return null;

            var pickupPier = yachtTour.PickupPier ?? "İskele";

            return new TransferRecommendationDto
            {
                RecommendedTransferType = TransferType.HotelToCity,
                RecommendationReason = $"Yat turu için otelden iskeleye transfer önerilir",
                PickupAddress = hotel.Address,
                DropoffAddress = pickupPier,
                RecommendedDate = yachtTour.TourDate.Date,
                RecommendedTime = yachtTour.StartTime ?? new TimeSpan(10, 0, 0),
                HotelId = hotel.Id,
                EstimatedPrice = yachtTour.FinalPrice * 0.1m,
                Priority = 2
            };
        }

        public async Task<TransferRecommendationDto?> RecommendTransferForRestaurantReservation(int guestId, int restaurantReservationId)
        {
            var guest = await _guestRepository.GetAll(x => x.Id == guestId, x => x.Hotel).FirstOrDefaultAsync();
            if (guest == null || !guest.HotelId.HasValue)
                return null;

            var reservation = await _restaurantReservationRepository.GetAll(
                x => x.Id == restaurantReservationId,
                x => x.Restaurant)
                .FirstOrDefaultAsync();

            if (reservation == null || reservation.Restaurant == null)
                return null;

            var hotel = await _hotelRepository.GetByIdAsync(guest.HotelId.Value);
            if (hotel == null)
                return null;

            // Zaten bu transfer var mı kontrol et
            var existingTransfer = await _transferRepository.GetAll(
                x => x.GuestId == guestId &&
                     x.Id == reservation.TransferId)
                .FirstOrDefaultAsync();

            if (existingTransfer != null)
                return null;

            return new TransferRecommendationDto
            {
                RecommendedTransferType = TransferType.HotelToRestaurant,
                RecommendationReason = $"Restoran rezervasyonu için otelden restorana transfer önerilir",
                PickupAddress = hotel.Address,
                DropoffAddress = reservation.Restaurant.Address,
                RecommendedDate = reservation.ReservationDate.Date,
                RecommendedTime = reservation.ReservationTime,
                HotelId = hotel.Id,
                RestaurantId = reservation.RestaurantId,
                EstimatedPrice = 0, // Restoran transferleri genellikle ücretsiz veya sabit ücret
                Priority = 3 // Düşük öncelik
            };
        }
    }
}

