using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GuestFlow.Application.Models.Responses.Analytics;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.UnitOfWork;

namespace GuestFlow.Application.Operations.Intelligence.Predictive
{
    public class PredictiveAnalyticsService : IPredictiveAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PredictiveAnalyticsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<OccupancyForecastDto>> PredictOccupancyAsync(DateTime startDate, DateTime endDate)
        {
            var forecasts = new List<OccupancyForecastDto>();
            var days = (endDate - startDate).Days + 1;
            const int TotalRooms = 100; // MVP assumption

            // Fetch all relevant historical reservations once to minimize DB hits
            // Range: [Start - 1 Year] to [End - 1 Year]
            var historicalStart = startDate.AddYears(-1);
            var historicalEnd = endDate.AddYears(-1);

            // Fetch reservations that overlap with the historical period
            var historicalReservations = await _unitOfWork.Reservations.GetQueryableBySpecification(null) // Using GetAll equivalent since Spec is null
                .Where(r => r.CheckInDate <= historicalEnd && 
                            r.CheckOutDate > historicalStart && 
                            r.Status != Domain.Entities.Enum.ReservationStatus.Cancelled)
                .ToListAsync(); // Materialize to memory for daily processing

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var targetDate = date.AddYears(-1);

                // Count active rooms on targetDate
                var occupiedRooms = historicalReservations.Count(r => 
                    r.CheckInDate <= targetDate && 
                    r.CheckOutDate > targetDate);

                // Simple Linear Regression / Growth Factor
                var historicalOccupancy = (double)occupiedRooms / TotalRooms;
                
                // Heuristic: Apply 5% YoY growth
                var forecastRate = historicalOccupancy * 1.05;

                // Adjust for weekends if historical comparison aligns differently (simple check)
                if ((date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Saturday) && 
                    (targetDate.DayOfWeek != DayOfWeek.Friday && targetDate.DayOfWeek != DayOfWeek.Saturday))
                {
                    forecastRate += 0.10; // Add weekend boost if this year is weekend but last year wasn't
                }

                forecastRate = Math.Clamp(forecastRate, 0, 1);

                forecasts.Add(new OccupancyForecastDto
                {
                    Date = date,
                    ForecastedOccupancyRate = Math.Round(forecastRate, 2),
                    ForecastedBookedRooms = (int)(forecastRate * TotalRooms),
                    ConfidenceIntervalLower = Math.Round(Math.Max(0, forecastRate - 0.1), 2),
                    ConfidenceIntervalUpper = Math.Round(Math.Min(1, forecastRate + 0.1), 2)
                });
            }

            return forecasts;
        }

        public async Task<List<RevenueForecastDto>> PredictRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var forecasts = new List<RevenueForecastDto>();
            var days = (endDate - startDate).Days + 1;

            var historicalStart = startDate.AddYears(-1);
            var historicalEnd = endDate.AddYears(-1);

            var historicalReservations = await _unitOfWork.Reservations.GetQueryableBySpecification(null) 
                .Where(r => r.CheckInDate <= historicalEnd && 
                            r.CheckOutDate > historicalStart &&
                            r.Status != Domain.Entities.Enum.ReservationStatus.Cancelled)
                .ToListAsync();

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var targetDate = date.AddYears(-1);

                decimal dailyRevenue = 0;

                // Sum up daily revenue from active reservations
                foreach (var res in historicalReservations.Where(r => r.CheckInDate <= targetDate && r.CheckOutDate > targetDate))
                {
                    if (res.CheckInDate.HasValue && res.CheckOutDate.HasValue && res.TotalAmount > 0)
                    {
                        var duration = (res.CheckOutDate.Value - res.CheckInDate.Value).TotalDays;
                        if (duration > 0)
                        {
                            dailyRevenue += res.TotalAmount / (decimal)duration;
                        }
                    }
                }

                // Apply Inflation/Growth (e.g., 10%)
                var forecastRevenue = dailyRevenue * 1.10m;

                forecasts.Add(new RevenueForecastDto
                {
                    Date = date,
                    ForecastedRevenue = Math.Round(forecastRevenue, 2),
                    ForecastedRevPAR = Math.Round(forecastRevenue / 100m, 2), // Assuming 100 rooms
                    Currency = "TRY"
                });
            }

            return forecasts;
        }
    }
}
