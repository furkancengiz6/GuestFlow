using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using GuestFlow.Application.Operations.Intelligence.Behavioral;
using GuestFlow.Application.Operations.Intelligence.Relationship;
using GuestFlow.Application.Operations.Intelligence.Sentiment;
using GuestFlow.Application.Operations.AI;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Domain.UnitOfWork;
using GuestFlow.Application.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MockQueryable.Moq;

namespace GuestFlow.Application.Tests.Operations.Intelligence.Predictive
{
    public class PredictiveIntelligenceServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBehavioralTrackingService> _mockBehavioralTrackingService;
        private readonly Mock<IRelationshipIntelligenceService> _mockRelationshipIntelligenceService;
        private readonly Mock<ISentimentAnalysisService> _mockSentimentAnalysisService;
        private readonly Mock<IAIAssistantService> _mockAiAssistantService;
        private readonly Mock<ILogger<PredictiveIntelligenceService>> _mockLogger;
        private readonly PredictiveIntelligenceService _service;

        public PredictiveIntelligenceServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBehavioralTrackingService = new Mock<IBehavioralTrackingService>();
            _mockRelationshipIntelligenceService = new Mock<IRelationshipIntelligenceService>();
            _mockSentimentAnalysisService = new Mock<ISentimentAnalysisService>();
            _mockAiAssistantService = new Mock<IAIAssistantService>();
            _mockLogger = new Mock<ILogger<PredictiveIntelligenceService>>();

            _service = new PredictiveIntelligenceService(
                _mockUnitOfWork.Object,
                _mockBehavioralTrackingService.Object,
                _mockRelationshipIntelligenceService.Object,
                _mockSentimentAnalysisService.Object,
                _mockAiAssistantService.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task PredictGuestBehaviorAsync_ShouldCalculateModerateLikelihood_WithFrequentActivity()
        {
            // Arrange
            int guestId = 1;
            string behaviorType = "RoomService";
            var behaviors = new List<GuestBehaviorEntity>();
            for (int i = 0; i < 5; i++)
            {
                behaviors.Add(new GuestBehaviorEntity 
                { 
                    GuestId = guestId, 
                    BehaviorType = behaviorType, 
                    BehaviorDate = DateTime.UtcNow.AddDays(-i) 
                });
            }

            var mockBehaviorsRepo = behaviors.BuildMockQueryable().Object;
            _mockUnitOfWork.Setup(u => u.GuestBehaviors.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<GuestBehaviorEntity, bool>>>(),
                It.IsAny<bool>()))
                .Returns(mockBehaviorsRepo);

            // Act
            var result = await _service.PredictGuestBehaviorAsync(guestId, behaviorType);

            // Assert
            Assert.Equal(behaviorType, result.BehaviorType);
            Assert.True(result.Probability >= 0.4);
            Assert.Contains("likelihood", result.Prediction);
        }

        [Fact]
        public async Task PredictRisksAsync_ShouldIdentifyDissatisfactionRisk_WhenSatisfactionIsLow()
        {
            // Arrange
            int guestId = 1;
            var lowSatisfactionBehaviors = new List<GuestBehaviorEntity>
            {
                new GuestBehaviorEntity { GuestId = guestId, SatisfactionScore = 2.0, BehaviorDate = DateTime.UtcNow.AddDays(-1) }
            };

            var mockBehaviorsRepo = lowSatisfactionBehaviors.BuildMockQueryable().Object;
            _mockUnitOfWork.Setup(u => u.GuestBehaviors.GetAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<GuestBehaviorEntity, bool>>>(),
                It.IsAny<bool>()))
                .Returns(mockBehaviorsRepo);

            _mockSentimentAnalysisService.Setup(s => s.GetGuestSentimentTrendsAsync(guestId, null, null))
                .ReturnsAsync(new Dictionary<string, object> { { "AverageSentiment", -0.4 } });

            // Act
            var result = await _service.PredictRisksAsync(guestId);

            // Assert
            Assert.NotEmpty(result.Risks);
            var dissatisfactionRisk = result.Risks.FirstOrDefault(r => r.RiskType == "Dissatisfaction");
            Assert.NotNull(dissatisfactionRisk);
            Assert.True(dissatisfactionRisk.RiskScore > 0.5);
        }
    }
}
