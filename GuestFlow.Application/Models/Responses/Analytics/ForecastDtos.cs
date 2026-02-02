using System;

namespace GuestFlow.Application.Models.Responses.Analytics
{
    public class OccupancyForecastDto
    {
        public DateTime Date { get; set; }
        public double ForecastedOccupancyRate { get; set; }
        public int ForecastedBookedRooms { get; set; }
        public double ConfidenceIntervalLower { get; set; }
        public double ConfidenceIntervalUpper { get; set; }
    }

    public class RevenueForecastDto
    {
        public DateTime Date { get; set; }
        public decimal ForecastedRevenue { get; set; }
        public decimal ForecastedRevPAR { get; set; }
        public string Currency { get; set; } = "TRY";
    }
}
