using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.UnitOfWork;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Operations;
using GuestFlow.Application.Operations.Supplier;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GuestFlow.Application.Tests.Integration
{
    public class SupplierCostIntegrationTests : IAsyncLifetime
    {
        private GuestFlowDbContext _context = null!;
        private UnitOfWork _unitOfWork = null!;
        private SupplierCostService _service = null!;

        public async Task InitializeAsync()
        {
            var options = new DbContextOptionsBuilder<GuestFlowDbContext>()
                .UseInMemoryDatabase($"SupplierCostTest_{Guid.NewGuid()}")
                .Options;

            var tenantProviderMock = new Mock<GuestFlow.Persistence.MultiTenancy.ITenantProvider>();
            tenantProviderMock.Setup(x => x.TenantId).Returns(1);

            _context = new GuestFlowDbContext(options, tenantProviderMock.Object, new GuestFlow.Domain.Events.NullDomainEventDispatcher());

            // Seed supplier
            var supplier = new Supplier
            {
                Name = "Test Supplier",
                Type = "General",
                DefaultCurrency = "USD",
                DefaultCost = 0m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            await _context.Suppliers.AddAsync(supplier);
            await _context.SaveChangesAsync();

            // Seed a transfer with supplier cost
            var transfer = new TransferEntity
            {
                GuestId = 0,
                TransferDate = DateTime.UtcNow,
                FinalPrice = 200m,
                Currency = "USD",
                SupplierName = "Test Supplier",
                SupplierCost = 80m,
                CreatedDate = DateTime.UtcNow
            };
            await _context.Transfers.AddAsync(transfer);
            await _context.SaveChangesAsync();

            _unitOfWork = new UnitOfWork(_context);
            var logger = new Mock<ILogger<SupplierCostService>>().Object;
            _service = new SupplierCostService(_unitOfWork, logger);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task SyncSupplierCosts_CreatesSupplierCostRecord()
        {
            // Act
            var result = await _service.SyncSupplierCostsAsync();

            // Assert
            result.Success.Should().BeTrue();

            var supplierCosts = await _unitOfWork.SupplierCosts.GetAll().ToListAsync();
            supplierCosts.Should().HaveCount(1);

            var sc = supplierCosts.First();
            sc.CostAmount.Should().Be(80m);
            sc.Currency.Should().Be("USD");
            sc.SupplierId.Should().BeGreaterThan(0);
            sc.TransferId.Should().NotBeNull();
        }
    }
}

