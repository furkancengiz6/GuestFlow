using System;
using System.Net.Http;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Application.Operations.OTA.BookingDotCom;
using GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.OTA
{
    public class BookingComAdapterTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<BookingComAdapter>> _mockLogger;
        private readonly Mock<IBookingDotComService> _mockBookingService;
        private readonly OTAIntegration _integration;
        private readonly BookingComAdapter _adapter;

        public BookingComAdapterTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<BookingComAdapter>>();
            _mockBookingService = new Mock<IBookingDotComService>();
            
            _integration = new OTAIntegration 
            { 
                Id = 1, 
                ProviderCode = "BOOKING", 
                ApiSecret = "test-secret" 
            };

            _adapter = new BookingComAdapter(
                _integration, 
                _mockHttpClientFactory.Object, 
                _mockLogger.Object, 
                _mockBookingService.Object
            );
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldCallValidateSignature_WhenSecretIsPresent()
        {
            // Arrange
            string payload = "{}";
            string signature = "valid-sig";

            _mockBookingService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(true);
            
            _mockBookingService.Setup(s => s.ParsePayload(payload))
                .Returns(new BookingWebhookPayloadDto());

            // Act
            var result = await _adapter.ProcessWebhookAsync(payload, signature);

            // Assert
            Assert.True(result);
            _mockBookingService.Verify(s => s.ValidateSignature(payload, signature, _integration.ApiSecret), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldReturnFalse_WhenSignatureIsInvalid()
        {
            // Arrange
            string payload = "{}";
            string signature = "invalid-sig";

            _mockBookingService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(false);

            // Act
            var result = await _adapter.ProcessWebhookAsync(payload, signature);

            // Assert
            Assert.False(result);
            _mockBookingService.Verify(s => s.ParsePayload(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldCallParsePayload_WhenValidationSucceeds()
        {
             // Arrange
            string payload = "{}";
            string signature = "valid-sig";

            _mockBookingService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(true);
            
            _mockBookingService.Setup(s => s.ParsePayload(payload))
                .Returns(new BookingWebhookPayloadDto());

            // Act
             await _adapter.ProcessWebhookAsync(payload, signature);

             // Assert
             _mockBookingService.Verify(s => s.ParsePayload(payload), Times.Once);
        }
    }
}
