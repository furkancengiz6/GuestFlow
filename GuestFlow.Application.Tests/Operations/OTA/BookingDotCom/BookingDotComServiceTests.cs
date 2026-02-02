using System.Text.Json;
using GuestFlow.Application.Operations.OTA.BookingDotCom;
using GuestFlow.Application.Operations.OTA.BookingDotCom.Dtos;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.OTA.BookingDotCom
{
    public class BookingDotComServiceTests
    {
        private readonly BookingDotComService _service;
        private readonly Mock<ILogger<BookingDotComService>> _loggerMock;

        public BookingDotComServiceTests()
        {
            _loggerMock = new Mock<ILogger<BookingDotComService>>();
            _service = new BookingDotComService(_loggerMock.Object);
        }

        [Fact]
        public void ValidateSignature_ValidSignature_ReturnsTrue()
        {
            // Arrange
            var payload = "{\"test\": \"payload\"}";
            var secret = "test_secret";
            // HMACSHA256 of "{"test": "payload"}" with key "test_secret"
            // Calculated using online tool or valid implementation reference
            var expectedSignature = "33f025539ab81308a8a47466547669d66144569ee62820556da8514930be84e0";

            // Act
            var result = _service.ValidateSignature(payload, expectedSignature, secret);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ValidateSignature_InvalidSignature_ReturnsFalse()
        {
            // Arrange
            var payload = "{\"test\": \"payload\"}";
            var secret = "test_secret";
            var invalidSignature = "invalid_signature";

            // Act
            var result = _service.ValidateSignature(payload, invalidSignature, secret);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ParsePayload_ValidJson_ReturnsDto()
        {
            // Arrange
            var json = @"
            {
                ""event"": ""reservation_creation"",
                ""reservation"": {
                    ""id"": 12345,
                    ""status"": ""new""
                }
            }";

            // Act
            var result = _service.ParsePayload(json);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Reservation);
            Assert.Equal(12345, result.Reservation.Id);
        }
    }
}
