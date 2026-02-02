using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GuestFlow.Application.Models.Responses.Analytics;

namespace GuestFlow.Application.Operations.Intelligence.Predictive
{
    public interface IPredictiveAnalyticsService
    {
        Task<List<OccupancyForecastDto>> PredictOccupancyAsync(DateTime startDate, DateTime endDate);
        Task<List<RevenueForecastDto>> PredictRevenueAsync(DateTime startDate, DateTime endDate);
    }
}
