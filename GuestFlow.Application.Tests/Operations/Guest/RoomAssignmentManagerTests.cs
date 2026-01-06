using FluentAssertions;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Operations.Notification;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Guest;

/// <summary>
/// Unit tests for RoomAssignmentManager
/// </summary>
public class RoomAssignmentManagerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<RoomAssignmentEntity>> _roomAssignmentRepositoryMock;
    private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
    private readonly Mock<IRepository<HotelEntity>> _hotelRepositoryMock;
    private readonly Mock<IRepository<TransferEntity>> _transferRepositoryMock;
    private readonly Mock<IRepository<CityTourEntity>> _cityTourRepositoryMock;
    private readonly Mock<IRepository<YachtTourEntity>> _yachtTourRepositoryMock;
    private readonly Mock<IRepository<InvoicesEntity>> _invoiceRepositoryMock;
    private readonly Mock<IRepository<PaymentEntity>> _paymentRepositoryMock;
    private readonly Mock<ILogger<RoomAssignmentManager>> _loggerMock;
    private readonly Mock<AutoMapper.IMapper> _mapperMock;
    private readonly Mock<INotificationHubService> _hubServiceMock;
    private readonly RoomAssignmentManager _roomAssignmentManager;

    public RoomAssignmentManagerTests()
    {
        _unitOfWorkMock = CreateMock<IUnitOfWork>();
        _roomAssignmentRepositoryMock = CreateMock<IRepository<RoomAssignmentEntity>>();
        _guestRepositoryMock = CreateMock<IRepository<GuestEntity>>();
        _hotelRepositoryMock = CreateMock<IRepository<HotelEntity>>();
        _transferRepositoryMock = CreateMock<IRepository<TransferEntity>>();
        _cityTourRepositoryMock = CreateMock<IRepository<CityTourEntity>>();
        _yachtTourRepositoryMock = CreateMock<IRepository<YachtTourEntity>>();
        _invoiceRepositoryMock = CreateMock<IRepository<InvoicesEntity>>();
        _paymentRepositoryMock = CreateMock<IRepository<PaymentEntity>>();
        _loggerMock = CreateMock<ILogger<RoomAssignmentManager>>();
        _mapperMock = CreateMock<AutoMapper.IMapper>();
        _hubServiceMock = CreateMock<INotificationHubService>();

        _roomAssignmentManager = new RoomAssignmentManager(
            _unitOfWorkMock.Object,
            _roomAssignmentRepositoryMock.Object,
            _guestRepositoryMock.Object,
            _hotelRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _cityTourRepositoryMock.Object,
            _yachtTourRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _loggerMock.Object,
            _mapperMock.Object,
            _hubServiceMock.Object
        );
    }

    [Fact]
    public async Task GetCurrentRoomAssignmentAsync_WithActiveAssignment_ReturnsAssignment()
    {
        // Arrange
        var guestId = 1;
        var today = DateTime.UtcNow.Date;

        var activeAssignment = new RoomAssignmentEntity
        {
            Id = 1,
            GuestId = guestId,
            RoomNumber = "101",
            StartDate = today.AddDays(-2), // Started 2 days ago
            EndDate = today.AddDays(3),    // Ends in 3 days (future)
            IsDeleted = false
        };

        var assignments = new List<RoomAssignmentEntity> { activeAssignment };
        var mockQueryable = assignments.AsQueryable();

        _roomAssignmentRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>(), false))
            .Returns(mockQueryable);

        // Act
        var result = await _roomAssignmentManager.GetCurrentRoomAssignmentAsync(guestId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(1);
        result.Data.RoomNumber.Should().Be("101");
        result.Data.StartDate.Should().Be(activeAssignment.StartDate);
        result.Data.EndDate.Should().Be(activeAssignment.EndDate);
    }

    [Fact]
    public async Task GetCurrentRoomAssignmentAsync_WithOngoingAssignment_ReturnsAssignment()
    {
        // Arrange
        var guestId = 1;
        var today = DateTime.UtcNow.Date;

        var ongoingAssignment = new RoomAssignmentEntity
        {
            Id = 2,
            GuestId = guestId,
            RoomNumber = "102",
            StartDate = today.AddDays(-5), // Started 5 days ago
            EndDate = null,                // No end date (ongoing)
            IsDeleted = false
        };

        var assignments = new List<RoomAssignmentEntity> { ongoingAssignment };
        var mockQueryable = assignments.AsQueryable();

        _roomAssignmentRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>(), false))
            .Returns(mockQueryable);

        // Act
        var result = await _roomAssignmentManager.GetCurrentRoomAssignmentAsync(guestId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(2);
        result.Data.RoomNumber.Should().Be("102");
        result.Data.EndDate.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentRoomAssignmentAsync_WithExpiredAssignment_ReturnsFailure()
    {
        // Arrange
        var guestId = 1;
        var today = DateTime.UtcNow.Date;

        var expiredAssignment = new RoomAssignmentEntity
        {
            Id = 3,
            GuestId = guestId,
            RoomNumber = "103",
            StartDate = today.AddDays(-10), // Started 10 days ago
            EndDate = today.AddDays(-2),    // Ended 2 days ago (expired)
            IsDeleted = false
        };

        var assignments = new List<RoomAssignmentEntity> { expiredAssignment };
        var mockQueryable = assignments.AsQueryable();

        _roomAssignmentRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>(), false))
            .Returns(mockQueryable);

        // Act
        var result = await _roomAssignmentManager.GetCurrentRoomAssignmentAsync(guestId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Aktif oda ataması bulunamadı");
    }

    [Fact]
    public async Task GetCurrentRoomAssignmentAsync_WithFutureAssignment_ReturnsFailure()
    {
        // Arrange
        var guestId = 1;
        var today = DateTime.UtcNow.Date;

        var futureAssignment = new RoomAssignmentEntity
        {
            Id = 4,
            GuestId = guestId,
            RoomNumber = "104",
            StartDate = today.AddDays(2), // Starts in 2 days (future)
            EndDate = today.AddDays(7),   // Ends in 7 days
            IsDeleted = false
        };

        var assignments = new List<RoomAssignmentEntity> { futureAssignment };
        var mockQueryable = assignments.AsQueryable();

        _roomAssignmentRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>(), false))
            .Returns(mockQueryable);

        // Act
        var result = await _roomAssignmentManager.GetCurrentRoomAssignmentAsync(guestId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Aktif oda ataması bulunamadı");
    }

    [Fact]
    public async Task GetCurrentRoomAssignmentAsync_WithMultipleAssignments_ReturnsMostRecent()
    {
        // Arrange
        var guestId = 1;
        var today = DateTime.UtcNow.Date;

        var olderAssignment = new RoomAssignmentEntity
        {
            Id = 5,
            GuestId = guestId,
            RoomNumber = "105",
            StartDate = today.AddDays(-10),
            EndDate = null,
            IsDeleted = false
        };

        var newerAssignment = new RoomAssignmentEntity
        {
            Id = 6,
            GuestId = guestId,
            RoomNumber = "106",
            StartDate = today.AddDays(-2), // More recent start date
            EndDate = null,
            IsDeleted = false
        };

        var assignments = new List<RoomAssignmentEntity> { olderAssignment, newerAssignment };
        var mockQueryable = assignments.AsQueryable();

        _roomAssignmentRepositoryMock
            .Setup(x => x.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>(), false))
            .Returns(mockQueryable);

        // Act
        var result = await _roomAssignmentManager.GetCurrentRoomAssignmentAsync(guestId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Id.Should().Be(6); // Should return the newer assignment
        result.Data.RoomNumber.Should().Be("106");
    }
}
