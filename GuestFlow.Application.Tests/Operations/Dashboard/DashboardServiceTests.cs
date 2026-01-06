using FluentAssertions;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Dashboard;

/// <summary>
/// Unit tests for DashboardService - focusing on performance optimizations from Phase 2
/// </summary>
public class DashboardServiceTests : TestBase
{
    private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
    private readonly Mock<IRepository<PersonnelEntity>> _personnelRepositoryMock;
    private readonly Mock<IRepository<CityEntity>> _cityRepositoryMock;
    private readonly Mock<IRepository<VehicleEntity>> _vehicleRepositoryMock;
    private readonly Mock<IRepository<CityTourEntity>> _cityTourRepositoryMock;
    private readonly Mock<IRepository<YachtTourEntity>> _yachtTourRepositoryMock;
    private readonly Mock<IRepository<TransferEntity>> _transferRepositoryMock;
    private readonly Mock<IRepository<InvoicesEntity>> _invoiceRepositoryMock;
    private readonly Mock<IRepository<PaymentEntity>> _paymentRepositoryMock;
    private readonly Mock<IPaymentStatusService> _paymentStatusServiceMock;
    private readonly Mock<ILogger<DashboardService>> _loggerMock;
    private readonly DashboardService _dashboardService;

    public DashboardServiceTests()
    {
        _guestRepositoryMock = CreateMock<IRepository<GuestEntity>>();
        _personnelRepositoryMock = CreateMock<IRepository<PersonnelEntity>>();
        _cityRepositoryMock = CreateMock<IRepository<CityEntity>>();
        _vehicleRepositoryMock = CreateMock<IRepository<VehicleEntity>>();
        _cityTourRepositoryMock = CreateMock<IRepository<CityTourEntity>>();
        _yachtTourRepositoryMock = CreateMock<IRepository<YachtTourEntity>>();
        _transferRepositoryMock = CreateMock<IRepository<TransferEntity>>();
        _invoiceRepositoryMock = CreateMock<IRepository<InvoicesEntity>>();
        _paymentRepositoryMock = CreateMock<IRepository<PaymentEntity>>();
        _paymentStatusServiceMock = CreateMock<IPaymentStatusService>();
        _loggerMock = CreateMock<ILogger<DashboardService>>();

        _dashboardService = new DashboardService(
            _guestRepositoryMock.Object,
            _personnelRepositoryMock.Object,
            _cityRepositoryMock.Object,
            _vehicleRepositoryMock.Object,
            _cityTourRepositoryMock.Object,
            _yachtTourRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _paymentStatusServiceMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldReturnCorrectStats_WhenDataExists()
    {
        // Arrange
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // Setup count mocks (parallel execution in Phase 2 optimization)
        _guestRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(100);

        _personnelRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(15);

        _transferRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);

        _cityTourRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(30);

        _yachtTourRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(20);

        _invoiceRepositoryMock.Setup(r => r.GetAll().CountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(75);

        // Setup active guests calculation (optimized in Phase 2)
        _transferRepositoryMock.Setup(r => r.GetAll()
            .Where(t => t.TransferDate >= thirtyDaysAgo)
            .Select(t => t.GuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(25);

        _cityTourRepositoryMock.Setup(r => r.GetAll()
            .Where(ct => ct.TourDate >= thirtyDaysAgo)
            .Select(ct => ct.OwnerGuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(20);

        _yachtTourRepositoryMock.Setup(r => r.GetAll()
            .Where(yt => yt.TourDate >= thirtyDaysAgo)
            .Select(yt => yt.OwnerGuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(15);

        // Setup revenue calculation
        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, default))
            .ReturnsAsync(15000m);

        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalGuests.Should().Be(100);
        result.TotalPersonnel.Should().Be(15);
        result.TotalTransfers.Should().Be(50);
        result.TotalCityTours.Should().Be(30);
        result.TotalYachtTours.Should().Be(20);
        result.TotalInvoices.Should().Be(75);
        result.ActiveGuests.Should().Be(25 + 20 + 15); // Union of all active guests
        result.TotalRevenue.Should().Be(15000m);
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldHandleEmptyData()
    {
        // Arrange
        SetupEmptyRepositories();

        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalGuests.Should().Be(0);
        result.TotalPersonnel.Should().Be(0);
        result.TotalTransfers.Should().Be(0);
        result.TotalCityTours.Should().Be(0);
        result.TotalYachtTours.Should().Be(0);
        result.TotalInvoices.Should().Be(0);
        result.ActiveGuests.Should().Be(0);
        result.TotalRevenue.Should().Be(0);
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldExecuteQueriesInParallel_ForPerformance()
    {
        // Arrange
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // Setup mocks with verification
        _guestRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(10);

        _personnelRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(5);

        _transferRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(8);

        _cityTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(6);

        _yachtTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(4);

        _invoiceRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(12);

        // Active guests queries
        _transferRepositoryMock.Setup(r => r.GetAll()
            .Where(t => t.TransferDate >= thirtyDaysAgo)
            .Select(t => t.GuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(3);

        _cityTourRepositoryMock.Setup(r => r.GetAll()
            .Where(ct => ct.TourDate >= thirtyDaysAgo)
            .Select(ct => ct.OwnerGuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(2);

        _yachtTourRepositoryMock.Setup(r => r.GetAll()
            .Where(yt => yt.TourDate >= thirtyDaysAgo)
            .Select(yt => yt.OwnerGuestId)
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(1);

        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, default))
            .ReturnsAsync(5000m);

        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalGuests.Should().Be(10);
        result.TotalPersonnel.Should().Be(5);
        result.TotalRevenue.Should().Be(5000m);

        // Verify that all repository methods were called (ensuring parallel execution)
        _guestRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
        _personnelRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
        _transferRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
        _cityTourRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
        _yachtTourRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
        _invoiceRepositoryMock.Verify(r => r.GetAll().CountAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetPopularServicesAsync_ShouldReturnCorrectServiceStats()
    {
        // Arrange
        _transferRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(50);

        _cityTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(30);

        _yachtTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ReturnsAsync(20);

        // Revenue calculations (parallel execution)
        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(p => p.TransferId.HasValue && p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, default))
            .ReturnsAsync(10000m);

        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(p => p.CityTourId.HasValue && p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, default))
            .ReturnsAsync(6000m);

        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(p => p.YachtTourId.HasValue && p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount, default))
            .ReturnsAsync(8000m);

        // Average prices
        _transferRepositoryMock.Setup(r => r.GetAll()
            .AverageAsync(t => (decimal?)t.FinalPrice, default))
            .ReturnsAsync(500m);

        _cityTourRepositoryMock.Setup(r => r.GetAll()
            .AverageAsync(ct => (decimal?)ct.FinalPrice, default))
            .ReturnsAsync(300m);

        _yachtTourRepositoryMock.Setup(r => r.GetAll()
            .AverageAsync(yt => (decimal?)yt.FinalPrice, default))
            .ReturnsAsync(1000m);

        // Act
        var result = await _dashboardService.GetPopularServicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        var transferService = result.First(s => s.ServiceType == "Transfer");
        transferService.BookingCount.Should().Be(50);
        transferService.TotalRevenue.Should().Be(10000m);
        transferService.AveragePrice.Should().Be(500m);

        var cityTourService = result.First(s => s.ServiceType == "CityTour");
        cityTourService.BookingCount.Should().Be(30);
        cityTourService.TotalRevenue.Should().Be(6000m);
        cityTourService.AveragePrice.Should().Be(300m);

        var yachtTourService = result.First(s => s.ServiceType == "YachtTour");
        yachtTourService.BookingCount.Should().Be(20);
        yachtTourService.TotalRevenue.Should().Be(8000m);
        yachtTourService.AveragePrice.Should().Be(1000m);
    }

    [Fact]
    public async Task GetQuickStatsAsync_ShouldLogErrors_WhenExceptionOccurs()
    {
        // Arrange
        _guestRepositoryMock.Setup(r => r.GetAll().CountAsync(default))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _dashboardService.GetQuickStatsAsync());

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Hızlı istatistikler getirilirken hata")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private void SetupEmptyRepositories()
    {
        _guestRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);
        _personnelRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);
        _transferRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);
        _cityTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);
        _yachtTourRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);
        _invoiceRepositoryMock.Setup(r => r.GetAll().CountAsync(default)).ReturnsAsync(0);

        _transferRepositoryMock.Setup(r => r.GetAll()
            .Where(It.IsAny<Expression<Func<TransferEntity, bool>>>())
            .Select(It.IsAny<Expression<Func<TransferEntity, int>>>())
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(0);

        _cityTourRepositoryMock.Setup(r => r.GetAll()
            .Where(It.IsAny<Expression<Func<CityTourEntity, bool>>>())
            .Select(It.IsAny<Expression<Func<CityTourEntity, int>>>())
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(0);

        _yachtTourRepositoryMock.Setup(r => r.GetAll()
            .Where(It.IsAny<Expression<Func<YachtTourEntity, bool>>>())
            .Select(It.IsAny<Expression<Func<YachtTourEntity, int>>>())
            .Select(It.IsAny<Expression<Func<YachtTourEntity, int>>>())
            .Distinct()
            .CountAsync(default))
            .ReturnsAsync(0);

        _paymentRepositoryMock.Setup(r => r.GetAll()
            .Where(It.IsAny<Expression<Func<PaymentEntity, bool>>>())
            .SumAsync(It.IsAny<Expression<Func<PaymentEntity, decimal?>>>(), default))
            .ReturnsAsync(0m);
    }
}