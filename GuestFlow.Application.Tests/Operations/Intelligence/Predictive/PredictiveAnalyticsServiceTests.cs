using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Tests.Helpers;
using MockQueryable.Moq;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Intelligence.Predictive
{
    public class PredictiveAnalyticsServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IRepository<ReservationEntity>> _mockReservationRepo;
        private readonly PredictiveAnalyticsService _service;

        public PredictiveAnalyticsServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockReservationRepo = new Mock<IRepository<ReservationEntity>>();

            _mockUnitOfWork.Setup(u => u.Reservations).Returns(_mockReservationRepo.Object);

            _service = new PredictiveAnalyticsService(_mockUnitOfWork.Object);
        }

        [Fact]
        public async Task PredictOccupancyAsync_ShouldCalculateBasedOnHistoricalData()
        {
            // Arrange
            var startDate = new DateTime(2025, 6, 1);
            var endDate = new DateTime(2025, 6, 2);

            // Historical dates (1 year prior)
            var historyDate1 = new DateTime(2024, 6, 1); 
            
            var reservations = new List<ReservationEntity>
            {
                // Reservation 1: Historically active overlap
                new ReservationEntity 
                { 
                    CheckInDate = historyDate1, 
                    CheckOutDate = historyDate1.AddDays(2), 
                    Status = ReservationStatus.Confirmed 
                },
                // Reservation 2: Cancelled (should be ignored)
                new ReservationEntity 
                { 
                    CheckInDate = historyDate1, 
                    CheckOutDate = historyDate1.AddDays(2), 
                    Status = ReservationStatus.Cancelled 
                }
            };

            // Setup Mock
            var mockQueryable = reservations.AsQueryable().BuildMock();
            _mockReservationRepo.Setup(r => r.GetQueryableBySpecification(null)).Returns(mockQueryable);

            // Act
            var result = await _service.PredictOccupancyAsync(startDate, endDate);

            // Assert
            Assert.Equal(2, result.Count); // 2 days
            
            // For June 1st 2025 (target history June 1st 2024):
            // 1 active reservation / 100 rooms = 1% occupancy
            // Growth = 1% * 1.05 = 1.05% => 0.0105
            var day1 = result.First(r => r.Date == startDate);
            Assert.True(day1.ForecastedOccupancyRate > 0);
            Assert.Equal(0.01, Math.Round(day1.ForecastedOccupancyRate, 2)); // Round(0.0105, 2) -> 0.01
        }

        [Fact]
        public async Task PredictRevenueAsync_ShouldCalculateBasedOnHistoricalRevenue()
        {
            // Arrange
            var startDate = new DateTime(2025, 6, 1);
            var endDate = new DateTime(2025, 6, 1); // 1 day prediction

            var historyStart = new DateTime(2024, 6, 1);
            
            var reservations = new List<ReservationEntity>
            {
                // Reservation: 1000 TRY total for 2 nights = 500 TRY/night
                new ReservationEntity 
                { 
                    CheckInDate = historyStart, 
                    CheckOutDate = historyStart.AddDays(2), 
                    TotalAmount = 1000m,
                    Status = ReservationStatus.Confirmed 
                }
            };

            var mockQueryable = reservations.AsQueryable().BuildMock();
            _mockReservationRepo.Setup(r => r.GetQueryableBySpecification(null)).Returns(mockQueryable);

            // Act
            var result = await _service.PredictRevenueAsync(startDate, endDate);

            // Assert
            var day1 = result.First();
            // Expected Daily Revenue = 500
            // Growth 10% = 550
            Assert.Equal(550m, day1.ForecastedRevenue);
        }
    }
}
