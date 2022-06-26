using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Catalogs.Suppliers.Data;

namespace ECommerce.Services.Catalogs.Suppliers;

internal class Configs : IModuleConfiguration
{
    public IServiceCollection AddModuleServices(
        IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        services.AddScoped<IDataSeeder, SupplierDataSeeder>();

        return services;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
