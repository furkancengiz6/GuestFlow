using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using GuestFlow.Api.Models;
using Xunit;

namespace GuestFlow.Application.Tests.Integration.Smoke;

public class ApiSmokeTests : IClassFixture<Integration.TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string StrongPassword = "A9!xQ2#kLm";

    public ApiSmokeTests(Integration.TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Live_Should_Return_200()
    {
        var resp = await _client.GetAsync("/health/live");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_Login_Me_Should_Work_And_Sanitize_FullName()
    {
        var email = $"test.{Guid.NewGuid():N}@guestflow.local";
        var password = StrongPassword;
        var fullName = "Test User";

        // Register (public endpoint)
        var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            Email = email,
            FullName = fullName,
            Password = password
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.OK, await registerResp.Content.ReadAsStringAsync());

        // Login (public endpoint) -> accessToken
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

        // /auth/me (authorized)
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var meResp = await _client.GetAsync("/api/v1.0/auth/me");
        meResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var meJson = await meResp.Content.ReadAsStringAsync();
        meJson.Should().Contain(email);
    }

    [Fact]
    public async Task Authenticated_Guests_List_Should_Return_200()
    {
        var email = $"staff.{Guid.NewGuid():N}@guestflow.local";
        var password = StrongPassword;

        // Register creates Staff user by default (allowed for GuestsController)
        var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            Email = email,
            FullName = "Staff User",
            Password = password
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.OK, await registerResp.Content.ReadAsStringAsync());

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

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        // Guests list is protected with Staff/Admin role
        var resp = await _client.GetAsync("/api/v1.0/guests?pageNumber=1&pageSize=1");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

