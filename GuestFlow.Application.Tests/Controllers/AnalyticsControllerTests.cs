using System;
using System.Threading.Tasks;
using GuestFlow.Api.Controllers;
using GuestFlow.Application.Operations.Intelligence.Predictive;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Controllers
{
    public class AnalyticsControllerTests
    {
        private readonly Mock<IPredictiveAnalyticsService> _analyticsServiceMock;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _analyticsServiceMock = new Mock<IPredictiveAnalyticsService>();
            _controller = new AnalyticsController(_analyticsServiceMock.Object);
        }

        [Fact]
        public async Task PredictOccupancy_ValidDateRange_ReturnsOkResult()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            _analyticsServiceMock.Setup(x => x.PredictOccupancyAsync(startDate, endDate))
                .ReturnsAsync(new System.Collections.Generic.List<OccupancyForecastDto>());

            // Act
            var result = await _controller.PredictOccupancy(startDate, endDate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task PredictOccupancy_EndDateBeforeStartDate_ReturnsBadRequest()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(-1);

            // Act
            var result = await _controller.PredictOccupancy(startDate, endDate);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PredictRevenue_ValidDateRange_ReturnsOkResult()
        {
            // Arrange
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddDays(7);
            _analyticsServiceMock.Setup(x => x.PredictRevenueAsync(startDate, endDate))
                .ReturnsAsync(new System.Collections.Generic.List<RevenueForecastDto>());

            // Act
            var result = await _controller.PredictRevenue(startDate, endDate);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
