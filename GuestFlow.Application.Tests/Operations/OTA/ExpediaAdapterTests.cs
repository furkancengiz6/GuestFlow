using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GuestFlow.Application.Operations.OTA;
using GuestFlow.Application.Operations.OTA.Expedia;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.OTA
{
    public class ExpediaAdapterTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<ExpediaAdapter>> _mockLogger;
        private readonly Mock<IExpediaService> _mockExpediaService;
        private readonly OTAIntegration _integration;
        private readonly ExpediaAdapter _adapter;

        public ExpediaAdapterTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<ExpediaAdapter>>();
            _mockExpediaService = new Mock<IExpediaService>();
            
            _integration = new OTAIntegration 
            { 
                Id = 1, 
                ProviderCode = "EXPEDIA", 
                ApiSecret = "test-secret" 
            };

            _adapter = new ExpediaAdapter(
                _integration, 
                _mockHttpClientFactory.Object, 
                _mockLogger.Object, 
                _mockExpediaService.Object
            );
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldCallValidateSignature_WhenSecretIsPresent()
        {
            // Arrange
            string payload = "{}";
            string signature = "valid-sig";

            _mockExpediaService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(true);
            
            _mockExpediaService.Setup(s => s.ParsePayload(payload))
                .Returns(new JsonElement());

            // Act
            var result = await _adapter.ProcessWebhookAsync(payload, signature);

            // Assert
            Assert.True(result);
            _mockExpediaService.Verify(s => s.ValidateSignature(payload, signature, _integration.ApiSecret), Times.Once);
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldReturnFalse_WhenSignatureIsInvalid()
        {
            // Arrange
            string payload = "{}";
            string signature = "invalid-sig";

            _mockExpediaService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(false);

            // Act
            var result = await _adapter.ProcessWebhookAsync(payload, signature);

            // Assert
            Assert.False(result);
            _mockExpediaService.Verify(s => s.ParsePayload(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessWebhookAsync_ShouldCallParsePayload_WhenValidationSucceeds()
        {
             // Arrange
            string payload = "{}";
            string signature = "valid-sig";

            _mockExpediaService.Setup(s => s.ValidateSignature(payload, signature, _integration.ApiSecret))
                .Returns(true);
            
            _mockExpediaService.Setup(s => s.ParsePayload(payload))
                .Returns(new JsonElement());

            // Act
             await _adapter.ProcessWebhookAsync(payload, signature);

             // Assert
             _mockExpediaService.Verify(s => s.ParsePayload(payload), Times.Once);
        }
    }
}
