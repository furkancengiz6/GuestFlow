using FluentAssertions;
using GuestFlow.Api.Models;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace GuestFlow.Application.Tests.Integration;

public sealed class ExportJournalIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ExportJournalIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Export_Journal_Csv_Should_Return_File_For_DateRange()
    {
        // Arrange: create and login a real Staff user
        var email = $"export.journal.{Guid.NewGuid():N}@guestflow.local";
        var password = "A9!xQ2#kLm";

        await RegisterAsync(email, password);
        var accessToken = await LoginAndGetAccessTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var postingDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc);
        int journalEntryId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();

            var je = new JournalEntry
            {
                PostingDate = postingDate,
                Currency = "TRY",
                Description = "Export test JE",
                TotalDebit = 100m,
                TotalCredit = 100m,
                CreatedBy = "integration-test",
                Lines = new List<JournalLine>
                {
                    new JournalLine { AccountCode = "1100", Debit = 100m, Credit = 0m, Description = "AR" },
                    new JournalLine { AccountCode = "4000", Debit = 0m, Credit = 100m, Description = "Revenue" },
                }
            };

            db.JournalEntries.Add(je);
            await db.SaveChangesAsync();
            journalEntryId = je.Id;
        }

        var startDate = "2026-01-10";
        var endDate = "2026-01-11";

        // Act
        var resp = await _client.GetAsync($"/api/v1.0/Export/journal/csv?startDate={startDate}&endDate={endDate}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK, await resp.Content.ReadAsStringAsync());
        resp.Content.Headers.ContentType!.ToString().Should().Contain("text/csv");

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        var csv = Encoding.UTF8.GetString(bytes);

        csv.Should().Contain("JournalEntryId,InvoiceId,PostingDate,Currency,Description");
        csv.Should().Contain(journalEntryId.ToString());
        csv.Should().Contain("1100");
        csv.Should().Contain("4000");
    }

    [Fact]
    public async Task Export_Journal_Excel_Should_Return_File_For_DateRange()
    {
        // Arrange: create and login a real Staff user
        var email = $"export.journal.excel.{Guid.NewGuid():N}@guestflow.local";
        var password = "A9!xQ2#kLm";

        await RegisterAsync(email, password);
        var accessToken = await LoginAndGetAccessTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();

            var je = new JournalEntry
            {
                PostingDate = new DateTime(2026, 1, 11, 0, 0, 0, DateTimeKind.Utc),
                Currency = "TRY",
                Description = "Export test JE (excel)",
                TotalDebit = 10m,
                TotalCredit = 10m,
                CreatedBy = "integration-test",
                Lines = new List<JournalLine>
                {
                    new JournalLine { AccountCode = "1100", Debit = 10m, Credit = 0m, Description = "AR" },
                    new JournalLine { AccountCode = "4000", Debit = 0m, Credit = 10m, Description = "Revenue" },
                }
            };

            db.JournalEntries.Add(je);
            await db.SaveChangesAsync();
        }

        var startDate = "2026-01-10";
        var endDate = "2026-01-11";

        // Act
        var resp = await _client.GetAsync($"/api/v1.0/Export/journal/excel?startDate={startDate}&endDate={endDate}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK, await resp.Content.ReadAsStringAsync());
        resp.Content.Headers.ContentType!.ToString().Should().Contain("spreadsheetml");

        var bytes = await resp.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(100);
        // XLSX is a zip file -> starts with 'PK'
        bytes[0].Should().Be((byte)'P');
        bytes[1].Should().Be((byte)'K');
    }

    private async Task RegisterAsync(string email, string password)
    {
        var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            FullName = "Export Journal Test User",
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
        using var loginDoc = System.Text.Json.JsonDocument.Parse(loginJson);
        var accessToken = loginDoc.RootElement.GetProperty("accessToken").GetString();
        accessToken.Should().NotBeNullOrWhiteSpace();
        return accessToken!;
    }
}

