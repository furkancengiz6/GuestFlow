using GuestFlow.Application.Operations.PMS;
using GuestFlow.Domain.Entities.Operations;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Xunit;

namespace GuestFlow.Application.Tests.Operations.PMS
{
    public class OperaPMSAdapterTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<ILogger<OperaPMSAdapter>> _loggerMock;
        private readonly PMSIntegration _integration;
        private readonly OperaPMSAdapter _adapter;

        public OperaPMSAdapterTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _loggerMock = new Mock<ILogger<OperaPMSAdapter>>();
            _integration = new PMSIntegration
            {
                Id = 1,
                ProviderCode = "OPERA",
                ApiEndpoint = "https://api.test-opera.com",
                ApiKey = "test-client-id",
                ApiSecret = "test-client-secret",
                IsActive = true
            };
            _adapter = new OperaPMSAdapter(_integration, _httpClientFactoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task RefreshAccessTokenAsync_ValidCredentials_ReturnsTrue()
        {
            // Arrange
            var tokenResponse = new
            {
                access_token = "new-token",
                token_type = "Bearer",
                expires_in = 3600,
                refresh_token = "new-refresh"
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(tokenResponse))
                });

            var httpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri(_integration.ApiEndpoint)
            };

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // We need a refresh token initially for it to try refreshing
            _integration.RefreshToken = "old-refresh-token";

            // Act
            var result = await _adapter.RefreshAccessTokenAsync();

            // Assert
            Assert.True(result);
            Assert.Equal("new-token", _integration.AccessToken);
        }

        [Fact]
        public async Task GetGuestProfileAsync_ValidId_ReturnsProfile()
        {
            // Arrange
            var guestResponse = new 
            {
                GuestId = "123",
                FullName = "John Doe",
                Email = "john@example.com",
                IsVIP = true
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri.ToString().Contains("/guests/123")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(guestResponse))
                });

            var httpClient = new HttpClient(httpMessageHandler.Object)
            {
                BaseAddress = new Uri(_integration.ApiEndpoint)
            };

            _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // Act
            var result = await _adapter.GetGuestProfileAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.PMSGuestId);
            Assert.Equal("John Doe", result.FullName);
            Assert.True(result.IsVIP);
        }
    }
}
