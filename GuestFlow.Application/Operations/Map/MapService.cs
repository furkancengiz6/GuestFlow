// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Operations.Map.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Map
{
    /// <summary>
    /// Harita servisi implementasyonu
    /// </summary>
    public class MapService : IMapService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<TransferEntity> _transferRepository;
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly IRepository<CityEntity> _cityRepository;
        private readonly IRepository<HotelEntity> _hotelRepository;
        private readonly IGeocodingService _geocodingService;
        private readonly ILogger<MapService> _logger;

        public MapService(
            IUnitOfWork unitOfWork,
            IRepository<TransferEntity> transferRepository,
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            IRepository<CityEntity> cityRepository,
            IRepository<HotelEntity> hotelRepository,
            IGeocodingService geocodingService,
            ILogger<MapService> logger)
        {
            _unitOfWork = unitOfWork;
            _transferRepository = transferRepository;
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _cityRepository = cityRepository;
            _hotelRepository = hotelRepository;
            _geocodingService = geocodingService;
            _logger = logger;
        }

        public async Task<MapViewDto> GetMapViewAsync(MapFilterDto? filter = null)
        {
            try
            {
                var now = DateTime.UtcNow;
                var startDate = filter?.StartDate ?? now.Date;
                var endDate = filter?.EndDate ?? now.Date.AddDays(1);

                var result = new MapViewDto
                {
                    Date = startDate,
                    Services = new List<MapServiceLocationDto>(),
                    Statistics = new MapStatisticsDto()
                };

                // Transfers
                if (filter == null || filter.ServiceTypes == null || filter.ServiceTypes.Contains("Transfer"))
                {
                    var transfers = await GetTransferLocationsAsync(startDate, endDate, filter);
                    result.Services.AddRange(transfers);
                }

                // City Tours
                if (filter == null || filter.ServiceTypes == null || filter.ServiceTypes.Contains("CityTour"))
                {
                    var cityTours = await GetCityTourLocationsAsync(startDate, endDate, filter);
                    result.Services.AddRange(cityTours);
                }

                // Yacht Tours
                if (filter == null || filter.ServiceTypes == null || filter.ServiceTypes.Contains("YachtTour"))
                {
                    var yachtTours = await GetYachtTourLocationsAsync(startDate, endDate, filter);
                    result.Services.AddRange(yachtTours);
                }

                // Calculate statistics
                result.Statistics = CalculateStatistics(result.Services);

                // Calculate bounds
                result.Bounds = CalculateBounds(result.Services);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Harita görünümü getirilirken hata oluştu");
                throw;
            }
        }

        public async Task<MapServiceLocationDto?> GetServiceLocationAsync(int serviceId, string serviceType)
        {
            try
            {
                return serviceType switch
                {
                    "Transfer" => await GetTransferLocationAsync(serviceId),
                    "CityTour" => await GetCityTourLocationAsync(serviceId),
                    "YachtTour" => await GetYachtTourLocationAsync(serviceId),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Servis lokasyonu getirilirken hata oluştu: ServiceId={ServiceId}, ServiceType={ServiceType}", serviceId, serviceType);
                return null;
            }
        }

        public async Task<MapLocationDto?> GeocodeAddressAsync(string address, string? cityName = null)
        {
            try
            {
                return await _geocodingService.GeocodeAsync(address, cityName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adres geocoding yapılırken hata oluştu: Address={Address}", address);
                return null;
            }
        }

        private async Task<List<MapServiceLocationDto>> GetTransferLocationsAsync(DateTime startDate, DateTime endDate, MapFilterDto? filter)
        {
            var query = _transferRepository.GetAll()
                .Where(t => !t.IsDeleted &&
                           t.TransferDate >= startDate &&
                           t.TransferDate <= endDate)
                .Include(t => t.Guest)
                .Include(t => t.Personnel)
                .Include(t => t.PickupCity)
                .Include(t => t.DropoffCity)
                .Include(t => t.Airport)
                .AsQueryable();

            // Apply filters
            if (filter?.Statuses != null && filter.Statuses.Any())
                query = query.Where(t => filter.Statuses.Contains(t.Status ?? ""));

            if (filter?.CityId.HasValue == true)
                query = query.Where(t => t.PickupCityId == filter.CityId || t.DropoffCityId == filter.CityId);

            if (filter?.PersonnelId.HasValue == true)
                query = query.Where(t => t.PersonnelId == filter.PersonnelId);

            var transfers = await query.ToListAsync();
            var result = new List<MapServiceLocationDto>();

            foreach (var transfer in transfers)
            {
                var now = DateTime.UtcNow;
                var isUrgent = transfer.TransferDate <= now.AddHours(3) && transfer.TransferDate > now;
                var isDelayed = transfer.TransferDate < now && (transfer.Status == "Confirmed" || transfer.Status == "Pending");

                var serviceLocation = new MapServiceLocationDto
                {
                    ServiceId = transfer.Id,
                    ServiceType = "Transfer",
                    ServiceName = $"Transfer #{transfer.Id}",
                    ServiceDate = transfer.TransferDate,
                    Status = transfer.Status ?? "Pending",
                    GuestId = transfer.GuestId,
                    GuestName = transfer.Guest?.FullName ?? "Bilinmiyor",
                    RoomNumber = transfer.Guest?.RoomNumber,
                    PersonnelId = transfer.PersonnelId,
                    PersonnelName = transfer.Personnel?.FullName,
                    IsUrgent = isUrgent,
                    IsDelayed = isDelayed,
                    ColorCode = GetStatusColor(transfer.Status, isUrgent, isDelayed),
                    Amount = transfer.FinalPrice,
                    Currency = transfer.Currency,
                    Notes = transfer.Note
                };

                // Geocode pickup location
                var pickupAddress = transfer.PickupAddress;
                if (transfer.Airport != null)
                    pickupAddress = $"{transfer.Airport.Name}, {transfer.Airport.City?.CityName}";
                else if (transfer.PickupCity != null)
                    pickupAddress = $"{pickupAddress}, {transfer.PickupCity.CityName}";

                serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress, transfer.PickupCity?.CityName);
                if (serviceLocation.PickupLocation != null)
                    serviceLocation.PickupLocation.Label = "Pickup";

                // Geocode dropoff location
                var dropoffAddress = transfer.DropoffAddress;
                if (transfer.DropoffCity != null)
                    dropoffAddress = $"{dropoffAddress}, {transfer.DropoffCity.CityName}";

                serviceLocation.DropoffLocation = await _geocodingService.GeocodeAsync(dropoffAddress, transfer.DropoffCity?.CityName);
                if (serviceLocation.DropoffLocation != null)
                    serviceLocation.DropoffLocation.Label = "Dropoff";

                result.Add(serviceLocation);
            }

            return result;
        }

        private async Task<List<MapServiceLocationDto>> GetCityTourLocationsAsync(DateTime startDate, DateTime endDate, MapFilterDto? filter)
        {
            var query = _cityTourRepository.GetAll()
                .Where(ct => !ct.IsDeleted &&
                            ct.TourDate >= startDate &&
                            ct.TourDate <= endDate)
                .Include(ct => ct.OwnerGuest)
                .Include(ct => ct.Personnel)
                .Include(ct => ct.City)
                .Include(ct => ct.PickupHotel)
                .AsQueryable();

            if (filter?.CityId.HasValue == true)
                query = query.Where(ct => ct.CityId == filter.CityId);

            if (filter?.PersonnelId.HasValue == true)
                query = query.Where(ct => ct.PersonnelId == filter.PersonnelId || ct.TourGuideId == filter.PersonnelId);

            var tours = await query.ToListAsync();
            var result = new List<MapServiceLocationDto>();

            foreach (var tour in tours)
            {
                var now = DateTime.UtcNow;
                var isUrgent = tour.TourDate <= now.AddDays(1) && tour.TourDate > now;

                var serviceLocation = new MapServiceLocationDto
                {
                    ServiceId = tour.Id,
                    ServiceType = "CityTour",
                    ServiceName = $"City Tour #{tour.Id}",
                    ServiceDate = tour.TourDate,
                    Status = "Confirmed", // CityTour doesn't have status field
                    GuestId = tour.OwnerGuestId,
                    GuestName = tour.OwnerGuest?.FullName ?? "Bilinmiyor",
                    RoomNumber = tour.OwnerGuest?.RoomNumber,
                    PersonnelId = tour.TourGuideId ?? tour.PersonnelId,
                    PersonnelName = tour.GuideName ?? tour.Personnel?.FullName,
                    IsUrgent = isUrgent,
                    IsDelayed = false,
                    ColorCode = isUrgent ? "yellow" : "green",
                    Amount = tour.FinalPrice,
                    Currency = tour.Currency,
                    Notes = tour.MeetingPointDetails
                };

                // Geocode pickup location
                var pickupAddress = tour.PickupLocation;
                if (string.IsNullOrEmpty(pickupAddress) && tour.PickupHotel != null)
                    pickupAddress = $"{tour.PickupHotel.HotelName}, {tour.PickupHotel.Address}";
                else if (string.IsNullOrEmpty(pickupAddress))
                    pickupAddress = tour.City?.CityName ?? "Unknown";

                serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress, tour.City?.CityName);
                if (serviceLocation.PickupLocation != null)
                    serviceLocation.PickupLocation.Label = "Tour Start";

                result.Add(serviceLocation);
            }

            return result;
        }

        private async Task<List<MapServiceLocationDto>> GetYachtTourLocationsAsync(DateTime startDate, DateTime endDate, MapFilterDto? filter)
        {
            var query = _yachtTourRepository.GetAll()
                .Where(yt => !yt.IsDeleted &&
                            yt.TourDate >= startDate &&
                            yt.TourDate <= endDate)
                .Include(yt => yt.OwnerGuest)
                .Include(yt => yt.Personnel)
                .Include(yt => yt.PickupHotel)
                .AsQueryable();

            if (filter?.PersonnelId.HasValue == true)
                query = query.Where(yt => yt.PersonnelId == filter.PersonnelId);

            var tours = await query.ToListAsync();
            var result = new List<MapServiceLocationDto>();

            foreach (var tour in tours)
            {
                var now = DateTime.UtcNow;
                var isUrgent = tour.TourDate <= now.AddDays(1) && tour.TourDate > now;

                var serviceLocation = new MapServiceLocationDto
                {
                    ServiceId = tour.Id,
                    ServiceType = "YachtTour",
                    ServiceName = $"Yacht Tour #{tour.Id}",
                    ServiceDate = tour.TourDate,
                    Status = "Confirmed",
                    GuestId = tour.OwnerGuestId,
                    GuestName = tour.OwnerGuest?.FullName ?? "Bilinmiyor",
                    RoomNumber = tour.OwnerGuest?.RoomNumber,
                    PersonnelId = tour.PersonnelId,
                    PersonnelName = tour.Personnel?.FullName,
                    IsUrgent = isUrgent,
                    IsDelayed = false,
                    ColorCode = isUrgent ? "yellow" : "green",
                    Amount = tour.FinalPrice,
                    Currency = tour.Currency,
                    Notes = tour.SpecialRequest
                };

                // Geocode pickup location (marina/pier)
                var pickupAddress = tour.PickupPier ?? tour.PierAddress;
                if (string.IsNullOrEmpty(pickupAddress) && tour.PickupHotel != null)
                    pickupAddress = $"{tour.PickupHotel.HotelName}, {tour.PickupHotel.Address}";
                else if (string.IsNullOrEmpty(pickupAddress))
                    pickupAddress = "Marina";

                serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress);
                if (serviceLocation.PickupLocation != null)
                    serviceLocation.PickupLocation.Label = "Marina";

                result.Add(serviceLocation);
            }

            return result;
        }

        private async Task<MapServiceLocationDto?> GetTransferLocationAsync(int transferId)
        {
            var transfer = await _transferRepository.GetAll()
                .Include(t => t.Guest)
                .Include(t => t.Personnel)
                .Include(t => t.PickupCity)
                .Include(t => t.DropoffCity)
                .Include(t => t.Airport)
                .FirstOrDefaultAsync(t => t.Id == transferId && !t.IsDeleted);

            if (transfer == null) return null;

            var now = DateTime.UtcNow;
            var isUrgent = transfer.TransferDate <= now.AddHours(3) && transfer.TransferDate > now;
            var isDelayed = transfer.TransferDate < now && (transfer.Status == "Confirmed" || transfer.Status == "Pending");

            var serviceLocation = new MapServiceLocationDto
            {
                ServiceId = transfer.Id,
                ServiceType = "Transfer",
                ServiceName = $"Transfer #{transfer.Id}",
                ServiceDate = transfer.TransferDate,
                Status = transfer.Status ?? "Pending",
                GuestId = transfer.GuestId,
                GuestName = transfer.Guest?.FullName ?? "Bilinmiyor",
                RoomNumber = transfer.Guest?.RoomNumber,
                PersonnelId = transfer.PersonnelId,
                PersonnelName = transfer.Personnel?.FullName,
                IsUrgent = isUrgent,
                IsDelayed = isDelayed,
                ColorCode = GetStatusColor(transfer.Status, isUrgent, isDelayed),
                Amount = transfer.FinalPrice,
                Currency = transfer.Currency,
                Notes = transfer.Note
            };

            // Geocode locations
            var pickupAddress = transfer.PickupAddress;
            if (transfer.Airport != null)
                pickupAddress = $"{transfer.Airport.Name}, {transfer.Airport.City?.CityName}";

            serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress, transfer.PickupCity?.CityName);
            if (serviceLocation.PickupLocation != null)
                serviceLocation.PickupLocation.Label = "Pickup";

            serviceLocation.DropoffLocation = await _geocodingService.GeocodeAsync(transfer.DropoffAddress, transfer.DropoffCity?.CityName);
            if (serviceLocation.DropoffLocation != null)
                serviceLocation.DropoffLocation.Label = "Dropoff";

            return serviceLocation;
        }

        private async Task<MapServiceLocationDto?> GetCityTourLocationAsync(int tourId)
        {
            var tour = await _cityTourRepository.GetAll()
                .Include(ct => ct.OwnerGuest)
                .Include(ct => ct.Personnel)
                .Include(ct => ct.City)
                .Include(ct => ct.PickupHotel)
                .FirstOrDefaultAsync(ct => ct.Id == tourId && !ct.IsDeleted);

            if (tour == null) return null;

            var now = DateTime.UtcNow;
            var isUrgent = tour.TourDate <= now.AddDays(1) && tour.TourDate > now;

            var serviceLocation = new MapServiceLocationDto
            {
                ServiceId = tour.Id,
                ServiceType = "CityTour",
                ServiceName = $"City Tour #{tour.Id}",
                ServiceDate = tour.TourDate,
                Status = "Confirmed",
                GuestId = tour.OwnerGuestId,
                GuestName = tour.OwnerGuest?.FullName ?? "Bilinmiyor",
                RoomNumber = tour.OwnerGuest?.RoomNumber,
                PersonnelId = tour.TourGuideId ?? tour.PersonnelId,
                PersonnelName = tour.GuideName ?? tour.Personnel?.FullName,
                IsUrgent = isUrgent,
                ColorCode = isUrgent ? "yellow" : "green",
                Amount = tour.FinalPrice,
                Currency = tour.Currency
            };

            var pickupAddress = tour.PickupLocation;
            if (string.IsNullOrEmpty(pickupAddress) && tour.PickupHotel != null)
                pickupAddress = $"{tour.PickupHotel.HotelName}, {tour.PickupHotel.Address}";

            serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress, tour.City?.CityName);
            if (serviceLocation.PickupLocation != null)
                serviceLocation.PickupLocation.Label = "Tour Start";

            return serviceLocation;
        }

        private async Task<MapServiceLocationDto?> GetYachtTourLocationAsync(int tourId)
        {
            var tour = await _yachtTourRepository.GetAll()
                .Include(yt => yt.OwnerGuest)
                .Include(yt => yt.Personnel)
                .Include(yt => yt.PickupHotel)
                .FirstOrDefaultAsync(yt => yt.Id == tourId && !yt.IsDeleted);

            if (tour == null) return null;

            var now = DateTime.UtcNow;
            var isUrgent = tour.TourDate <= now.AddDays(1) && tour.TourDate > now;

            var serviceLocation = new MapServiceLocationDto
            {
                ServiceId = tour.Id,
                ServiceType = "YachtTour",
                ServiceName = $"Yacht Tour #{tour.Id}",
                ServiceDate = tour.TourDate,
                Status = "Confirmed",
                GuestId = tour.OwnerGuestId,
                GuestName = tour.OwnerGuest?.FullName ?? "Bilinmiyor",
                RoomNumber = tour.OwnerGuest?.RoomNumber,
                PersonnelId = tour.PersonnelId,
                PersonnelName = tour.Personnel?.FullName,
                IsUrgent = isUrgent,
                ColorCode = isUrgent ? "yellow" : "green",
                Amount = tour.FinalPrice,
                Currency = tour.Currency
            };

            var pickupAddress = tour.PickupPier ?? tour.PierAddress ?? "Marina";
            serviceLocation.PickupLocation = await _geocodingService.GeocodeAsync(pickupAddress);
            if (serviceLocation.PickupLocation != null)
                serviceLocation.PickupLocation.Label = "Marina";

            return serviceLocation;
        }

        private string GetStatusColor(string? status, bool isUrgent, bool isDelayed)
        {
            if (isDelayed) return "red";
            if (isUrgent) return "yellow";
            if (status == "Completed") return "green";
            if (status == "InProgress") return "blue";
            return "gray";
        }

        private MapStatisticsDto CalculateStatistics(List<MapServiceLocationDto> services)
        {
            return new MapStatisticsDto
            {
                TotalServices = services.Count,
                ConfirmedServices = services.Count(s => s.Status == "Confirmed"),
                InProgressServices = services.Count(s => s.Status == "InProgress"),
                CompletedServices = services.Count(s => s.Status == "Completed"),
                UrgentServices = services.Count(s => s.IsUrgent),
                DelayedServices = services.Count(s => s.IsDelayed)
            };
        }

        private MapBoundsDto? CalculateBounds(List<MapServiceLocationDto> services)
        {
            var locations = services
                .SelectMany(s => new[] { s.PickupLocation, s.DropoffLocation })
                .Where(l => l != null)
                .ToList();

            if (!locations.Any()) return null;

            return new MapBoundsDto
            {
                North = locations.Max(l => l!.Latitude),
                South = locations.Min(l => l!.Latitude),
                East = locations.Max(l => l!.Longitude),
                West = locations.Min(l => l!.Longitude)
            };
        }
    }
}
