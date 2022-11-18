using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.CQRS.Events.Internal;
using BuildingBlocks.Core.CQRS.Events.Internal;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.Shared.Data;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.DeletingRestockSubscription.v1;

public record RestockSubscriptionDeleted(RestockSubscription RestockSubscription) : DomainEvent;

internal class RestockSubscriptionDeletedHandler : IDomainEventHandler<RestockSubscriptionDeleted>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IMapper _mapper;
    private readonly CustomersDbContext _customersDbContext;

    public RestockSubscriptionDeletedHandler(
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CustomersDbContext customersDbContext)
    {
        _commandProcessor = commandProcessor;
        _mapper = mapper;
        _customersDbContext = customersDbContext;
    }

    public async Task Handle(RestockSubscriptionDeleted notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // var isDeleted = (bool)_customersDbContext.Entry(notification.RestockSubscription)
        //     .Property("IsDeleted")
        //     .CurrentValue!;

        // https://github.com/kgrzybek/modular-monolith-with-ddd#38-internal-processing
        await _commandProcessor.SendAsync(
            new UpdateMongoRestockSubscriptionReadModel(notification.RestockSubscription, true),
            cancellationToken);
    }
}
