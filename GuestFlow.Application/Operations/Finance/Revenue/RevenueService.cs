using System;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Application.Operations.Finance.Revenue
{
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        // Hardcoded total rooms for MVP. In reality, this should be count of active Rooms in DB.
        private const int TotalInventory = 50; 

        public RevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<RevenueDashboardDto> GetRevenueDashboardAsync(DateTime startDate, DateTime endDate)
        {
            // Normalize dates
            startDate = startDate.Date;
            endDate = endDate.Date;
            var daysInPeriod = (endDate - startDate).Days + 1;
            if (daysInPeriod <= 0) daysInPeriod = 1;

            // Fetch reservations overlapping the period
            var reservations = await _unitOfWork.Reservations.GetAll()
                .Where(r => r.CheckInDate.HasValue && r.CheckOutDate.HasValue)
                .Where(r => r.CheckInDate <= endDate && r.CheckOutDate >= startDate)
                .Where(r => r.Status != ReservationStatus.Cancelled) // Assuming Enum usage or string
                .ToListAsync();

            decimal totalRoomRevenue = 0;
            int totalRoomsSold = 0;

            foreach (var res in reservations)
            {
                if (!res.CheckInDate.HasValue || !res.CheckOutDate.HasValue) continue;

                // Calculate intersection of reservation dates and query period
                var start = res.CheckInDate.Value < startDate ? startDate : res.CheckInDate.Value;
                var end = res.CheckOutDate.Value > endDate ? endDate : res.CheckOutDate.Value;
                
                var nights = (end - start).Days;
                // If distinct check-in/out logic applies (e.g. stay over night), nights logic holds.
                // Note: CheckOutDate is usually "day of departure", so not a stayed night.
                // Assuming CheckOutDate is exclusive for night calculation.
                // If intersection is same day, 0 nights.

                if (nights > 0)
                {
                    totalRoomsSold += nights;
                    
                    // Simple pro-ration: Average Nightly Rate * Nights
                    // Ideally we should look at daily rates if variable.
                    // Assuming TotalPrice / TotalNights = StartRate for now.
                    var totalNights = (res.CheckOutDate.Value - res.CheckInDate.Value).Days;
                    if (totalNights > 0)
                    {
                        var dailyRate = res.TotalAmount / totalNights; // Use TotalAmount instead of TotalPrice
                        totalRoomRevenue += dailyRate * nights;
                    }
                }
            }

            var adr = totalRoomsSold > 0 ? totalRoomRevenue / totalRoomsSold : 0;
            var totalAvailableRooms = TotalInventory * daysInPeriod;
            var occupancyRate = totalAvailableRooms > 0 ? (decimal)totalRoomsSold / totalAvailableRooms : 0;
            var revPar = totalAvailableRooms > 0 ? totalRoomRevenue / totalAvailableRooms : 0;

            return new RevenueDashboardDto
            {
                ADR = Math.Round(adr, 2),
                RevPAR = Math.Round(revPar, 2),
                OccupancyRate = Math.Round(occupancyRate, 2),
                TotalRevenue = Math.Round(totalRoomRevenue, 2),
                TotalRoomsSold = totalRoomsSold
            };
        }
    }
}
