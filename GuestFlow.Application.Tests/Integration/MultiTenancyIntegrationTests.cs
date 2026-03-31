using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GuestFlow.Api.Models; // For ApiResponse wrapper (if needed explicitly, but checks below use qualified)
using GuestFlow.Application.Operations.Guest.Dtos; // For GetGuestDto
using Xunit;

namespace GuestFlow.Application.Tests.Integration
{
    public class MultiTenancyIntegrationTests : IClassFixture<AuthorizedTestWebApplicationFactory>
    {
        private readonly AuthorizedTestWebApplicationFactory _factory;

        public MultiTenancyIntegrationTests(AuthorizedTestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetGuests_AsTenant2_ReturnsOnlyTenant2Guests()
        {
            // Arrange
            var client = _factory.CreateClient();
            // TenantResolutionMiddleware will pick this up since TestAuthHandler provides no TenantId claim
            client.DefaultRequestHeaders.Add("X-Tenant-ID", "2"); 

            // Act
            var response = await client.GetAsync("/api/v1/guests");
            
            // Assert
            response.EnsureSuccessStatusCode();
            
            // Explicitly use GuestFlow.Api.Models.ApiResponse and GuestFlow.Application.Models.PagedResult to avoid ambiguity
            // Note: If Api.Models.PagedResult exists, we probably want Application.Models.PagedResult as it is the domain model usually returned.
            var result = await response.Content.ReadFromJsonAsync<GuestFlow.Api.Models.ApiResponse<GuestFlow.Application.Models.PagedResult<GetGuestDto>>>();

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            
            // Allow for empty list if seeding failed, but ideally it shouldn't
            // But we expect "Tenant 2 Guest A"
            result.Data.Data.Should().Contain(g => g.FullName == "Tenant 2 Guest A");
            result.Data.Data.Should().NotContain(g => g.FullName.Contains("James")); // Standard seed data
        }

        [Fact]
        public async Task GetGuests_AsDefaultTenant_ReturnsOnlyDefaultGuests()
        {
            // Arrange
            var client = _factory.CreateClient();
            // No Header -> TenantResolutionMiddleware defaults to 0 (Standard Tenant)

            // Act
            var response = await client.GetAsync("/api/v1/guests");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<GuestFlow.Api.Models.ApiResponse<GuestFlow.Application.Models.PagedResult<GetGuestDto>>>();

            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();

            result.Data.Data.Should().Contain(g => g.FullName.Contains("James")); // Standard seed data
            result.Data.Data.Should().NotContain(g => g.FullName == "Tenant 2 Guest A"); // Tenant 2 data
        }

        [Fact]
        public async Task AccessingSpecificGuest_FromWrongTenant_ReturnsNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add("X-Tenant-ID", "2");

            // Assuming a standard guest exists with ID 1 (based on seeding order)
            // Ideally we would fetch it first from Tenant 1 context, but hardcoding ID 1 is a reasonable assumption for seeded data.
            int targetGuestId = 1; 

            // Act: Try to get a Tenant 1 guest using Tenant 2 context
            var response = await client.GetAsync($"/api/v1/guests/{targetGuestId}");

            // Assert
            // Global Query Filter should hide it, treating it as if it doesn't exist -> 404 Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
