using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.Intelligence.Graph;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Persistence.Context;
using GuestFlow.Application.Tests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GuestFlow.Persistence.MultiTenancy;
using GuestFlow.Domain.Events;
using Moq;
using Xunit;
using MockQueryable.Moq;

namespace GuestFlow.Application.Tests.Operations.Intelligence.Relationship
{
    public class RelationshipIntelligenceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<GuestFlowDbContext> _mockContext;
        private readonly Mock<ITenantProvider> _mockTenantProvider;
        private readonly Mock<IDomainEventDispatcher> _mockDispatcher;
        private readonly Mock<IGraphDataService> _mockGraphDataService;
        private readonly Mock<IBehavioralTrackingService> _mockBehavioralTrackingService;
        private readonly Mock<IAIAssistantService> _mockAiAssistantService;
        private readonly Mock<ILogger<RelationshipIntelligenceService>> _mockLogger;
        private readonly RelationshipIntelligenceService _service;

        public RelationshipIntelligenceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockTenantProvider = new Mock<ITenantProvider>();
            _mockDispatcher = new Mock<IDomainEventDispatcher>();
            _mockContext = new Mock<GuestFlowDbContext>(
                new DbContextOptions<GuestFlowDbContext>(),
                _mockTenantProvider.Object,
                _mockDispatcher.Object);
            _mockGraphDataService = new Mock<IGraphDataService>();
            _mockBehavioralTrackingService = new Mock<IBehavioralTrackingService>();
            _mockAiAssistantService = new Mock<IAIAssistantService>();
            _mockLogger = new Mock<ILogger<RelationshipIntelligenceService>>();

            _service = new RelationshipIntelligenceService(
                _mockUnitOfWork.Object,
                _mockContext.Object,
                _mockGraphDataService.Object,
                _mockBehavioralTrackingService.Object,
                _mockAiAssistantService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task FindBestStaffMatchesAsync_ShouldReturnMatchesFromInteractions_WhenAINotAvailable()
        {
            // Arrange
            int guestId = 1;
            var staff = new PersonnelEntity { Id = 10, FullName = "John Doe", Department = "Concierge" };
            var interactions = new List<GuestStaffInteractionEntity>
            {
                new GuestStaffInteractionEntity 
                { 
                    GuestId = guestId, 
                    StaffId = staff.Id, 
                    Staff = staff,
                    SatisfactionScore = 9,
                    SentimentScore = 0.8
                }
            };

            var mockInteractionsRepo = interactions.BuildMockQueryable().Object;
            _mockUnitOfWork.Setup(u => u.GuestStaffInteractions.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<GuestStaffInteractionEntity, bool>>>(),
                It.IsAny<bool>()))
                .Returns(mockInteractionsRepo);

            _mockAiAssistantService.Setup(a => a.ProcessMessageAsync(It.IsAny<GuestFlow.Application.Models.AI.AIChatRequest>()))
                .ReturnsAsync(new GuestFlow.Application.Models.AI.AIChatResponse { Response = "No matches found" });

            // Act
            var result = await _service.FindBestStaffMatchesAsync(guestId);

            // Assert
            Assert.NotEmpty(result);
            var match = result.First();
            Assert.Equal(staff.Id, match.StaffId);
            Assert.Equal(staff.FullName, match.StaffName);
            Assert.True(match.CompatibilityScore > 0.5);
        }

        [Fact]
        public async Task GetGuestPreferencePatternsAsync_ShouldAggregateBehaviors()
        {
            // Arrange
            int guestId = 1;
            var behaviors = new List<GuestBehaviorEntity>
            {
                new GuestBehaviorEntity { GuestId = guestId, BehaviorType = "Preference", Category = "Service", BehaviorValue = "Late Check-out" },
                new GuestBehaviorEntity { GuestId = guestId, BehaviorType = "Preference", Category = "Service", BehaviorValue = "Late Check-out" },
                new GuestBehaviorEntity { GuestId = guestId, BehaviorType = "Preference", Category = "Room", BehaviorValue = "High Floor" }
            };

            var mockBehaviorsRepo = behaviors.BuildMockQueryable().Object;
            _mockUnitOfWork.Setup(u => u.GuestBehaviors.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<GuestBehaviorEntity, bool>>>(),
                It.IsAny<bool>()))
                .Returns(mockBehaviorsRepo);

            var preferences = new List<GuestPreferencesEntity>();
            var mockPrefsRepo = preferences.BuildMockQueryable().Object;
            _mockUnitOfWork.Setup(u => u.GuestPreferences.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<GuestPreferencesEntity, bool>>>(),
                It.IsAny<bool>()))
                .Returns(mockPrefsRepo);

            // Act
            var result = await _service.GetGuestPreferencePatternsAsync(guestId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ContainsKey("ServicePreferences"));
            var servicePrefs = result["ServicePreferences"] as Dictionary<string, int>;
            Assert.Equal(2, servicePrefs["Late Check-out"]);
        }
    }
}
