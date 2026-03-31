using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GuestFlow.Api.Models;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Intelligence;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuestFlow.Application.Tests.Integration;

public class IntelligenceIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private const string StrongPassword = "A9!xQ2#kLm";

    public IntelligenceIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var email = $"staff.{Guid.NewGuid():N}@guestflow.local";
        var password = StrongPassword;

        // Register
        await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            Email = email,
            FullName = "Staff User",
            Password = password
        });

        // Login
        var loginResp = await _client.PostAsJsonAsync("/api/v1.0/auth/login", new LoginRequest
        {
            Email = email,
            Password = password
        });

        var loginJson = await loginResp.Content.ReadAsStringAsync();
        using var loginDoc = JsonDocument.Parse(loginJson);
        return loginDoc.RootElement.GetProperty("accessToken").GetString()!;
    }

    private async Task<(int GuestId, int StaffId)> SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();

        // Create Guest
        var guest = new GuestEntity
        {
            FullName = "Integration Test Guest",
            GuestCode = "ITG" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
            Email = $"guest.{Guid.NewGuid():N}@example.com",
            PhoneNumber = "05550001122",
            CreatedDate = DateTime.UtcNow
        };
        context.Guests.Add(guest);

        // Create Staff (Personnel)
        var staff = new PersonnelEntity
        {
            FullName = "Integration Test Staff",
            Email = $"staff.{Guid.NewGuid():N}@example.com",
            UserType = Domain.Entities.Enum.UserType.Staff,
            CreatedDate = DateTime.UtcNow
        };
        context.Personnels.Add(staff);

        await context.SaveChangesAsync();

        // Create Behaviors
        var behavior = new GuestBehaviorEntity
        {
            GuestId = guest.Id,
            BehaviorType = "Activity",
            Category = "Interaction",
            BehaviorValue = "1.0",
            BehaviorDate = DateTime.UtcNow,
            SentimentScore = 0.8,
            SatisfactionScore = 9,
            CreatedDate = DateTime.UtcNow
        };
        context.GuestBehaviors.Add(behavior);

        await context.SaveChangesAsync();

        return (guest.Id, staff.Id);
    }

    [Fact]
    public async Task FindBestStaffMatches_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var (guestId, _) = await SeedTestDataAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await _client.GetAsync($"/api/v1.0/intelligence/guests/{guestId}/best-staff-matches?limit=5");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("Best staff matches retrieved successfully.");
    }

    [Fact]
    public async Task GetGuestPreferencePatterns_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var (guestId, _) = await SeedTestDataAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await _client.GetAsync($"/api/v1.0/intelligence/guests/{guestId}/preference-patterns");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("Guest preference patterns retrieved successfully.");
    }

    [Fact]
    public async Task PredictGuestBehavior_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var (guestId, _) = await SeedTestDataAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var request = new
        {
            BehaviorType = "Activity",
            TargetDate = DateTime.UtcNow.AddDays(7)
        };

        // Act
        var resp = await _client.PostAsJsonAsync($"/api/v1.0/intelligence/guests/{guestId}/predict-behavior", request);

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("Behavior prediction completed successfully.");
    }

    [Fact]
    public async Task PredictRisks_ShouldReturnSuccess()
    {
        // Arrange
        var token = await GetAccessTokenAsync();
        var (guestId, _) = await SeedTestDataAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var resp = await _client.GetAsync($"/api/v1.0/intelligence/guests/{guestId}/predict-risks");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadAsStringAsync();
        content.Should().Contain("Risk prediction completed successfully.");
    }
}
