using BuildingBlocks.Abstractions.Web.Module;

namespace ECommerce.Services.Customers.RestockSubscriptions;

public class RestockSubscriptionsConfigs : IModuleDefinition
{
    public const string Tag = "RestockSubscriptions";

    public const string RestockSubscriptionsUrl =
        $"{CustomersModuleConfiguration.CustomerModulePrefixUri}/restock-subscriptions";

    public IServiceCollection AddModuleServices(IServiceCollection services, IConfiguration configuration)
    {
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }
}
