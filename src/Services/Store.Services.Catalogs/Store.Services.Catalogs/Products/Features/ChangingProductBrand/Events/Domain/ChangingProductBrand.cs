using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.CQRS.Event.Internal;
using Store.Services.Catalogs.Brands;
using Store.Services.Catalogs.Shared.Contracts;
using Store.Services.Catalogs.Shared.Extensions;

namespace Store.Services.Catalogs.Products.Features.ChangingProductBrand.Events.Domain;

internal record ChangingProductBrand(BrandId BrandId) : DomainEvent;

internal class ChangingProductBrandValidationHandler :
    IDomainEventHandler<ChangingProductBrand>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ChangingProductBrandValidationHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task Handle(ChangingProductBrand notification, CancellationToken cancellationToken)
    {
        // Handling some validations
        Guard.Against.Null(notification, nameof(notification));
        Guard.Against.NegativeOrZero(notification.BrandId, nameof(notification.BrandId));
        Guard.Against.ExistsBrand(
            await _catalogDbContext.BrandExistsAsync(notification.BrandId, cancellationToken: cancellationToken),
            notification.BrandId);
    }
}
