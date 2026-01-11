using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Api.Models.YachtTourModels;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuestFlow.Application.Tests.Integration.Crud;

public class YachtToursCrudTests : IClassFixture<AuthorizedTestWebApplicationFactory>
{
    private readonly AuthorizedTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public YachtToursCrudTests(AuthorizedTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task YachtTours_CRUD_Should_Work()
    {
        // Ensure Personnel(1) exists (used by update request and some service validations)
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

        // Seed City (required by YachtTour request)
        int cityId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var city = new CityEntity { CityName = "Bodrum", Country = "TR" };
            db.Cities.Add(city);
            db.SaveChanges();
            cityId = city.Id;
        }

        // Create guest (required FK)
        var guestEmail = $"yacht.guest.{Guid.NewGuid():N}@guestflow.local";
        var createGuestResp = await _client.PostAsJsonAsync("/api/v1.0/guests", new AddGuestRequest
        {
            FullName = "Yacht Guest",
            Email = guestEmail,
            PhoneNumber = "+905551234570",
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

        var tourDate = DateTime.UtcNow.AddDays(3);

        // Create yacht tour
        var createResp = await _client.PostAsJsonAsync("/api/v1.0/yachttours", new AddYachtTourRequest
        {
            TourDate = tourDate,
            NumberOfPeople = 4,
            Price = 1500m,
            YachtName = "Test Yacht",
            OwnerGuestId = guestId,
            CityId = cityId,
            Currency = "USD",
            CreateInvoice = false
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK, await createResp.Content.ReadAsStringAsync());

        int yachtTourId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            yachtTourId = db.YachtTours.OrderByDescending(x => x.Id).First().Id;
            yachtTourId.Should().BeGreaterThan(0);
        }

        // Update (UpdateYachtTourRequest requires YachtName + PersonnelId even though Add allows them to be optional)
        var updateResp = await _client.PutAsJsonAsync($"/api/v1.0/yachttours/{yachtTourId}", new UpdateYachtTourRequest
        {
            TourDate = tourDate,
            NumberOfPeople = 5,
            Price = 1600m,
            SpecialRequest = "Updated request",
            YachtName = "Test Yacht",
            OwnerGuestId = guestId,
            PersonnelId = 1,
            CityId = cityId
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK, await updateResp.Content.ReadAsStringAsync());

        // Delete
        var deleteResp = await _client.DeleteAsync($"/api/v1.0/yachttours/{yachtTourId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK, await deleteResp.Content.ReadAsStringAsync());
    }
}

