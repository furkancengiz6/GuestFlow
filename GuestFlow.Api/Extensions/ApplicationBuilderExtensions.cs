using GuestFlow.Domain.DataProtection;
using GuestFlow.Persistence.Context;
using GuestFlow.Persistence.Data;
using Microsoft.EntityFrameworkCore;
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

                try
                {
                    // Veritabanının oluşturulduğundan emin ol
                    await context.Database.MigrateAsync();

                    var seeder = new DatabaseSeeder(context, logger, dataProtection);
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

