using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GuestFlow.Api.Models.CityTourModels;
using GuestFlow.Api.Models.GuestModels;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Persistence.Context;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GuestFlow.Application.Tests.Integration.Crud;

public class CityToursCrudTests : IClassFixture<AuthorizedTestWebApplicationFactory>
{
    private readonly AuthorizedTestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CityToursCrudTests(AuthorizedTestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CityTours_CRUD_Should_Work()
    {
        // Ensure Personnel(1) exists because CityTour creation auto-assigns PersonnelId from JWT claim ("id" = 1)
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

        // Seed City + Tour definitions (required by CityTour request)
        int cityId;
        int tourId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            var city = new CityEntity { CityName = "Istanbul", Country = "TR" };
            db.Cities.Add(city);
            db.SaveChanges();
            cityId = city.Id;

            var tour = new TourEntity { Name = "Integration Tour", CityId = cityId, IsActive = true };
            db.Tours.Add(tour);
            db.SaveChanges();
            tourId = tour.Id;
        }

        // Create guest (required FK)
        var guestEmail = $"citytour.guest.{Guid.NewGuid():N}@guestflow.local";
        var createGuestResp = await _client.PostAsJsonAsync("/api/v1.0/guests", new AddGuestRequest
        {
            FullName = "CityTour Guest",
            Email = guestEmail,
            PhoneNumber = "+905551234569",
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

        var tourDate = DateTime.UtcNow.AddDays(2);

        // Create city tour
        var createResp = await _client.PostAsJsonAsync("/api/v1.0/citytours", new AddCityTourRequest
        {
            TourDate = tourDate,
            Language = "EN",
            DurationHours = 4,
            Price = 500m,
            OwnerGuestId = guestId,
            CityId = cityId,
            TourId = tourId,
            Currency = "USD",
            CreateInvoice = false
        });
        createResp.StatusCode.Should().Be(HttpStatusCode.OK, await createResp.Content.ReadAsStringAsync());

        int cityTourId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GuestFlowDbContext>();
            cityTourId = db.CityTours.OrderByDescending(x => x.Id).First().Id;
            cityTourId.Should().BeGreaterThan(0);
        }

        // Update
        var updateResp = await _client.PutAsJsonAsync($"/api/v1.0/citytours/{cityTourId}", new UpdateCityTourRequest
        {
            TourDate = tourDate,
            Language = "TR",
            DurationHours = 5,
            Price = 600m,
            OwnerGuestId = guestId,
            CityId = cityId,
            TourId = tourId,
            Currency = "USD"
        });
        updateResp.StatusCode.Should().Be(HttpStatusCode.OK, await updateResp.Content.ReadAsStringAsync());

        // Delete (restricted to Manager/Admin/Owner - we are Admin via TestAuth)
        var deleteResp = await _client.DeleteAsync($"/api/v1.0/citytours/{cityTourId}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.OK, await deleteResp.Content.ReadAsStringAsync());
    }
}

