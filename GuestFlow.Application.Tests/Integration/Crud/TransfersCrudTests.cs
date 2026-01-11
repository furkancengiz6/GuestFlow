using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Api.Models.TransferModel;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuestFlow.Application.Tests.Integration.Crud;

public class TransfersCrudTests : IClassFixture<AuthorizedTestWebApplicationFactory>
{
    private readonly AuthorizedTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TransfersCrudTests(AuthorizedTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Transfers_CRUD_Should_Work()
    {
        // Ensure Personnel(1) exists because Transfer creation auto-assigns PersonnelId from JWT claim ("id" = 1)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            if (!db.Personnels.Any(p => p.Id == 1))
            {
                db.Personnels.Add(new PersonnelEntity
                {
                    Id = 1,
                    FullName = "Integration Admin",
                    Email = "integration@guestflow.local",
                    Password = "not-used-in-tests",
                    UserType = UserType.Admin
                });
                db.SaveChanges();
            }
        }

        // Create guest (required FK)
        var guestEmail = $"transfer.guest.{Guid.NewGuid():N}@guestflow.local";
        var createGuestResp = await _client.PostAsJsonAsync("/api/v1.0/guests", new AddGuestRequest
        {
            FullName = "Transfer Guest",
            Email = guestEmail,
            PhoneNumber = "+905551234568",
            Nationality = "TR",
            IsSpecialGuest = false
        });
        createGuestResp.StatusCode.Should().Be(HttpStatusCode.OK, await createGuestResp.Content.ReadAsStringAsync());

        int guestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            guestId = db.Guests.Single(g => g.Email == guestEmail).Id;
        }

        // Create transfer
        var transferDate = DateTime.UtcNow.AddDays(1);
        var createResp = await _client.PostAsJsonAsync("/api/v1.0/transfers", new AddTransferRequest
        {
            TransferDate = transferDate,
            PickupAddress = "IST Airport",
            DropoffAddress = "Hotel",
            Price = 250m,
            GuestId = guestId,
            Currency = "USD",
            CreateInvoice = false
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK, await createResp.Content.ReadAsStringAsync());

        int transferId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            transferId = db.Transfers.OrderByDescending(t => t.Id).First().Id;
            transferId.Should().BeGreaterThan(0);
        }

        // Update transfer
        var updateResp = await _client.PutAsJsonAsync($"/api/v1.0/transfers/{transferId}", new UpdateTransferRequest
        {
            TransferDate = transferDate,
            PickupAddress = "IST Airport (Updated)",
            DropoffAddress = "Hotel (Updated)",
            Price = 300m,
            GuestId = guestId,
            Currency = "USD",
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK, await updateResp.Content.ReadAsStringAsync());

        // Delete transfer (restricted to Manager/Admin/Owner - we are Admin via TestAuth)
        var deleteResp = await _client.DeleteAsync($"/api/v1.0/transfers/{transferId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK, await deleteResp.Content.ReadAsStringAsync());
    }
}

