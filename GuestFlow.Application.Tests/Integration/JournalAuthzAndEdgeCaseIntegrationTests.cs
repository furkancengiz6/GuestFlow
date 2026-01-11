using FluentAssertions;
using GuestFlow.Api.Models;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GuestFlow.Application.Tests.Integration;

public sealed class JournalAuthzAndEdgeCaseIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JournalAuthzAndEdgeCaseIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Journal_And_Export_Should_Require_Authentication()
    {
        (await _client.GetAsync("/api/v1.0/Journal/preview?invoiceId=1")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await _client.GetAsync("/api/v1.0/Export/journal/csv?startDate=2026-01-01&endDate=2026-01-02")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Journal_And_Export_Should_Reject_NonStaff_Roles()
    {
        // Arrange: register (creates Staff), then downgrade to Reception, then login again
        var email = $"authz.{Guid.NewGuid():N}@guestflow.local";
        var password = "A9!xQ2#kLm";

        await RegisterAsync(email, password);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var personnel = db.Personnels.OrderByDescending(p => p.Id).First(p => p.Email == email);
            personnel.UserType = UserType.Reception;
            await db.SaveChangesAsync();
        }

        var accessToken = await LoginAndGetAccessTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Act + Assert
        (await _client.GetAsync("/api/v1.0/Journal/preview?invoiceId=1")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await _client.GetAsync("/api/v1.0/Export/journal/csv?startDate=2026-01-01&endDate=2026-01-02")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Journal_Preview_Should_Return_400_For_Unknown_Invoice()
    {
        var (email, password, token) = await CreateAndLoginStaffAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync("/api/v1.0/Journal/preview?invoiceId=999999");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Journal_Post_Should_Return_400_For_Invalid_PostingDate()
    {
        var (_, _, token) = await CreateAndLoginStaffAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Seed minimal invoice
        int invoiceId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var guest = db.Guests.Add(new GuestFlow.Domain.Entities.Core.GuestEntity
            {
                FullName = "Edge Guest",
                Email = $"edge.{Guid.NewGuid():N}@guestflow.local",
                PhoneNumber = "+905551234567",
                Nationality = "TR",
                GuestCode = $"GE-{Guid.NewGuid():N}".Substring(0, 12),
                IsSpecialGuest = false
            }).Entity;
            await db.SaveChangesAsync();

            var invoice = db.Invoices.Add(new GuestFlow.Domain.Entities.Core.InvoicesEntity
            {
                InvoiceNumber = Random.Shared.Next(100000, 999999),
                IssueDate = DateTime.UtcNow,
                TotalAmount = 10m,
                Currency = "TRY",
                Notes = "edge",
                PdfUrl = "",
                GuestId = guest.Id,
                Status = GuestFlow.Domain.Entities.Core.InvoiceStatus.Draft,
                IsPdfGenerated = false
            }).Entity;
            await db.SaveChangesAsync();

            invoiceId = invoice.Id;
        }

        var resp = await _client.PostAsJsonAsync("/api/v1.0/Journal/post", new
        {
            invoiceId,
            postingDate = "11/01/2026", // wrong format
            lines = new[]
            {
                new { accountCode = "1100", debit = 10m, credit = 0m, description = "AR" },
                new { accountCode = "4000", debit = 0m, credit = 10m, description = "Revenue" }
            }
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Journal_Post_Should_Return_400_For_Empty_Lines()
    {
        var (_, _, token) = await CreateAndLoginStaffAsync();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Seed minimal invoice
        int invoiceId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var guest = db.Guests.Add(new GuestFlow.Domain.Entities.Core.GuestEntity
            {
                FullName = "Edge Guest 2",
                Email = $"edge2.{Guid.NewGuid():N}@guestflow.local",
                PhoneNumber = "+905551234567",
                Nationality = "TR",
                GuestCode = $"GE-{Guid.NewGuid():N}".Substring(0, 12),
                IsSpecialGuest = false
            }).Entity;
            await db.SaveChangesAsync();

            var invoice = db.Invoices.Add(new GuestFlow.Domain.Entities.Core.InvoicesEntity
            {
                InvoiceNumber = Random.Shared.Next(100000, 999999),
                IssueDate = DateTime.UtcNow,
                TotalAmount = 10m,
                Currency = "TRY",
                Notes = "edge2",
                PdfUrl = "",
                GuestId = guest.Id,
                Status = GuestFlow.Domain.Entities.Core.InvoiceStatus.Draft,
                IsPdfGenerated = false
            }).Entity;
            await db.SaveChangesAsync();
            invoiceId = invoice.Id;
        }

        var resp = await _client.PostAsJsonAsync("/api/v1.0/Journal/post", new
        {
            invoiceId,
            postingDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            lines = Array.Empty<object>()
        });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(string email, string password, string token)> CreateAndLoginStaffAsync()
    {
        var email = $"staff.{Guid.NewGuid():N}@guestflow.local";
        var password = "A9!xQ2#kLm";
        await RegisterAsync(email, password);
        var token = await LoginAndGetAccessTokenAsync(email, password);
        return (email, password, token);
    }

    private async Task RegisterAsync(string email, string password)
    {
        var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            FullName = "Test User",
            Email = email,
            Password = password
        });
        registerResp.StatusCode.Should().Be(HttpStatusCode.OK, await registerResp.Content.ReadAsStringAsync());
    }

    private async Task<string> LoginAndGetAccessTokenAsync(string email, string password)
    {
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
        return accessToken!;
    }
}

