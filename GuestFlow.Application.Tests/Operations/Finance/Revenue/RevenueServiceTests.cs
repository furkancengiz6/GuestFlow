using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Finance.Revenue;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Tests.Helpers;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Finance.Revenue
{
    public class RevenueServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepository<ReservationEntity>> _reservationsRepoMock;
        private readonly RevenueService _service;

        public RevenueServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _reservationsRepoMock = new Mock<IRepository<ReservationEntity>>();

            _unitOfWorkMock.Setup(u => u.Reservations).Returns(_reservationsRepoMock.Object);

            _service = new RevenueService(_unitOfWorkMock.Object);
        }

        [Fact]
        public async Task GetRevenueDashboardAsync_CalculatesMetricsCorrectly()
        {
            // Arrange
            var startDate = new DateTime(2024, 6, 1);
            var endDate = new DateTime(2024, 6, 30); // 30 days

            // Mock reservations
            // Res 1: 5 nights, 500 total (100/night)
            var res1 = new ReservationEntity
            {
                CheckInDate = new DateTime(2024, 6, 1),
                CheckOutDate = new DateTime(2024, 6, 6), // 5 nights
                TotalAmount = 500, // Use TotalAmount
                Status = ReservationStatus.Confirmed
            };

            // Res 2: 2 nights, 300 total (150/night)
            var res2 = new ReservationEntity
            {
                CheckInDate = new DateTime(2024, 6, 10),
                CheckOutDate = new DateTime(2024, 6, 12), // 2 nights
                TotalAmount = 300,
                Status = ReservationStatus.Confirmed
            };

            // Res 3: Overlapping start (CheckIn May 30, CheckOut June 2) - 1 night in June (June 1)
            // Total Price 300 for 3 nights (100/night). 1 night in June = 100 revenue.
            var res3 = new ReservationEntity
            {
                CheckInDate = new DateTime(2024, 5, 30),
                CheckOutDate = new DateTime(2024, 6, 2), // 3 nights total. June 1 is inside.
                TotalAmount = 300,
                Status = ReservationStatus.Confirmed
            };
            // Logic in service: Intersection with [Start, End].
            // Res3: Max(May30, June1) = June1. Min(June2, June30) = June2.
            // Nights = (June2 - June1).Days = 1.
            // Revenue = (300 / 3) * 1 = 100.

            var mockData = new List<ReservationEntity> { res1, res2, res3 };
            var mockQueryable = mockData.BuildMockQueryable();
            _reservationsRepoMock.Setup(r => r.GetAll(It.IsAny<Expression<Func<ReservationEntity, bool>>>(), It.IsAny<bool>()))
                .Returns(mockQueryable.Object);

            // Act
            var result = await _service.GetRevenueDashboardAsync(startDate, endDate);

            // Assert
            // Total Room Revenue:
            // Res 1: 500 (5 nights * 100)
            // Res 2: 300 (2 nights * 150)
            // Res 3: 100 (1 night * 100)
            // Total = 900
            Assert.Equal(900, result.TotalRevenue);

            // Total Rooms Sold: 5 + 2 + 1 = 8
            Assert.Equal(8, result.TotalRoomsSold);

            // ADR: 900 / 8 = 112.5
            Assert.Equal(112.5m, result.ADR);

            // Total Available Rooms: 30 days * 50 rooms = 1500
            // Occupancy Rate: 8 / 1500 = 0.0053 (0.53%)
            Assert.Equal(0.01m, result.OccupancyRate); // Rounded to 2 decimals? 0.0053 -> 0.01

            // RevPAR: 900 / 1500 = 0.6
            Assert.Equal(0.6m, result.RevPAR);
        }

        [Fact]
        public async Task GetRevenueDashboardAsync_NoReservations_ReturnsZero()
        {
            // Arrange
            var mockData = new List<ReservationEntity>();
            var mockQueryable = mockData.BuildMockQueryable();
            _reservationsRepoMock.Setup(r => r.GetAll(It.IsAny<Expression<Func<ReservationEntity, bool>>>(), It.IsAny<bool>()))
                .Returns(mockQueryable.Object);

            // Act
            var result = await _service.GetRevenueDashboardAsync(DateTime.Today, DateTime.Today.AddDays(1));

            // Assert
            Assert.Equal(0, result.TotalRevenue);
            Assert.Equal(0, result.ADR);
            Assert.Equal(0, result.RevPAR);
        }
    }
}
