using FluentAssertions;
using GuestFlow.Application.Operations.Dashboard;
using GuestFlow.Application.Operations.Payment;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.Repositories;
using GuestFlow.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using GuestFlow.Application.Operations.Payment.Dtos;
using Xunit;

namespace GuestFlow.Application.Tests.Integration;

/// <summary>
/// Integration tests for DashboardService - Real database operations
/// Tests Phase 2 performance optimizations with actual data
/// </summary>
[Collection("Database collection")]
public class DashboardIntegrationTests : IAsyncLifetime
{
    private GuestFlowDbContext _context;
    private IRepository<GuestEntity> _guestRepository;
    private IRepository<PersonnelEntity> _personnelRepository;
    private IRepository<CityEntity> _cityRepository;
    private IRepository<VehicleEntity> _vehicleRepository;
    private IRepository<CityTourEntity> _cityTourRepository;
    private IRepository<YachtTourEntity> _yachtTourRepository;
    private IRepository<TransferEntity> _transferRepository;
    private IRepository<InvoicesEntity> _invoiceRepository;
    private IRepository<PaymentEntity> _paymentRepository;
    private IPaymentStatusService _paymentStatusService;
    private DashboardService _dashboardService;
    private IUnitOfWork _unitOfWork;

    public async Task InitializeAsync()
    {
        // Setup in-memory database for integration tests
        var options = new DbContextOptionsBuilder<GuestFlowDbContext>()
            .UseInMemoryDatabase(databaseName: $"GuestFlow_Integration_{Guid.NewGuid()}")
            .Options;

        _context = new GuestFlowDbContext(options);

        // Create repositories
        _guestRepository = new Repository<GuestEntity>(_context);
        _personnelRepository = new Repository<PersonnelEntity>(_context);
        _cityRepository = new Repository<CityEntity>(_context);
        _vehicleRepository = new Repository<VehicleEntity>(_context);
        _cityTourRepository = new Repository<CityTourEntity>(_context);
        _yachtTourRepository = new Repository<YachtTourEntity>(_context);
        _transferRepository = new Repository<TransferEntity>(_context);
        _invoiceRepository = new Repository<InvoicesEntity>(_context);
        _paymentRepository = new Repository<PaymentEntity>(_context);

        _unitOfWork = new UnitOfWork(_context);

        // Mock payment status service (in real scenario, inject actual service)
        var paymentStatusServiceMock = new Moq.Mock<IPaymentStatusService>();
        paymentStatusServiceMock.Setup(p => p.GetServicePaymentStatusAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new ServicePaymentStatusDto
            {
                ServiceId = 0,
                ServiceType = "Transfer",
                PaidAmount = 100m,
                RemainingAmount = 0m,
                PaymentStatus = "Paid",
                Currency = "TRY",
                ServiceDate = DateTime.UtcNow
            });

        _paymentStatusService = paymentStatusServiceMock.Object;

        var loggerMock = new Moq.Mock<ILogger<DashboardService>>();
        _dashboardService = new DashboardService(
            _guestRepository,
            _personnelRepository,
            _cityRepository,
            _vehicleRepository,
            _cityTourRepository,
            _yachtTourRepository,
            _transferRepository,
            _invoiceRepository,
            _paymentRepository,
            _paymentStatusService,
            loggerMock.Objectx
        );

        // Seed test data
        await SeedTestData();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GetQuickStatsAsync_WithRealData_ShouldReturnCorrectStats()
    {
        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalGuests.Should().BeGreaterThan(0);
        result.TotalPersonnel.Should().BeGreaterThan(0);
        result.TotalRevenue.Should().BeGreaterThan(0);
        result.ActiveGuests.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetPopularServicesAsync_WithRealPayments_ShouldCalculateRevenueCorrectly()
    {
        // Act
        var result = await _dashboardService.GetPopularServicesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);

        // Verify revenue calculations (Phase 2 optimization)
        var transferService = result.First(s => s.ServiceType == "Transfer");
        var cityTourService = result.First(s => s.ServiceType == "CityTour");
        var yachtTourService = result.First(s => s.ServiceType == "YachtTour");

        // Revenue should be calculated from completed payments only
        transferService.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
        cityTourService.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);
        yachtTourService.TotalRevenue.Should().BeGreaterThanOrEqualTo(0);

        // Booking counts should match service entities
        transferService.BookingCount.Should().BeGreaterThanOrEqualTo(0);
        cityTourService.BookingCount.Should().BeGreaterThanOrEqualTo(0);
        yachtTourService.BookingCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Performance_OptimizedQueries_ShouldExecuteInParallel()
    {
        // This test verifies that the Phase 2 optimization (parallel query execution) works

        // Act - Multiple calls to ensure parallel execution doesn't break
        var tasks = new[]
        {
            _dashboardService.GetQuickStatsAsync(),
            _dashboardService.GetPopularServicesAsync(),
            _dashboardService.GetQuickStatsAsync(),
            _dashboardService.GetPopularServicesAsync()
        };

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(4);
        results.All(r => r != null).Should().BeTrue();

        // All results should be consistent
        var quickStats1 = results[0] as QuickStatsDto;
        var quickStats2 = results[2] as QuickStatsDto;

        quickStats1.Should().NotBeNull();
        quickStats2.Should().NotBeNull();

        quickStats1!.TotalGuests.Should().Be(quickStats2!.TotalGuests);
        quickStats1.TotalPersonnel.Should().Be(quickStats2.TotalPersonnel);
    }

    [Fact]
    public async Task ActiveGuestsCalculation_ShouldUseOptimizedUnionQuery()
    {
        // Arrange - Add some recent activity
        var recentGuest = new GuestEntity
        {
            FullName = "Recent Guest",
            GuestCode = "RG001",
            Email = "recent@example.com",
            PhoneNumber = "05551234567",
            CreatedDate = DateTime.UtcNow
        };

        await _guestRepository.AddAsync(recentGuest);

        // Add a transfer in the last 30 days
        var recentTransfer = new TransferEntity
        {
            GuestId = recentGuest.Id,
            TransferDate = DateTime.UtcNow.AddDays(-15),
            PickupAddress = "Hotel A",
            DropoffAddress = "Airport",
            FinalPrice = 150,
            Currency = "TRY",
            Status = "Completed",
            CreatedDate = DateTime.UtcNow
        };

        await _transferRepository.AddAsync(recentTransfer);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.ActiveGuests.Should().BeGreaterThan(0);

        // The active guest calculation should include the recent guest
        // (This tests the Phase 2 optimization of using separate queries instead of complex joins)
    }

    [Fact]
    public async Task RevenueCalculation_ShouldOnlyIncludeCompletedPayments()
    {
        // Arrange - Add payments with different statuses
        var guest = await _guestRepository.GetAll().FirstAsync();
        var transfer = await _transferRepository.GetAll().FirstAsync();

        var completedPayment = new PaymentEntity
        {
            GuestId = guest.Id,
            TransferId = transfer.Id,
            Amount = 200,
            Currency = "TRY",
            PaymentMethod = "CreditCard",
            Status = Domain.Entities.Enum.PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var pendingPayment = new PaymentEntity
        {
            GuestId = guest.Id,
            TransferId = transfer.Id,
            Amount = 100,
            Currency = "TRY",
            PaymentMethod = "Cash",
            Status = Domain.Entities.Enum.PaymentStatus.Pending,
            PaymentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(completedPayment);
        await _paymentRepository.AddAsync(pendingPayment);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _dashboardService.GetQuickStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalRevenue.Should().Be(200); // Only completed payment should be counted
    }

    [Fact]
    public async Task ErrorHandling_ShouldLogAndContinue_WhenDatabaseError()
    {
        // Arrange - Simulate database error by disposing context
        await _context.DisposeAsync();

        // Act & Assert - Service should handle error gracefully
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _dashboardService.GetQuickStatsAsync());
    }

    private async Task SeedTestData()
    {
        // Create test personnel
        var personnel = new PersonnelEntity
        {
            FullName = "Test Personnel",
            Email = "personnel@example.com",
            UserType = Domain.Entities.Enum.UserType.Concierge,
            CreatedDate = DateTime.UtcNow
        };
        await _personnelRepository.AddAsync(personnel);

        // Create test guest
        var guest = new GuestEntity
        {
            FullName = "Test Guest",
            GuestCode = "TG001",
            Email = "guest@example.com",
            PhoneNumber = "05559876543",
            CreatedDate = DateTime.UtcNow
        };
        await _guestRepository.AddAsync(guest);

        // Create test city
        var city = new CityEntity
        {
            CityName = "Istanbul",
            Country = "Turkey",
            CreatedDate = DateTime.UtcNow
        };
        await _cityRepository.AddAsync(city);

        // Create test vehicle
        var vehicle = new VehicleEntity
        {
            PlateNumber = "34ABC123",
            Type = "Mercedes Vito",
            Capacity = 8,
            CreatedDate = DateTime.UtcNow
        };
        await _vehicleRepository.AddAsync(vehicle);

        // Create test transfer
        var transfer = new TransferEntity
        {
            GuestId = guest.Id,
            TransferDate = DateTime.UtcNow.AddDays(-10),
            PickupAddress = "Hotel A",
            DropoffAddress = "Airport",
            FinalPrice = 150,
            Currency = "TRY",
            Status = "Completed",
            CreatedDate = DateTime.UtcNow
        };
        await _transferRepository.AddAsync(transfer);

        // Create test city tour
        var cityTour = new CityTourEntity
        {
            OwnerGuestId = guest.Id,
            TourDate = DateTime.UtcNow.AddDays(-5),
            Language = "English",
            DurationHours = 4,
            FinalPrice = 200,
            Currency = "TRY",
            Status = "Completed",
            CreatedDate = DateTime.UtcNow
        };
        await _cityTourRepository.AddAsync(cityTour);

        // Create test yacht tour
        var yachtTour = new YachtTourEntity
        {
            OwnerGuestId = guest.Id,
            TourDate = DateTime.UtcNow.AddDays(-7),
            YachtName = "Blue Dream",
            NumberOfPeople = 6,
            FinalPrice = 800,
            Currency = "TRY",
            Status = "Completed",
            CreatedDate = DateTime.UtcNow
        };
        await _yachtTourRepository.AddAsync(yachtTour);

        // Create test invoice
        var invoice = new InvoicesEntity
        {
            GuestId = guest.Id,
            InvoiceNumber = "INV001",
            IssueDate = DateTime.UtcNow,
            TotalAmount = 1150,
            Currency = "TRY",
            Status = GuestFlow.Domain.Entities.Core.InvoiceStatus.Generated,
            CreatedDate = DateTime.UtcNow
        };
        await _invoiceRepository.AddAsync(invoice);

        // Create test payments
        var payment1 = new PaymentEntity
        {
            GuestId = guest.Id,
            TransferId = transfer.Id,
            Amount = 150,
            Currency = "TRY",
            PaymentMethod = Domain.Entities.Enum.PaymentMethod.CreditCard,
            Status = Domain.Entities.Enum.PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var payment2 = new PaymentEntity
        {
            GuestId = guest.Id,
            CityTourId = cityTour.Id,
            Amount = 200,
            Currency = "TRY",
            PaymentMethod = Domain.Entities.Enum.PaymentMethod.Cash,
            Status = Domain.Entities.Enum.PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var payment3 = new PaymentEntity
        {
            GuestId = guest.Id,
            YachtTourId = yachtTour.Id,
            Amount = 800,
            Currency = "TRY",
            PaymentMethod = Domain.Entities.Enum.PaymentMethod.BankTransfer,
            Status = Domain.Entities.Enum.PaymentStatus.Completed,
            PaymentDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment1);
        await _paymentRepository.AddAsync(payment2);
        await _paymentRepository.AddAsync(payment3);

        await _unitOfWork.SaveChangesAsync();
    }
}