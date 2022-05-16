using BuildingBlocks.Core.CQRS.Event.Internal;
using ECommerce.Services.Catalogs.Products.ValueObjects;

namespace ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductStockDebited(ProductId ProductId, Stock NewStock, int DebitedQuantity) : DomainEvent;
