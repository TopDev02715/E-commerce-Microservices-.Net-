using BuildingBlocks.Resiliency.Extensions;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Clients.Identity;

namespace ECommerce.Services.Customers.Shared.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomHttpClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<IdentityApiClientOptions>().Bind(configuration.GetSection(nameof(IdentityApiClientOptions)))
            .ValidateDataAnnotations();

        services.AddOptions<CatalogsApiClientOptions>().Bind(configuration.GetSection(nameof(CatalogsApiClientOptions)))
            .ValidateDataAnnotations();

        services.AddHttpApiClient<ICatalogApiClient, CatalogApiClient>();
        services.AddHttpApiClient<IIdentityApiClient, IdentityApiClient>();

        return services;
    }
}
