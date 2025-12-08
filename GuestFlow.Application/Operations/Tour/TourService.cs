using GuestFlow.Application.Operations.CityTour.Dtos;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.YachtTour;

namespace GuestFlow.Application.Operations.Tour
{
    public class TourService : ITourService
    {
        private readonly IRepository<CityTourEntity> _cityTourRepository;
        private readonly IRepository<YachtTourEntity> _yachtTourRepository;
        private readonly ILogger<TourService> _logger;

        public TourService(
            IRepository<CityTourEntity> cityTourRepository,
            IRepository<YachtTourEntity> yachtTourRepository,
            ILogger<TourService> logger)
        {
            _cityTourRepository = cityTourRepository;
            _yachtTourRepository = yachtTourRepository;
            _logger = logger;
        }

        public async Task<TourCalendarDto> GetTourCalendarAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today;
                var end = endDate?.Date ?? today.AddDays(30);
                var weekEnd = today.AddDays(7);
                var monthEnd = today.AddMonths(1);

                // Bugünkü turlar
                var todayCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date == today && !ct.IsDeleted)
                    .Select(ct => new TourCalendarItemDto
                    {
                        Id = ct.Id,
                        TourType = "CityTour",
                        TourDate = ct.TourDate,
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : null,
                        CityName = ct.City != null ? ct.City.CityName : null,
                        FinalPrice = ct.FinalPrice,
                        AdditionalInfo = $"{ct.Language} - {ct.DurationHours} saat"
                    })
                    .ToListAsync();

                var todayYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date == today && !yt.IsDeleted)
                    .Select(yt => new TourCalendarItemDto
                    {
                        Id = yt.Id,
                        TourType = "YachtTour",
                        TourDate = yt.TourDate,
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : null,
                        CityName = yt.City != null ? yt.City.CityName : null,
                        FinalPrice = yt.FinalPrice,
                        AdditionalInfo = $"{yt.YachtName} - {yt.NumberOfPeople} kişi"
                    })
                    .ToListAsync();

                // Bu haftanın turları
                var weekCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date > today && ct.TourDate.Date <= weekEnd && !ct.IsDeleted)
                    .Select(ct => new TourCalendarItemDto
                    {
                        Id = ct.Id,
                        TourType = "CityTour",
                        TourDate = ct.TourDate,
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : null,
                        CityName = ct.City != null ? ct.City.CityName : null,
                        FinalPrice = ct.FinalPrice,
                        AdditionalInfo = $"{ct.Language} - {ct.DurationHours} saat"
                    })
                    .ToListAsync();

                var weekYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date > today && yt.TourDate.Date <= weekEnd && !yt.IsDeleted)
                    .Select(yt => new TourCalendarItemDto
                    {
                        Id = yt.Id,
                        TourType = "YachtTour",
                        TourDate = yt.TourDate,
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : null,
                        CityName = yt.City != null ? yt.City.CityName : null,
                        FinalPrice = yt.FinalPrice,
                        AdditionalInfo = $"{yt.YachtName} - {yt.NumberOfPeople} kişi"
                    })
                    .ToListAsync();

                // Bu ayın turları
                var monthCityTours = await _cityTourRepository.GetAll()
                    .Include(ct => ct.OwnerGuest)
                    .Include(ct => ct.Personnel)
                    .Include(ct => ct.City)
                    .Where(ct => ct.TourDate.Date > weekEnd && ct.TourDate.Date <= monthEnd && !ct.IsDeleted)
                    .Select(ct => new TourCalendarItemDto
                    {
                        Id = ct.Id,
                        TourType = "CityTour",
                        TourDate = ct.TourDate,
                        GuestName = ct.OwnerGuest != null ? ct.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = ct.Personnel != null ? ct.Personnel.FullName : null,
                        CityName = ct.City != null ? ct.City.CityName : null,
                        FinalPrice = ct.FinalPrice,
                        AdditionalInfo = $"{ct.Language} - {ct.DurationHours} saat"
                    })
                    .ToListAsync();

                var monthYachtTours = await _yachtTourRepository.GetAll()
                    .Include(yt => yt.OwnerGuest)
                    .Include(yt => yt.Personnel)
                    .Include(yt => yt.City)
                    .Where(yt => yt.TourDate.Date > weekEnd && yt.TourDate.Date <= monthEnd && !yt.IsDeleted)
                    .Select(yt => new TourCalendarItemDto
                    {
                        Id = yt.Id,
                        TourType = "YachtTour",
                        TourDate = yt.TourDate,
                        GuestName = yt.OwnerGuest != null ? yt.OwnerGuest.FullName : "Bilinmiyor",
                        PersonnelName = yt.Personnel != null ? yt.Personnel.FullName : null,
                        CityName = yt.City != null ? yt.City.CityName : null,
                        FinalPrice = yt.FinalPrice,
                        AdditionalInfo = $"{yt.YachtName} - {yt.NumberOfPeople} kişi"
                    })
                    .ToListAsync();

                var todayTours = todayCityTours
                    .Concat(todayYachtTours)
                    .OrderBy(t => t.TourDate)
                    .ToList();

                var weekTours = weekCityTours
                    .Concat(weekYachtTours)
                    .OrderBy(t => t.TourDate)
                    .ToList();

                var monthTours = monthCityTours
                    .Concat(monthYachtTours)
                    .OrderBy(t => t.TourDate)
                    .ToList();

                var totalUpcoming = todayTours.Count + weekTours.Count + monthTours.Count;

                return new TourCalendarDto
                {
                    Today = todayTours,
                    ThisWeek = weekTours,
                    ThisMonth = monthTours,
                    TotalUpcoming = totalUpcoming
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur takvimi getirilirken hata: {ex.Message}");
                throw;
            }
        }

        public async Task<TourStatisticsDto> GetTourStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var today = DateTime.UtcNow.Date;
                var start = startDate?.Date ?? today.AddMonths(-1);
                var end = endDate?.Date ?? today;

                var cityTours = await _cityTourRepository.GetAll()
                    .Where(ct => ct.TourDate.Date >= start && ct.TourDate.Date <= end && !ct.IsDeleted)
                    .ToListAsync();

                var yachtTours = await _yachtTourRepository.GetAll()
                    .Where(yt => yt.TourDate.Date >= start && yt.TourDate.Date <= end && !yt.IsDeleted)
                    .ToListAsync();

                var totalTours = cityTours.Count + yachtTours.Count;
                var totalGuests = cityTours.Select(ct => ct.OwnerGuestId)
                    .Concat(yachtTours.Select(yt => yt.OwnerGuestId))
                    .Distinct()
                    .Count();

                var statistics = new TourStatisticsDto
                {
                    TotalCityTours = cityTours.Count,
                    TotalYachtTours = yachtTours.Count,
                    TotalTours = totalTours,
                    CityTourRevenue = cityTours.Sum(ct => ct.FinalPrice),
                    YachtTourRevenue = yachtTours.Sum(yt => yt.FinalPrice),
                    TotalRevenue = cityTours.Sum(ct => ct.FinalPrice) + yachtTours.Sum(yt => yt.FinalPrice),
                    AveragePrice = totalTours > 0 
                        ? (cityTours.Sum(ct => ct.FinalPrice) + yachtTours.Sum(yt => yt.FinalPrice)) / totalTours 
                        : 0,
                    TotalGuests = totalGuests
                };

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Tur istatistikleri getirilirken hata: {ex.Message}");
                throw;
            }
        }
    }
}

