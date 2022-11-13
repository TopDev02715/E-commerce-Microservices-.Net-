using BuildingBlocks.Abstractions.Web.Module;
using ECommerce.Services.Identity.Shared;
using ECommerce.Services.Identity.Users.Features.GettingUerByEmail;
using ECommerce.Services.Identity.Users.Features.GettingUserById;
using ECommerce.Services.Identity.Users.Features.RegisteringUser;
using ECommerce.Services.Identity.Users.Features.UpdatingUserState;

namespace ECommerce.Services.Identity.Users;

internal class UsersConfigs : IModuleConfiguration
{
    public const string Tag = "Users";
    public const string UsersPrefixUri = $"{SharedModulesConfiguration.IdentityModulePrefixUri}/users";

    public WebApplicationBuilder AddModuleServices(WebApplicationBuilder builder)
    {
        return builder;
    }

    public Task<WebApplication> ConfigureModule(WebApplication app)
    {
        return Task.FromResult(app);
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRegisterNewUserEndpoint();
        endpoints.MapUpdateUserStateEndpoint();
        endpoints.MapGetUserByIdEndpoint();
        endpoints.MapGetUserByEmailEndpoint();

        return endpoints;
    }
}
