using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Domain.ValueObjects;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Core.IdsGenerator;
using ECommerce.Services.Customers.Customers.Exceptions.Application;
using ECommerce.Services.Customers.Customers.ValueObjects;
using ECommerce.Services.Customers.Products;
using ECommerce.Services.Customers.Products.Exceptions;
using ECommerce.Services.Customers.RestockSubscriptions.Dtos.v1;
using ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1.Exceptions;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Write;
using ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;
using ECommerce.Services.Customers.Shared.Clients.Catalogs;
using ECommerce.Services.Customers.Shared.Data;
using ECommerce.Services.Customers.Shared.Extensions;
using FluentValidation;

namespace ECommerce.Services.Customers.RestockSubscriptions.Features.CreatingRestockSubscription.v1;

public record CreateRestockSubscription(long CustomerId, long ProductId, string Email)
    : ITxCreateCommand<CreateRestockSubscriptionResponse>
{
    public long Id { get; init; } = SnowFlakIdGenerator.NewId();
}

internal class CreateRestockSubscriptionValidator : AbstractValidator<CreateRestockSubscription>
{
    public CreateRestockSubscriptionValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();

        RuleFor(x => x.ProductId)
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

internal class CreateRestockSubscriptionHandler
    : ICommandHandler<CreateRestockSubscription, CreateRestockSubscriptionResponse>
{
    private readonly CustomersDbContext _customersDbContext;
    private readonly ICatalogApiClient _catalogApiClient;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateRestockSubscriptionHandler> _logger;

    public CreateRestockSubscriptionHandler(
        CustomersDbContext customersDbContext,
        ICatalogApiClient catalogApiClient,
        IMapper mapper,
        ILogger<CreateRestockSubscriptionHandler> logger)
    {
        _customersDbContext = customersDbContext;
        _catalogApiClient = catalogApiClient;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<CreateRestockSubscriptionResponse> Handle(
        CreateRestockSubscription request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var existsCustomer = await _customersDbContext.ExistsCustomerByIdAsync(CustomerId.Of(request.CustomerId));
        Guard.Against.NotExists(existsCustomer, new CustomerNotFoundException(request.CustomerId));

        var product = (await _catalogApiClient.GetProductByIdAsync(request.ProductId, cancellationToken))?.Product;
        Guard.Against.NotFound(product, new ProductNotFoundException(request.ProductId));

        if (product!.AvailableStock > 0)
            throw new ProductHaveStockException(product.Id, product.AvailableStock, product.Name);

        var alreadySubscribed = _customersDbContext.RestockSubscriptions
            .Any(x => x.Email.Value == request.Email &&
                      x.ProductInformation.Id == request.ProductId &&
                      x.Processed == false);

        if (alreadySubscribed)
            throw new ProductAlreadySubscribedException(product.Id, product.Name);

        var restockSubscription =
            RestockSubscription.Create(
                RestockSubscriptionId.Of(request.Id),
                CustomerId.Of(request.CustomerId),
                ProductInformation.Of(ProductId.Of(product.Id), product.Name),
                Email.Of(request.Email));

        await _customersDbContext.AddAsync(restockSubscription, cancellationToken);

        await _customersDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RestockSubscription with id '{@InternalCommandId}' saved successfully",
            restockSubscription.Id);

        var restockSubscriptionDto = _mapper.Map<RestockSubscriptionDto>(restockSubscription);

        return new CreateRestockSubscriptionResponse(restockSubscriptionDto.Id);
    }
}
