using FluentAssertions;
using GuestFlow.Api.Models;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GuestFlow.Application.Tests.Integration;

public sealed class JournalIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JournalIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Journal_Flow_Should_Preview_Post_Block_Duplicate_And_Fetch_ByInvoice()
    {
        // Arrange: create and login a real Staff user
        var email = $"journal.{Guid.NewGuid():N}@guestflow.local";
        var password = "A9!xQ2#kLm";

        await RegisterAsync(email, password);
        var accessToken = await LoginAndGetAccessTokenAsync(email, password);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        // Seed: guest + invoice + invoice items
        int invoiceId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();

            var guest = new GuestEntity
            {
                FullName = "Journal Test Guest",
                Email = $"guest.{Guid.NewGuid():N}@guestflow.local",
                PhoneNumber = "+905551234567",
                Nationality = "TR",
                GuestCode = $"GT-{Guid.NewGuid():N}".Substring(0, 12),
                IsSpecialGuest = false
            };
            db.Guests.Add(guest);
            await db.SaveChangesAsync();

            var invoice = new InvoicesEntity
            {
                InvoiceNumber = Random.Shared.Next(100000, 999999),
                IssueDate = DateTime.UtcNow,
                TotalAmount = 300m,
                Currency = "TRY",
                Notes = "integration test",
                PdfUrl = "",
                GuestId = guest.Id,
                Status = InvoiceStatus.Draft,
                IsPdfGenerated = false
            };
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();

            db.InvoiceItems.AddRange(
                new InvoiceItemEntity
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "Transfer",
                    ServiceId = 1,
                    Amount = 100m,
                    Currency = "TRY",
                    Notes = "t1"
                },
                new InvoiceItemEntity
                {
                    InvoiceId = invoice.Id,
                    ServiceType = "CityTour",
                    ServiceId = 2,
                    Amount = 200m,
                    Currency = "TRY",
                    Notes = "c2"
                }
            );
            await db.SaveChangesAsync();

            invoiceId = invoice.Id;
        }

        // Act 1: preview
        var previewResp = await _client.GetAsync($"/api/v1.0/Journal/preview?invoiceId={invoiceId}");
        previewResp.StatusCode.Should().Be(HttpStatusCode.OK, await previewResp.Content.ReadAsStringAsync());

        var previewJson = await previewResp.Content.ReadAsStringAsync();
        using var previewDoc = JsonDocument.Parse(previewJson);
        previewDoc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var previewData = previewDoc.RootElement.GetProperty("data");
        previewData.GetProperty("invoiceId").GetInt32().Should().Be(invoiceId);
        previewData.GetProperty("totalDebit").GetDecimal().Should().Be(previewData.GetProperty("totalCredit").GetDecimal());
        previewData.GetProperty("lines").GetArrayLength().Should().BeGreaterThan(0);

        // Build post payload from preview lines
        var postingDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var lines = previewData.GetProperty("lines")
            .EnumerateArray()
            .Select(l => new
            {
                accountCode = l.GetProperty("accountCode").GetString(),
                debit = l.GetProperty("debit").GetDecimal(),
                credit = l.GetProperty("credit").GetDecimal(),
                description = l.TryGetProperty("description", out var d) ? d.GetString() : null
            })
            .ToList();

        // Act 2: post OK
        var postResp = await _client.PostAsJsonAsync("/api/v1.0/Journal/post", new
        {
            invoiceId,
            postingDate,
            lines
        });
        postResp.StatusCode.Should().Be(HttpStatusCode.OK, await postResp.Content.ReadAsStringAsync());

        var postDoc = JsonDocument.Parse(await postResp.Content.ReadAsStringAsync());
        postDoc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        postDoc.RootElement.GetProperty("data").GetBoolean().Should().BeTrue();

        // Act 3: duplicate post should fail
        var dupResp = await _client.PostAsJsonAsync("/api/v1.0/Journal/post", new
        {
            invoiceId,
            postingDate,
            lines
        });
        dupResp.StatusCode.Should().Be(HttpStatusCode.BadRequest, await dupResp.Content.ReadAsStringAsync());

        // Act 4: by-invoice should return JE details
        var byInvResp = await _client.GetAsync($"/api/v1.0/Journal/by-invoice/{invoiceId}");
        byInvResp.StatusCode.Should().Be(HttpStatusCode.OK, await byInvResp.Content.ReadAsStringAsync());
        var byInvDoc = JsonDocument.Parse(await byInvResp.Content.ReadAsStringAsync());
        byInvDoc.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        var je = byInvDoc.RootElement.GetProperty("data");
        je.GetProperty("invoiceId").GetInt32().Should().Be(invoiceId);
        je.GetProperty("journalEntryId").GetInt32().Should().BeGreaterThan(0);
        je.GetProperty("lines").GetArrayLength().Should().BeGreaterThan(0);

        // Act 5: unbalanced post should fail (use a new invoice)
        int invoiceId2;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var guest = db.Guests.OrderByDescending(g => g.Id).First();
            var invoice2 = new InvoicesEntity
            {
                InvoiceNumber = Random.Shared.Next(100000, 999999),
                IssueDate = DateTime.UtcNow,
                TotalAmount = 50m,
                Currency = "TRY",
                Notes = "integration test 2",
                PdfUrl = "",
                GuestId = guest.Id,
                Status = InvoiceStatus.Draft,
                IsPdfGenerated = false
            };
            db.Invoices.Add(invoice2);
            await db.SaveChangesAsync();
            db.InvoiceItems.Add(new InvoiceItemEntity
            {
                InvoiceId = invoice2.Id,
                ServiceType = "Transfer",
                ServiceId = 99,
                Amount = 50m,
                Currency = "TRY",
            });
            await db.SaveChangesAsync();
            invoiceId2 = invoice2.Id;
        }

        var preview2 = await _client.GetAsync($"/api/v1.0/Journal/preview?invoiceId={invoiceId2}");
        preview2.StatusCode.Should().Be(HttpStatusCode.OK, await preview2.Content.ReadAsStringAsync());
        var preview2Doc = JsonDocument.Parse(await preview2.Content.ReadAsStringAsync());
        var lines2 = preview2Doc.RootElement.GetProperty("data").GetProperty("lines")
            .EnumerateArray()
            .Select(l => new
            {
                accountCode = l.GetProperty("accountCode").GetString(),
                debit = l.GetProperty("debit").GetDecimal(),
                credit = l.GetProperty("credit").GetDecimal(),
                description = l.TryGetProperty("description", out var d) ? d.GetString() : null
            })
            .ToList();

        // Break balance by adjusting first line
        lines2[0] = new
        {
            accountCode = lines2[0].accountCode,
            debit = lines2[0].debit + 1m,
            credit = lines2[0].credit,
            description = lines2[0].description
        };

        var unbalancedResp = await _client.PostAsJsonAsync("/api/v1.0/Journal/post", new
        {
            invoiceId = invoiceId2,
            postingDate,
            lines = lines2
        });
        unbalancedResp.StatusCode.Should().Be(HttpStatusCode.BadRequest, await unbalancedResp.Content.ReadAsStringAsync());
    }

    private async Task RegisterAsync(string email, string password)
    {
        var registerResp = await _client.PostAsJsonAsync("/api/v1.0/auth/register", new RegisterRequest
        {
            FullName = "Journal Test User",
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

