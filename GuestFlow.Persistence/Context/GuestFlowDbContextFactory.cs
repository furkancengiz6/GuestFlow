using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using GuestFlow.Domain.Events;

namespace GuestFlow.Persistence.Context
{
    public class GuestFlowDbContextFactory : IDesignTimeDbContextFactory<GuestFlowDbContext>
    {
        public GuestFlowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GuestFlowDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=GuestFlowDb;Trusted_Connection=True;TrustServerCertificate=True;",
                x => x.MigrationsAssembly("GuestFlow.Persistence"));

            return new GuestFlowDbContext(optionsBuilder.Options, new DesignTimeTenantProvider(), new NullDomainEventDispatcher());
        }

        private class DesignTimeTenantProvider : GuestFlow.Persistence.MultiTenancy.ITenantProvider
        {
             public int TenantId => 1;
             public void SetTenantId(int tenantId) { }
        }
    }
}

