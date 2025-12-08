using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GuestFlow.Persistence.Context
{
    public class GuestFlowDbContextFactory : IDesignTimeDbContextFactory<GuestFlowDbContext>
    {
        public GuestFlowDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GuestFlowDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=GuestFlowDb;Trusted_Connection=True;TrustServerCertificate=True;",
                x => x.MigrationsAssembly("GuestFlow.Persistence"));

            return new GuestFlowDbContext(optionsBuilder.Options);
        }
    }
}

