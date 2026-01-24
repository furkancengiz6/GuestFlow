// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using FluentAssertions;
using GuestFlow.Application.Operations.Analytics;
using GuestFlow.Application.Operations.Analytics.Dtos;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Domain.Entities.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Analytics
{
    public class AnalyticsServiceTests
    {
        private readonly Mock<IRepository<PaymentEntity>> _paymentRepositoryMock;
        private readonly Mock<IRepository<TransferEntity>> _transferRepositoryMock;
        private readonly Mock<IRepository<CityTourEntity>> _cityTourRepositoryMock;
        private readonly Mock<IRepository<YachtTourEntity>> _yachtTourRepositoryMock;
        private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
        private readonly Mock<IRepository<InvoicesEntity>> _invoiceRepositoryMock;
        private readonly Mock<IRepository<SupplierCost>> _supplierCostRepositoryMock;
        private readonly Mock<IRepository<CityEntity>> _cityRepositoryMock;
        private readonly Mock<IRepository<GuestFlow.Domain.Entities.Core.Supplier>> _supplierRepositoryMock;
        private readonly Mock<ILogger<AnalyticsService>> _loggerMock;
        private readonly AnalyticsService _analyticsService;

        public AnalyticsServiceTests()
        {
            _paymentRepositoryMock = new Mock<IRepository<PaymentEntity>>();
            _transferRepositoryMock = new Mock<IRepository<TransferEntity>>();
            _cityTourRepositoryMock = new Mock<IRepository<CityTourEntity>>();
            _yachtTourRepositoryMock = new Mock<IRepository<YachtTourEntity>>();
            _guestRepositoryMock = new Mock<IRepository<GuestEntity>>();
            _invoiceRepositoryMock = new Mock<IRepository<InvoicesEntity>>();
            _supplierCostRepositoryMock = new Mock<IRepository<SupplierCost>>();
            _cityRepositoryMock = new Mock<IRepository<CityEntity>>();
            _supplierRepositoryMock = new Mock<IRepository<GuestFlow.Domain.Entities.Core.Supplier>>();
            _loggerMock = new Mock<ILogger<AnalyticsService>>();

            _analyticsService = new AnalyticsService(
                _paymentRepositoryMock.Object,
                _transferRepositoryMock.Object,
                _cityTourRepositoryMock.Object,
                _yachtTourRepositoryMock.Object,
                _guestRepositoryMock.Object,
                _invoiceRepositoryMock.Object,
                _supplierCostRepositoryMock.Object,
                _cityRepositoryMock.Object,
                _supplierRepositoryMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task GetRealTimeKpisAsync_WithValidData_ReturnsKpis()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Id = 1,
                    Amount = 1000,
                    PaymentDate = today,
                    Status = PaymentStatus.Completed,
                    IsDeleted = false
                },
                new PaymentEntity
                {
                    Id = 2,
                    Amount = 2000,
                    PaymentDate = monthStart,
                    Status = PaymentStatus.Completed,
                    IsDeleted = false
                }
            };

            var transfers = new List<TransferEntity>
            {
                new TransferEntity { Id = 1, TransferDate = today, IsDeleted = false }
            };

            var cityTours = new List<CityTourEntity>
            {
                new CityTourEntity { Id = 1, TourDate = today, IsDeleted = false }
            };

            var yachtTours = new List<YachtTourEntity>
            {
                new YachtTourEntity { Id = 1, TourDate = today, IsDeleted = false }
            };

            var supplierCosts = new List<SupplierCost>
            {
                new SupplierCost
                {
                    Id = 1,
                    CostAmount = 500,
                    CreatedDate = today,
                    IsDeleted = false
                }
            };

            SetupMockQueryable(_paymentRepositoryMock, payments);
            SetupMockQueryable(_transferRepositoryMock, transfers);
            SetupMockQueryable(_cityTourRepositoryMock, cityTours);
            SetupMockQueryable(_yachtTourRepositoryMock, yachtTours);
            SetupMockQueryable(_supplierCostRepositoryMock, supplierCosts);

            // Act
            var result = await _analyticsService.GetRealTimeKpisAsync(today);

            // Assert
            result.Should().NotBeNull();
            result.TodayRevenue.Should().BeGreaterThanOrEqualTo(0);
            result.ThisMonthRevenue.Should().BeGreaterThanOrEqualTo(0);
            result.ThisMonthNetProfit.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task GetRealTimeKpisAsync_WithNoData_ReturnsZeroValues()
        {
            // Arrange
            var today = DateTime.UtcNow.Date;

            SetupMockQueryable(_paymentRepositoryMock, new List<PaymentEntity>());
            SetupMockQueryable(_transferRepositoryMock, new List<TransferEntity>());
            SetupMockQueryable(_cityTourRepositoryMock, new List<CityTourEntity>());
            SetupMockQueryable(_yachtTourRepositoryMock, new List<YachtTourEntity>());
            SetupMockQueryable(_supplierCostRepositoryMock, new List<SupplierCost>());

            // Act
            var result = await _analyticsService.GetRealTimeKpisAsync(today);

            // Assert
            result.Should().NotBeNull();
            result.TodayRevenue.Should().Be(0);
            result.ThisMonthRevenue.Should().Be(0);
            result.ThisMonthServiceCount.Should().Be(0);
        }

        [Fact]
        public async Task GetRevenueTrendAsync_DailyPeriod_ReturnsDailyDataPoints()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(-7);
            var endDate = DateTime.UtcNow.Date;

            var payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Id = 1,
                    Amount = 1000,
                    PaymentDate = startDate,
                    Status = PaymentStatus.Completed,
                    IsDeleted = false
                }
            };

            SetupMockQueryable(_paymentRepositoryMock, payments);
            SetupMockQueryable(_transferRepositoryMock, new List<TransferEntity>());
            SetupMockQueryable(_cityTourRepositoryMock, new List<CityTourEntity>());
            SetupMockQueryable(_yachtTourRepositoryMock, new List<YachtTourEntity>());
            SetupMockQueryable(_supplierCostRepositoryMock, new List<SupplierCost>());

            // Act
            var result = await _analyticsService.GetRevenueTrendAsync("daily", startDate, endDate, false);

            // Assert
            result.Should().NotBeNull();
            result.Period.Should().Be("daily");
            result.DataPoints.Should().NotBeNull();
            result.DataPoints.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetRevenueTrendAsync_WeeklyPeriod_ReturnsWeeklyDataPoints()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddDays(-30);
            var endDate = DateTime.UtcNow.Date;

            SetupMockQueryable(_paymentRepositoryMock, new List<PaymentEntity>());
            SetupMockQueryable(_transferRepositoryMock, new List<TransferEntity>());
            SetupMockQueryable(_cityTourRepositoryMock, new List<CityTourEntity>());
            SetupMockQueryable(_yachtTourRepositoryMock, new List<YachtTourEntity>());
            SetupMockQueryable(_supplierCostRepositoryMock, new List<SupplierCost>());

            // Act
            var result = await _analyticsService.GetRevenueTrendAsync("weekly", startDate, endDate, false);

            // Assert
            result.Should().NotBeNull();
            result.Period.Should().Be("weekly");
            result.DataPoints.Should().NotBeNull();
        }

        [Fact]
        public async Task GetRevenueTrendAsync_MonthlyPeriod_ReturnsMonthlyDataPoints()
        {
            // Arrange
            var startDate = DateTime.UtcNow.Date.AddMonths(-6);
            var endDate = DateTime.UtcNow.Date;

            SetupMockQueryable(_paymentRepositoryMock, new List<PaymentEntity>());
            SetupMockQueryable(_transferRepositoryMock, new List<TransferEntity>());
            SetupMockQueryable(_cityTourRepositoryMock, new List<CityTourEntity>());
            SetupMockQueryable(_yachtTourRepositoryMock, new List<YachtTourEntity>());
            SetupMockQueryable(_supplierCostRepositoryMock, new List<SupplierCost>());

            // Act
            var result = await _analyticsService.GetRevenueTrendAsync("monthly", startDate, endDate, false);

            // Assert
            result.Should().NotBeNull();
            result.Period.Should().Be("monthly");
            result.DataPoints.Should().NotBeNull();
        }

        // Helper method to setup mock queryable
        // Note: This is a simplified mock. In real scenarios, you might need to use
        // Microsoft.EntityFrameworkCore.InMemory or a more sophisticated mocking approach
        // IMPORTANT: GetAll() has optional parameters which can't be used in expression trees
        // So we only mock the parameterless version and return a queryable that can be filtered
        private void SetupMockQueryable<T>(Mock<IRepository<T>> repositoryMock, List<T> data) where T : GuestFlow.Domain.Entities.Core.BaseEntity
        {
            var mockQueryable = data.BuildMockQueryable();
            
            repositoryMock.Setup(r => r.GetAll(It.Is<System.Linq.Expressions.Expression<Func<T, bool>>>(x => x == null), It.IsAny<bool>()))
                .Returns(mockQueryable.Object);
            
            repositoryMock.Setup(r => r.GetAll(It.IsAny<System.Linq.Expressions.Expression<Func<T, bool>>>(), It.IsAny<bool>()))
                .Returns<System.Linq.Expressions.Expression<Func<T, bool>>, bool>((predicate, includeDeleted) =>
                {
                    if (predicate == null)
                        return mockQueryable.Object;
                    
                    return data.Where(predicate.Compile()).BuildMockQueryable().Object;
                });

            repositoryMock.Setup(r => r.GetAll(It.IsAny<System.Linq.Expressions.Expression<Func<T, bool>>>(), It.IsAny<System.Linq.Expressions.Expression<Func<T, object>>[]>()))
                .Returns<System.Linq.Expressions.Expression<Func<T, bool>>, System.Linq.Expressions.Expression<Func<T, object>>[]>((predicate, includes) =>
                {
                    if (predicate == null)
                        return mockQueryable.Object;
                    
                    return data.Where(predicate.Compile()).BuildMockQueryable().Object;
                });
        }
    }
}
