using GuestFlow.Domain.DataProtection;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.Data;
using GuestFlow.Persistence.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GuestFlow.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
                var context = services.GetRequiredService<GuestFlowDbContext>();
                var dataProtection = services.GetRequiredService<IDataProtection>();
                var configuration = services.GetRequiredService<IConfiguration>();
                var tenantProvider = services.GetRequiredService<ITenantProvider>();

                try
                {
                    // SQLite ve In-Memory veritabanları için EnsureCreated kullan
                    var databaseProvider = context.Database.ProviderName;
                    if (databaseProvider == "Microsoft.EntityFrameworkCore.Sqlite" || 
                        databaseProvider == "Microsoft.EntityFrameworkCore.InMemory")
                    {
                        await context.Database.EnsureCreatedAsync();
                    }
                    else
                    {
                        // Diğer ilişkisel veritabanları (SQL Server vb.) için migrationları uygula
                        await context.Database.MigrateAsync();
                    }

                    var seeder = new DatabaseSeeder(context, logger, dataProtection, configuration, tenantProvider);
                    await seeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Veritabanı seed işlemi sırasında hata oluştu!");
                    throw;
                }
            }
        }
    }
}

