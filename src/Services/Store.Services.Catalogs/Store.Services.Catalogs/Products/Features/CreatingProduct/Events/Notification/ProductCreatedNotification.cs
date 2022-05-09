using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Abstractions.Messaging;
using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Products.Features.CreatingProduct.Events.Domain;

namespace Store.Services.Catalogs.Products.Features.CreatingProduct.Events.Notification;

public record ProductCreatedNotification
    (ProductCreated DomainEvent) : DomainNotificationEventWrapper<ProductCreated>(DomainEvent)
{
    public long Id => DomainEvent.Product.Id;
    public string Name => DomainEvent.Product.Name;
    public long CategoryId => DomainEvent.Product.CategoryId.Value;
    public string? CategoryName => DomainEvent.Product.Category?.Name;
    public int Stock => DomainEvent.Product.Stock.Available;
}

internal class ProductCreatedNotificationHandler : IDomainNotificationEventHandler<ProductCreatedNotification>
{
    private readonly IBus _bus;

    public ProductCreatedNotificationHandler(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
    {
        // We could publish integration event to bus here
        // await _bus.PublishAsync(
        //     new Store.Services.Shared.Catalogs.Products.Events.Integration.ProductCreated(
        //         notification.Id,
        //         notification.Name,
        //         notification.Stock,
        //         notification.CategoryName ?? "",
        //         notification.Stock),
        //     null,
        //     cancellationToken);

        return;
    }
}
