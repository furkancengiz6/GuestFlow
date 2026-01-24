using FluentAssertions;
using GuestFlow.Application.Operations.Privacy;
using GuestFlow.Application.Tests.Helpers;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.Privacy;

public class PIIManagementServiceTests : TestBase
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IRepository<GuestEntity>> _guestRepositoryMock;
    private readonly Mock<IRepository<PrivacyActionHistoryEntity>> _privacyHistoryRepositoryMock;
    private readonly Mock<ILogger<PIIManagementService>> _loggerMock;
    private readonly PIIManagementService _piiManagementService;

    public PIIManagementServiceTests()
    {
        _unitOfWorkMock = CreateMock<IUnitOfWork>();
        _guestRepositoryMock = CreateMock<IRepository<GuestEntity>>();
        _privacyHistoryRepositoryMock = CreateMock<IRepository<PrivacyActionHistoryEntity>>();
        _loggerMock = CreateMock<ILogger<PIIManagementService>>();

        _unitOfWorkMock.Setup(u => u.Guests).Returns(_guestRepositoryMock.Object);
        _unitOfWorkMock.Setup(u => u.PrivacyActionHistories).Returns(_privacyHistoryRepositoryMock.Object);

        _piiManagementService = new PIIManagementService(
            _unitOfWorkMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public void MaskEmail_ShouldMaskEmailCorrectly()
    {
        // Arrange
        var email = "john.doe@example.com";

        // Act
        var masked = _piiManagementService.MaskEmail(email);

        // Assert
        masked.Should().NotBe(email);
        masked.Should().Contain("***");
        masked.Should().Contain("@");
        masked.Should().Contain(".com");
    }

    [Fact]
    public void MaskPhone_ShouldMaskPhoneCorrectly()
    {
        // Arrange
        var phone = "+905551234567";

        // Act
        var masked = _piiManagementService.MaskPhone(phone);

        // Assert
        masked.Should().NotBe(phone);
        masked.Should().Contain("***");
    }

    [Fact]
    public async Task AnonymizeGuestAsync_ShouldAnonymizeGuestData_WhenGuestExists()
    {
        // Arrange
        var guestId = 1;
        var reason = "GDPR right to be forgotten";
        var personnelId = 10;

        var guest = new GuestEntity
        {
            Id = guestId,
            FullName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "+905551234567",
            IsAnonymized = false
        };

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, false))
            .ReturnsAsync(guest);

        _guestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<GuestEntity>()))
            .Returns(Task.CompletedTask);

        _privacyHistoryRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrivacyActionHistoryEntity>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _piiManagementService.AnonymizeGuestAsync(guestId, reason, personnelId);

        // Assert
        result.Should().BeTrue();
        guest.IsAnonymized.Should().BeTrue();
        guest.FullName.Should().Contain("Anonymized");
        guest.Email.Should().Contain("anonymized");
        guest.PhoneNumber.Should().Contain("***");

        _guestRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<GuestEntity>()), Times.Once);
        _privacyHistoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PrivacyActionHistoryEntity>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AnonymizeGuestAsync_ShouldReturnFalse_WhenGuestNotFound()
    {
        // Arrange
        var guestId = 999;
        var reason = "Test";
        var personnelId = 10;

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, false))
            .ReturnsAsync((GuestEntity?)null);

        // Act
        var result = await _piiManagementService.AnonymizeGuestAsync(guestId, reason, personnelId);

        // Assert
        result.Should().BeFalse();
        _guestRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<GuestEntity>()), Times.Never);
    }

    [Fact]
    public async Task IsGuestAnonymizedAsync_ShouldReturnTrue_WhenGuestIsAnonymized()
    {
        // Arrange
        var guestId = 1;
        var guest = new GuestEntity { Id = guestId, IsAnonymized = true };

        _guestRepositoryMock.Setup(r => r.GetByIdAsync(guestId, false))
            .ReturnsAsync(guest);

        // Act
        var result = await _piiManagementService.IsGuestAnonymizedAsync(guestId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetPrivacyActionHistoryAsync_ShouldReturnHistory_WhenHistoryExists()
    {
        // Arrange
        var history = new List<PrivacyActionHistoryEntity>
        {
            new PrivacyActionHistoryEntity { Id = 1, GuestId = 1, ActionType = "Anonymize", Reason = "GDPR", ActionDate = DateTime.UtcNow },
            new PrivacyActionHistoryEntity { Id = 2, GuestId = 2, ActionType = "Delete", Reason = "KVKK", ActionDate = DateTime.UtcNow }
        };

        _privacyHistoryRepositoryMock.Setup(r => r.GetAll(null, false))
            .Returns(history.BuildMockQueryable().Object);

        // Act
        var result = await _piiManagementService.GetPrivacyActionHistoryAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}
