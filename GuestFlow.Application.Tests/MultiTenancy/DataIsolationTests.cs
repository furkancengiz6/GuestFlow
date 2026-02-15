using FluentAssertions;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GuestFlow.Application.Tests.MultiTenancy
{
    public class DataIsolationTests
    {
        private readonly Mock<ITenantProvider> _tenantProviderMock;
        private readonly DbContextOptions<GuestFlowDbContext> _options;

        public DataIsolationTests()
        {
            _tenantProviderMock = new Mock<ITenantProvider>();
            _options = new DbContextOptionsBuilder<GuestFlowDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task Should_Only_Return_Data_For_Current_Tenant()
        {
            // Arrange
            // 1. Create Tenant 1 context and add data
            _tenantProviderMock.Setup(t => t.TenantId).Returns(1);
            using (var context1 = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                context1.Guests.Add(new GuestEntity { Name = "Tenant1 Guest", Surname = "Test", Email = "t1@test.com", IdentityNumber = "123", Nationality = "TR" });
                await context1.SaveChangesAsync();
            }

            // 2. Create Tenant 2 context and add data
            _tenantProviderMock.Setup(t => t.TenantId).Returns(2);
            using (var context2 = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                context2.Guests.Add(new GuestEntity { Name = "Tenant2 Guest", Surname = "Test", Email = "t2@test.com", IdentityNumber = "456", Nationality = "TR" });
                await context2.SaveChangesAsync();
            }

            // Act
            // 3. Switch back to Tenant 1 and query
            _tenantProviderMock.Setup(t => t.TenantId).Returns(1);
            using (var contextQuery = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                var guests = await contextQuery.Guests.ToListAsync();

                // Assert
                guests.Should().HaveCount(1);
                guests.First().Name.Should().Be("Tenant1 Guest");
                guests.First().TenantId.Should().Be(1);
            }
        }

        [Fact]
        public async Task IgnoreQueryFilters_Should_Return_All_Tenants_Data()
        {
             // Arrange
            // 1. Create Tenant 1 context and add data
            _tenantProviderMock.Setup(t => t.TenantId).Returns(1);
            using (var context1 = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                context1.Guests.Add(new GuestEntity { Name = "Tenant1 Guest", Surname = "Test", Email = "t1@test.com", IdentityNumber = "123", Nationality = "TR" });
                await context1.SaveChangesAsync();
            }

            // 2. Create Tenant 2 context and add data
            _tenantProviderMock.Setup(t => t.TenantId).Returns(2);
            using (var context2 = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                context2.Guests.Add(new GuestEntity { Name = "Tenant2 Guest", Surname = "Test", Email = "t2@test.com", IdentityNumber = "456", Nationality = "TR" });
                await context2.SaveChangesAsync();
            }

            // Act
            // 3. Query with IgnoreQueryFilters
            using (var contextQuery = new GuestFlowDbContext(_options, _tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher()))
            {
                var guests = await contextQuery.Guests.IgnoreQueryFilters().ToListAsync();

                // Assert
                guests.Should().HaveCount(2);
            }
        }
    }
}
