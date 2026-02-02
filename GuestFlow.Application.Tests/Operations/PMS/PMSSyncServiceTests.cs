using Xunit;
using Moq;
using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Application.Models.Responses.PMS;
using GuestFlow.Application.Models.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MockQueryable.Moq;
using GuestFlow.Domain.Entities.Enum;

namespace GuestFlow.Application.Tests.Operations.PMS
{
    public class PMSSyncServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IPMSIntegrationService> _mockPmsIntegrationService;
        private readonly Mock<IRepository<GuestEntity>> _mockGuestRepository;
        private readonly Mock<IRepository<RoomAssignmentEntity>> _mockRoomAssignmentRepository;
        private readonly Mock<IRepository<InvoicesEntity>> _mockInvoiceRepository;
        private readonly Mock<IRepository<InvoiceItemEntity>> _mockInvoiceItemRepository;
        private readonly Mock<ILogger<PMSSyncService>> _mockLogger;

        private readonly Mock<IRepository<PMSIntegration>> _mockIntegrationRepository;
        private readonly Mock<IRepository<PMSGuestMapping>> _mockGuestMappingRepository;
        private readonly Mock<IRepository<PMSReservationMapping>> _mockReservationMappingRepository;
        private readonly Mock<IRepository<GuestPreferencesEntity>> _mockGuestPreferencesRepository;
        private readonly Mock<IRepository<PMSSyncHistory>> _mockSyncHistoryRepository;

        private readonly PMSSyncService _service;

        public PMSSyncServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockPmsIntegrationService = new Mock<IPMSIntegrationService>();
            _mockGuestRepository = new Mock<IRepository<GuestEntity>>();
            _mockRoomAssignmentRepository = new Mock<IRepository<RoomAssignmentEntity>>();
            _mockInvoiceRepository = new Mock<IRepository<InvoicesEntity>>();
            _mockInvoiceItemRepository = new Mock<IRepository<InvoiceItemEntity>>();
            _mockLogger = new Mock<ILogger<PMSSyncService>>();

            // Setup UoW Repositories
            _mockIntegrationRepository = new Mock<IRepository<PMSIntegration>>();
            _mockGuestMappingRepository = new Mock<IRepository<PMSGuestMapping>>();
            _mockReservationMappingRepository = new Mock<IRepository<PMSReservationMapping>>();
            _mockGuestPreferencesRepository = new Mock<IRepository<GuestPreferencesEntity>>();
            _mockSyncHistoryRepository = new Mock<IRepository<PMSSyncHistory>>();

            _mockUnitOfWork.Setup(u => u.PMSIntegrations).Returns(_mockIntegrationRepository.Object);
            _mockUnitOfWork.Setup(u => u.PMSGuestMappings).Returns(_mockGuestMappingRepository.Object);
            _mockUnitOfWork.Setup(u => u.PMSReservationMappings).Returns(_mockReservationMappingRepository.Object);
            _mockUnitOfWork.Setup(u => u.GuestPreferences).Returns(_mockGuestPreferencesRepository.Object);
            _mockUnitOfWork.Setup(u => u.PMSSyncHistories).Returns(_mockSyncHistoryRepository.Object);

            _service = new PMSSyncService(
                _mockUnitOfWork.Object,
                _mockPmsIntegrationService.Object,
                _mockGuestRepository.Object,
                _mockRoomAssignmentRepository.Object,
                _mockInvoiceRepository.Object,
                _mockInvoiceItemRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SyncGuestAsync_Should_CreateNewGuest_When_NotExists()
        {
            // Arrange
            int integrationId = 1;
            var pmsGuest = new PMSGuestProfile
            {
                PMSGuestId = "G123",
                FullName = "New Guest",
                Email = "new@example.com",
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1)
            };

            // Existing Mapping: Empty
            _mockGuestMappingRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<PMSGuestMapping, bool>>>()))
                .Returns(new List<PMSGuestMapping>().AsQueryable().BuildMock());
            
            // Existing Guest Preferences: Empty
            _mockGuestPreferencesRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<GuestPreferencesEntity, bool>>>()))
                .Returns(new List<GuestPreferencesEntity>().AsQueryable().BuildMock());

            // Guest Repository (for code generation): Empty
            _mockGuestRepository.Setup(r => r.GetAll(null))
                .Returns(new List<GuestEntity>().AsQueryable().BuildMock());

            // Act
            var result = await _service.SyncGuestAsync(integrationId, pmsGuest);

            // Assert
            Assert.True(result.Success);
            
            // Verify Guest Added
            _mockGuestRepository.Verify(r => r.AddAsync(It.Is<GuestEntity>(g => 
                g.FullName == pmsGuest.FullName && 
                g.Email == pmsGuest.Email)), Times.Once);

            // Verify Mapping Added
            _mockGuestMappingRepository.Verify(r => r.AddAsync(It.Is<PMSGuestMapping>(m => 
                m.PMSIntegrationId == integrationId && 
                m.PMSGuestId == pmsGuest.PMSGuestId)), Times.Once);

            // Verify Commit
            _mockUnitOfWork.Verify(u => u.CommitAsync(default), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SyncGuestAsync_Should_UpdateGuest_When_Exists()
        {
            // Arrange
            int integrationId = 1;
            var pmsGuest = new PMSGuestProfile
            {
                PMSGuestId = "G123",
                FullName = "Updated Guest",
                Email = "updated@example.com"
            };

            var existingGuestId = 10;
            var existingMapping = new PMSGuestMapping { PMSIntegrationId = integrationId, PMSGuestId = "G123", GuestFlowGuestId = existingGuestId };
            var existingGuest = new GuestEntity { Id = existingGuestId, FullName = "Old Name", Email = "old@example.com" };

            // Existing Mapping: Returns one
            _mockGuestMappingRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<PMSGuestMapping, bool>>>()))
                .Returns(new List<PMSGuestMapping> { existingMapping }.AsQueryable().BuildMock());
            
            // Guest Repository GetById: Returns guest
            _mockGuestRepository.Setup(r => r.GetByIdAsync(existingGuestId)).ReturnsAsync(existingGuest);

            // Guest Preferences
             _mockGuestPreferencesRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<GuestPreferencesEntity, bool>>>()))
                .Returns(new List<GuestPreferencesEntity>().AsQueryable().BuildMock());

            // Act
            var result = await _service.SyncGuestAsync(integrationId, pmsGuest);

            // Assert
            Assert.True(result.Success);
            
            // Verify Update called
            _mockGuestRepository.Verify(r => r.Update(It.Is<GuestEntity>(g => 
                g.Id == existingGuestId &&
                g.FullName == pmsGuest.FullName && 
                g.Email == pmsGuest.Email)), Times.Once);

            // Verify No Add
            _mockGuestRepository.Verify(r => r.AddAsync(It.IsAny<GuestEntity>()), Times.Never);
        }

        [Fact]
        public async Task SyncReservationAsync_Should_Sync_When_GuestExists()
        {
            // Arrange
            int integrationId = 1;
            var pmsReservation = new PMSReservation
            {
                PMSReservationId = "R999",
                PMSGuestId = "G123",
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(3),
                Status = "Confirmed",
                RoomNumber = "101"
            };

            var existingGuestId = 10;
            var guestMapping = new PMSGuestMapping { PMSIntegrationId = integrationId, PMSGuestId = "G123", GuestFlowGuestId = existingGuestId };
            var guestEntity = new GuestEntity { Id = existingGuestId, FullName = "Test Guest" };
            var integration = new PMSIntegration { Id = integrationId, ProviderName = "TestPMS" };

            // Mocks
            _mockReservationMappingRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<PMSReservationMapping, bool>>>()))
                .Returns(new List<PMSReservationMapping>().AsQueryable().BuildMock());

            _mockGuestMappingRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<PMSGuestMapping, bool>>>()))
                .Returns(new List<PMSGuestMapping> { guestMapping }.AsQueryable().BuildMock());

            _mockGuestRepository.Setup(r => r.GetByIdAsync(existingGuestId)).ReturnsAsync(guestEntity);
            _mockIntegrationRepository.Setup(r => r.GetByIdAsync(integrationId)).ReturnsAsync(integration);
            
            // Room Assignment Query (Return null to create new assignment)
            _mockRoomAssignmentRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<RoomAssignmentEntity, bool>>>()))
                .Returns(new List<RoomAssignmentEntity>().AsQueryable().BuildMock());

            // Act
            var result = await _service.SyncReservationAsync(integrationId, pmsReservation);

            // Assert
            Assert.True(result.Success);

            // Verify Guest Updated
            _mockGuestRepository.Verify(r => r.Update(It.Is<GuestEntity>(g => 
                g.CheckInDate == pmsReservation.CheckInDate &&
                g.CheckOutDate == pmsReservation.CheckOutDate)), Times.Once);

            // Verify Mapping Added
            _mockReservationMappingRepository.Verify(r => r.AddAsync(It.Is<PMSReservationMapping>(m => 
                m.PMSReservationId == pmsReservation.PMSReservationId)), Times.Once);

            // Verify Room Assignment Added (CONFIRMED status)
            _mockRoomAssignmentRepository.Verify(r => r.AddAsync(It.Is<RoomAssignmentEntity>(ra => 
                ra.GuestId == existingGuestId &&
                ra.RoomNumber == pmsReservation.RoomNumber)), Times.Once);
        }
    }
}
