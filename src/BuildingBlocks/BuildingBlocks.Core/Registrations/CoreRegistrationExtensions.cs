using System.Reflection;
using BuildingBlocks.Abstractions.Core;
using BuildingBlocks.Abstractions.CQRS.Event;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Abstractions.Serialization;
using BuildingBlocks.Abstractions.Types;
using BuildingBlocks.Core.CQRS.Event;
using BuildingBlocks.Core.Extensions;
using BuildingBlocks.Core.Extensions.ServiceCollection;
using BuildingBlocks.Core.IdsGenerator;
using BuildingBlocks.Core.Serialization;
using BuildingBlocks.Core.Types;
using Microsoft.Extensions.Configuration;
using Scrutor;

namespace BuildingBlocks.Core.Registrations;

public static class CoreRegistrationExtensions
{
    public static IServiceCollection AddCore(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assembliesToScan)
    {
        var systemInfo = SystemInfo.New();

        services.AddSingleton<ISystemInfo>(systemInfo);
        services.AddSingleton(systemInfo);
        services.AddSingleton<IExclusiveLock, ExclusiveLock>();

        services.AddTransient<IAggregatesDomainEventsRequestStore, AggregatesDomainEventsStore>();

        services.AddHttpContextAccessor();

        AddDefaultSerializer(services);

        AddMessagingCore(services, configuration, ServiceLifetime.Transient, assembliesToScan);

        RegisterEventMappers(services, assembliesToScan);

        switch (configuration["IdGenerator:Type"])
        {
            case "Guid":
                services.AddSingleton<IIdGenerator<Guid>, GuidIdGenerator>();
                break;
            default:
                services.AddSingleton<IIdGenerator<long>, SnowFlakIdGenerator>();
                break;
        }

        return services;
    }

    private static void RegisterEventMappers(IServiceCollection services, params Assembly[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIntegrationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IIDomainNotificationEventMapper)), false)
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    private static void AddMessagingCore(
        this IServiceCollection services,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime = ServiceLifetime.Transient,
        params Assembly[] assembliesToScan)
    {
        AddMessagingMediator(services, serviceLifetime, assembliesToScan);
    }

    private static void AddMessagingMediator(
        IServiceCollection services,
        ServiceLifetime serviceLifetime,
        Assembly[] assembliesToScan)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembliesToScan.Any() ? assembliesToScan : AppDomain.CurrentDomain.GetAssemblies())
            .AddClasses(classes => classes.AssignableTo(typeof(IMessageHandler<>)))
            .UsingRegistrationStrategy(RegistrationStrategy.Append)
            .AsClosedTypeOf(typeof(IMessageHandler<>))
            .AsSelf()
            .WithLifetime(serviceLifetime));
    }

    private static void AddDefaultSerializer(
        IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        services.Add<ISerializer, DefaultSerializer>(lifetime);
        services.Add<IMessageSerializer, DefaultMessageSerializer>(lifetime);
    }
}
