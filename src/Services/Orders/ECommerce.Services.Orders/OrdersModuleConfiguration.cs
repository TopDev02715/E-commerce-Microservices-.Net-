using BuildingBlocks.Abstractions.Web.Module;
using BuildingBlocks.Monitoring;
using ECommerce.Services.Orders.Shared.Extensions.ApplicationBuilderExtensions;
using ECommerce.Services.Orders.Shared.Extensions.ServiceCollectionExtensions;

namespace ECommerce.Services.Customers;

public class OrdersModuleConfiguration : IRootModuleDefinition
{
    public const string OrderModulePrefixUri = "api/v1/orders";

    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddInfrastructure(configuration, webHostEnvironment);

        services.AddStorage(configuration);

        // Add Sub Modules Services
        return services;
    }

    public async Task<WebApplication> ConfigureModule(WebApplication app)
    {
        if (app.Environment.IsEnvironment("test") == false)
            app.UseMonitoring();

        await app.ApplyDatabaseMigrations(app.Logger);
        await app.SeedData(app.Logger, app.Environment);

        await app.UseInfrastructure(app.Logger);

        return app;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", (HttpContext context) =>
        {
            var requestId = context.Request.Headers.TryGetValue("X-Request-Id", out var requestIdHeader)
                ? requestIdHeader.FirstOrDefault()
                : string.Empty;

            return $"Orders Service Apis, RequestId: {requestId}";
        }).ExcludeFromDescription();

        // Add Sub Modules Endpoints
        return endpoints;
    }
}
