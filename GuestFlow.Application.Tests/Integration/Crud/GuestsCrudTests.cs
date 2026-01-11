using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuestFlow.Application.Tests.Integration.Crud;

public class GuestsCrudTests : IClassFixture<AuthorizedTestWebApplicationFactory>
{
    private readonly AuthorizedTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GuestsCrudTests(AuthorizedTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Guests_CRUD_Should_Work()
    {
        // Create
        var createResp = await _client.PostAsJsonAsync("/api/v1.0/guests", new AddGuestRequest
        {
            FullName = "Integration Guest",
            Email = "integration.guest@guestflow.local",
            PhoneNumber = "+905551234567",
            Nationality = "TR",
            IsSpecialGuest = false
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK, await createResp.Content.ReadAsStringAsync());

        int guestId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var guest = db.Guests.Single(g => g.Email == "integration.guest@guestflow.local");
            guestId = guest.Id;
            guestId.Should().BeGreaterThan(0);
        }

        // Update
        var updateResp = await _client.PutAsJsonAsync($"/api/v1.0/guests/{guestId}", new UpdateGuestRequest
        {
            FullName = "Integration Guest Updated",
            Email = "integration.guest@guestflow.local",
            PhoneNumber = "+905551234567",
            Nationality = "TR",
            IsSpecialGuest = true
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK, await updateResp.Content.ReadAsStringAsync());

        // Read
        var getResp = await _client.GetAsync($"/api/v1.0/guests/{guestId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK, await getResp.Content.ReadAsStringAsync());

        // Delete (soft-delete)
        var deleteResp = await _client.DeleteAsync($"/api/v1.0/guests/{guestId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK, await deleteResp.Content.ReadAsStringAsync());
    }
}

