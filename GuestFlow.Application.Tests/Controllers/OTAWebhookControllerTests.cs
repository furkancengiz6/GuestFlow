using System.Text.Json;
using System.Threading.Tasks;
using GuestFlow.Api.Controllers;
using GuestFlow.Application.Models.Responses;
using GuestFlow.Application.Operations.OTA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Controllers
{
    public class OTAWebhookControllerTests
    {
        private readonly Mock<IOTAIntegrationService> _otaServiceMock;
        private readonly Mock<ILogger<OTAWebhookController>> _loggerMock;
        private readonly OTAWebhookController _controller;

        public OTAWebhookControllerTests()
        {
            _otaServiceMock = new Mock<IOTAIntegrationService>();
            _loggerMock = new Mock<ILogger<OTAWebhookController>>();
            _controller = new OTAWebhookController(_otaServiceMock.Object, _loggerMock.Object);
            
            // Setup ControllerContext for HttpContext simulation
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task HandleBookingWebhook_ValidRequest_ReturnsOk()
        {
            // Arrange
            var payload = "{}"; // minimal valid json for test
            var signature = "validsignature";
            _controller.HttpContext.Request.Headers["X-Booking-Signature"] = signature;
            
            _otaServiceMock.Setup(x => x.ProcessWebhookAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.HandleBookingWebhook(JsonDocument.Parse("{}").RootElement);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<ApiResponse<bool>>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task HandleBookingWebhook_ServiceFailure_ReturnsBadRequestOrError()
        {
             // Arrange
            _controller.HttpContext.Request.Headers["X-Booking-Signature"] = "sig";
            
            _otaServiceMock.Setup(x => x.ProcessWebhookAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()))
                .ReturnsAsync(ApiResponse<bool>.Fail("Processing failed"));

            // Act
            var result = await _controller.HandleBookingWebhook(JsonDocument.Parse("{}").RootElement);

            // Assert
            var objectResult = Assert.IsType<BadRequestObjectResult>(result); // Or whatever the controller returns on failure
            // Note: need to check controller impl if it returns BadRequest on failure
        }
    }
}
