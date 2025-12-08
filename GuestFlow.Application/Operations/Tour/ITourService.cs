using GuestFlow.Application.Operations.CityTour.Dtos;
using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Tour
{
    /// <summary>
    /// Birleşik tur servisi (CityTour ve YachtTour için)
    /// </summary>
    public interface ITourService
    {
        /// <summary>
        /// Tur takvim görünümünü getirir (CityTour ve YachtTour birleşik)
        /// </summary>
        Task<TourCalendarDto> GetTourCalendarAsync(DateTime? startDate = null, DateTime? endDate = null);
        
        /// <summary>
        /// Tur istatistiklerini getirir (CityTour ve YachtTour birleşik)
        /// </summary>
        Task<TourStatisticsDto> GetTourStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}

