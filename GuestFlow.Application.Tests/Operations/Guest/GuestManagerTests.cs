using FluentAssertions;
using GuestFlow.Application.Operations.Guest;
using GuestFlow.Application.Operations.Guest.Dtos;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Guest;

/// <summary>
/// Unit tests for GuestManager
/// </summary>
public class GuestManagerTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
    private readonly Mock<IRepository<TransferEntity>> _transferRepositoryMock;
    private readonly Mock<IRepository<CityTourEntity>> _cityTourRepositoryMock;
    private readonly Mock<IRepository<YachtTourEntity>> _yachtTourRepositoryMock;
    private readonly Mock<IRepository<InvoicesEntity>> _invoiceRepositoryMock;
    private readonly Mock<ILogger<GuestManager>> _loggerMock;
    private readonly Mock<AutoMapper.IMapper> _mapperMock;
    private readonly GuestManager _guestManager;

    public GuestManagerTests()
    {
        _unitOfWorkMock = CreateMock<IUnitOfWork>();
        _guestRepositoryMock = CreateMock<IRepository<GuestEntity>>();
        _transferRepositoryMock = CreateMock<IRepository<TransferEntity>>();
        _cityTourRepositoryMock = CreateMock<IRepository<CityTourEntity>>();
        _yachtTourRepositoryMock = CreateMock<IRepository<YachtTourEntity>>();
        _invoiceRepositoryMock = CreateMock<IRepository<InvoicesEntity>>();
        _loggerMock = CreateMock<ILogger<GuestManager>>();
        _mapperMock = CreateMock<AutoMapper.IMapper>();
        _guestManager = new GuestManager(
            _unitOfWorkMock.Object,
            _guestRepositoryMock.Object,
            _transferRepositoryMock.Object,
            _cityTourRepositoryMock.Object,
            _yachtTourRepositoryMock.Object,
            _invoiceRepositoryMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task GetGuestById_WithValidId_ReturnsGuestDto()
    {
        // Arrange
        var guestId = 1;
        var expectedGuest = TestDataBuilder.CreateGuest(id: guestId);
        var expectedDto = new GetGuestDto
        {
            Id = expectedGuest.Id,
            FullName = expectedGuest.FullName,
            Email = expectedGuest.Email,
            PhoneNumber = expectedGuest.PhoneNumber,
            Nationality = expectedGuest.Nationality,
            GuestCode = expectedGuest.GuestCode,
            IsSpecialGuest = expectedGuest.IsSpecialGuest
        };

        _guestRepositoryMock
            .Setup(x => x.GetByIdAsync(guestId, false))
            .ReturnsAsync(expectedGuest);

        _mapperMock
            .Setup(m => m.Map<GetGuestDto>(expectedGuest))
            .Returns(expectedDto);

        // Act
        var result = await _guestManager.GetGuestById(guestId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(guestId);
        result.FullName.Should().Be(expectedGuest.FullName);
        result.Email.Should().Be(expectedGuest.Email);
    }

    [Fact]
    public async Task GetGuestById_WithInvalidId_ThrowsException()
    {
        // Arrange
        var guestId = 999;

        _guestRepositoryMock
            .Setup(x => x.GetByIdAsync(guestId, false))
            .ReturnsAsync((GuestEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _guestManager.GetGuestById(guestId));
    }

    [Fact]
    public async Task GetGuestById_WithDeletedGuest_ThrowsException()
    {
        // Arrange
        var guestId = 1;

        _guestRepositoryMock
            .Setup(x => x.GetByIdAsync(guestId, false))
            .ReturnsAsync((GuestEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(async () => await _guestManager.GetGuestById(guestId));
    }
}

