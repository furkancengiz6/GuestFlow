using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.Finance.Pricing;
using GuestFlow.Domain.Entities.Finance;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.Extensions.Logging;
using GuestFlow.Application.Tests.Helpers;
using Moq;
using Xunit;
using System.Linq.Expressions;

namespace GuestFlow.Application.Tests.Operations.Finance.Pricing
{
    public class DynamicPricingServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<DynamicPricingService>> _loggerMock;
        private readonly Mock<IRepository<PricingRuleEntity>> _pricingRulesRepoMock;
        private readonly Mock<GuestFlow.Application.Operations.OTA.IOTAChannelManagerService> _channelManagerMock;
        private readonly DynamicPricingService _service;

        public DynamicPricingServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<DynamicPricingService>>();
            _pricingRulesRepoMock = new Mock<IRepository<PricingRuleEntity>>();
            _channelManagerMock = new Mock<GuestFlow.Application.Operations.OTA.IOTAChannelManagerService>();

            _unitOfWorkMock.Setup(u => u.PricingRules).Returns(_pricingRulesRepoMock.Object);

            _service = new DynamicPricingService(_unitOfWorkMock.Object, _loggerMock.Object, _channelManagerMock.Object);
        }

        private void SetupPricingRules(List<PricingRuleEntity> rules)
        {
            // BuildMockQueryable() creates an IQueryable that supports async operations (ToListAsync etc.)
            var mockQueryable = rules.BuildMockQueryable();
            _pricingRulesRepoMock
                .Setup(r => r.GetAll(It.IsAny<Expression<Func<PricingRuleEntity, bool>>>(), It.IsAny<bool>()))
                .Returns(mockQueryable.Object);
        }

        [Fact]
        public async Task CalculateRateAsync_NoRules_ReturnsBaseRate()
        {
            // Arrange
            SetupPricingRules(new List<PricingRuleEntity>());

            // Act
            var result = await _service.CalculateRateAsync(1, DateTime.Today, 100m);

            // Assert
            Assert.Equal(100m, result.FinalRate);
        }

        [Fact]
        public async Task CalculateRateAsync_SeasonalityRule_AppliesPercentageAdjustment()
        {
            // +20% during July
            var rule = new PricingRuleEntity
            {
                RuleName = "Summer Peak",
                RuleType = PricingRuleType.Seasonality,
                ConditionValue = 7, // July
                AdjustmentType = PriceAdjustmentType.Percentage,
                AdjustmentValue = 20, // +20%
                IsActive = true,
                Priority = 1
            };

            SetupPricingRules(new List<PricingRuleEntity> { rule });

            var julyDate = new DateTime(2024, 7, 1);

            // Act
            var result = await _service.CalculateRateAsync(1, julyDate, 100m);

            // Assert: 100 + (100 * 20 / 100) = 120
            Assert.Equal(120m, result.FinalRate);
        }

        [Fact]
        public async Task CalculateRateAsync_SeasonalityRule_DoesNotApplyOutsideSeason()
        {
            // +20% during July
            var rule = new PricingRuleEntity
            {
                RuleName = "Summer Peak",
                RuleType = PricingRuleType.Seasonality,
                ConditionValue = 7, // July
                AdjustmentType = PriceAdjustmentType.Percentage,
                AdjustmentValue = 20, // +20%
                IsActive = true,
                Priority = 1
            };

            SetupPricingRules(new List<PricingRuleEntity> { rule });

            var juneDate = new DateTime(2024, 6, 1); // June, not July

            // Act
            var result = await _service.CalculateRateAsync(1, juneDate, 100m);

            // Assert: No adjustment
            Assert.Equal(100m, result.FinalRate);
        }

        [Fact]
        public async Task CalculateRateAsync_FixedAmountAdjustment_AppliesCorrectly()
        {
            // +50 on Saturdays
            var rule = new PricingRuleEntity
            {
                RuleName = "Weekend Surcharge",
                RuleType = PricingRuleType.DayOfWeek,
                ConditionValue = (decimal)DayOfWeek.Saturday,
                AdjustmentType = PriceAdjustmentType.FixedAmount,
                AdjustmentValue = 50, // +50
                IsActive = true,
                Priority = 1
            };

            SetupPricingRules(new List<PricingRuleEntity> { rule });
            
            // Saturday date
            var saturday = new DateTime(2024, 6, 1); // June 1st 2024 is Saturday
            Assert.Equal(DayOfWeek.Saturday, saturday.DayOfWeek);

            // Act
            var result = await _service.CalculateRateAsync(1, saturday, 100m);

            // Assert: 100 + 50 = 150
            Assert.Equal(150m, result.FinalRate);
        }

        [Fact]
        public async Task CalculateRateAsync_MultipleRules_AppliesInPriorityOrder()
        {
            // Rule 1: +20% during July (Priority 1)
            var rule1 = new PricingRuleEntity
            {
                RuleName = "Summer Peak",
                RuleType = PricingRuleType.Seasonality,
                ConditionValue = 7, // July
                AdjustmentType = PriceAdjustmentType.Percentage,
                AdjustmentValue = 20, // +20%
                IsActive = true,
                Priority = 1
            };
            // Rule 2: +10% when occupancy >= 80% (Priority 2)
            var rule2 = new PricingRuleEntity
            {
                RuleName = "High Occupancy",
                RuleType = PricingRuleType.Occupancy,
                ConditionValue = 0.80m,
                AdjustmentType = PriceAdjustmentType.Percentage,
                AdjustmentValue = 10, // +10%
                IsActive = true,
                Priority = 2
            };

            SetupPricingRules(new List<PricingRuleEntity> { rule1, rule2 });

            var julyDate = new DateTime(2024, 7, 1);

            // Act
            var result = await _service.CalculateRateAsync(1, julyDate, 100m);

            // Assert: Only July rule should apply (occupancy condition not met by default mock)
            Assert.Equal(120m, result.FinalRate);
        }

        [Fact]
        public async Task CalculateRateAsync_LastMinuteRule_AppliesWhenConditionMet()
        {
            // -10% when booking is less than 3 days away
            var rule = new PricingRuleEntity
            {
                RuleName = "Last Minute Deal",
                RuleType = PricingRuleType.LastMinute,
                ConditionValue = 3, // 3 days or less
                AdjustmentType = PriceAdjustmentType.Percentage,
                AdjustmentValue = -10, // -10%
                IsActive = true,
                Priority = 1
            };
            
            SetupPricingRules(new List<PricingRuleEntity> { rule });

            var tomorrow = DateTime.UtcNow.Date.AddDays(1); // 1 day away

            // Act
            var result = await _service.CalculateRateAsync(1, tomorrow, 100m);

            // Assert: 100 - (100 * 10 / 100) = 90
            Assert.Equal(90m, result.FinalRate);
        }

        [Fact]
        public async Task PushDynamicRatesToOTAsAsync_ActiveIntegrations_PushesRates()
        {
            // Arrange
            var integration = new GuestFlow.Domain.Entities.Operations.OTAIntegration
            {
                Id = 1,
                ProviderName = "Booking.com",
                IsActive = true,
                IsDeleted = false
            };
            var integrations = new List<GuestFlow.Domain.Entities.Operations.OTAIntegration> { integration };

            // Setup Pricing Rules (empty means base rate used)
            SetupPricingRules(new List<PricingRuleEntity>());

            // Mock OTA Integrations GetAll
            var mockRepo = new Mock<IRepository<GuestFlow.Domain.Entities.Operations.OTAIntegration>>();
            var otaMock = integrations.BuildMockQueryable();
            mockRepo.Setup(r => r.GetAll(It.IsAny<Expression<Func<GuestFlow.Domain.Entities.Operations.OTAIntegration, bool>>>(), It.IsAny<bool>()))
                .Returns(otaMock.Object);

            _unitOfWorkMock.Setup(u => u.OTAIntegrations).Returns(mockRepo.Object);

            // Mock Channel Manager Success
            _channelManagerMock.Setup(cm => cm.PushRateUpdateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), "TRY"))
                .ReturnsAsync(GuestFlow.Application.ApiResponse<bool>.SuccessResponse(true));

            // Act
            await _service.PushDynamicRatesToOTAsAsync(daysAhead: 1);

            // Assert
            _channelManagerMock.Verify(cm => cm.PushRateUpdateAsync(1, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<decimal>(), "TRY"), Times.AtLeastOnce);
        }
    }
}
