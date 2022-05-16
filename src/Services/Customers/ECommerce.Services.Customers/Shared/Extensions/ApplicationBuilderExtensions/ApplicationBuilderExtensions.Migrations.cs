using Microsoft.EntityFrameworkCore;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.Shared.Extensions.ApplicationBuilderExtensions;

public static partial class ApplicationBuilderExtensions
{
    public static async Task ApplyDatabaseMigrations(this IApplicationBuilder app, ILogger logger)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        if (configuration.GetValue<bool>("UseInMemoryDatabase") == false)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<CustomersDbContext>();

            logger.LogInformation("Updating catalog database...");

            await dbContext.Database.MigrateAsync();

            logger.LogInformation("Updated catalog database");
        }
    }
}
