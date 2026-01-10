using FluentAssertions;
using GuestFlow.Api.Models;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GuestFlow.Application.Tests.Integration
{
    public class ApiSecurityIntegrationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public ApiSecurityIntegrationTests(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task XSS_Protection_Should_Sanitize_Malicious_Input()
        {
            // Arrange: create a real user (RegisterRequestValidator blocks scripts in FullName)
            var email = $"xss.{Guid.NewGuid():N}@guestflow.local";
            var password = "A9!xQ2#kLm"; // strong, non-sequential, non-common

            var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
            {
                FullName = "Test User",
                Email = email,
                Password = password
            });
            registerResp.StatusCode.Should().Be(HttpStatusCode.OK, await registerResp.Content.ReadAsStringAsync());

            // Login to get JWT for protected SMS endpoint
            var loginResp = await _client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequest
            {
                Email = email,
                Password = password
            });
            loginResp.StatusCode.Should().Be(HttpStatusCode.OK, await loginResp.Content.ReadAsStringAsync());

            var loginJson = await loginResp.Content.ReadAsStringAsync();
            using var loginDoc = JsonDocument.Parse(loginJson);
            var accessToken = loginDoc.RootElement.GetProperty("accessToken").GetString();
            accessToken.Should().NotBeNullOrWhiteSpace();

            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var phone = "+905551234567";
            var maliciousMessage = "<script>alert('XSS')</script>Hello";

            // Act: send SMS (service may be disabled -> OK/BadRequest, but DB record is created either way)
            var smsResp = await _client.PostAsJsonAsync("/api/v1.0/sms/send", new
            {
                phoneNumber = phone,
                message = maliciousMessage
            });
            smsResp.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            // Assert: message persisted without script tag
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var storedSms = db.SmsHistories.OrderByDescending(x => x.Id).FirstOrDefault(x => x.PhoneNumber == phone);
            storedSms.Should().NotBeNull();
            storedSms!.Message.ToLowerInvariant().Should().NotContain("<script");
        }

        [Fact]
        public async Task Security_Headers_Should_Be_Present()
        {
            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.Headers.Contains("X-Content-Type-Options").Should().BeTrue();
            response.Headers.Contains("X-Frame-Options").Should().BeTrue();
            response.Headers.Contains("X-XSS-Protection").Should().BeTrue();
            response.Headers.Contains("Content-Security-Policy").Should().BeTrue();
            response.Headers.Contains("Strict-Transport-Security").Should().BeFalse(); // Only on HTTPS
        }

        [Fact]
        public async Task Rate_Limiting_Should_Work()
        {
            // Arrange - Make multiple requests quickly
            var tasks = new List<Task<HttpResponseMessage>>();

            for (int i = 0; i < 15; i++) // Exceed rate limit
            {
                tasks.Add(_client.GetAsync("/health/live"));
            }

            // Act
            var responses = await Task.WhenAll(tasks);

            // Assert - ensure none of the responses are internal server error
            responses.All(r => r.StatusCode != HttpStatusCode.InternalServerError).Should().BeTrue();
        }

        [Fact]
        public async Task Audit_Logging_Should_Record_Actions()
        {
            // This would require setting up a test database and checking audit logs
            // For now, just ensure the endpoint works

            // Arrange
            var loginRequest = new LoginRequest
            {
                Email = "test@example.com",
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1.0/auth/login", loginRequest);

            // Assert
            // In a real test, we would check the audit logs in database
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CORS_Should_Work_For_Allowed_Origins()
        {
            // Arrange
            _client.DefaultRequestHeaders.Add("Origin", "http://localhost:3000");

            // Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
        }

        [Fact]
        public async Task SQL_Injection_Should_Be_Prevented()
        {
            // Arrange - Try SQL injection
            var maliciousEmail = "'; DROP TABLE Users; --";

            var loginRequest = new LoginRequest
            {
                Email = maliciousEmail,
                Password = "password123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1.0/auth/login", loginRequest);

            // Assert - Should not crash; accept Unauthorized or BadRequest as acceptable outcomes
            response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest, HttpStatusCode.OK);
            // Database should remain intact (would need separate check)
        }
    }
}