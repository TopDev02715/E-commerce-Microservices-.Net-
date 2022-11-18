using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Core.Exception;
using ECommerce.Services.Catalogs.Brands.Exceptions.Application;
using ECommerce.Services.Catalogs.Categories.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using ECommerce.Services.Catalogs.Shared.Extensions;
using ECommerce.Services.Catalogs.Suppliers.Exceptions.Application;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct.v1;

public record UpdateProduct(
    long Id,
    string Name,
    decimal Price,
    int RestockThreshold,
    int MaxStockThreshold,
    ProductStatus Status,
    int Width,
    int Height,
    int Depth,
    string Size,
    long CategoryId,
    long SupplierId,
    long BrandId,
    string? Description = null) : ITxUpdateCommand;

internal class UpdateProductValidator : AbstractValidator<UpdateProduct>
{
    public UpdateProductValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}

internal class UpdateProductCommandHandler : ICommandHandler<UpdateProduct>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public UpdateProductCommandHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public async Task<Unit> Handle(UpdateProduct command, CancellationToken cancellationToken)
    {
        Guard.Against.Null(command, nameof(command));

        var product = await _catalogDbContext.FindProductByIdAsync(command.Id);
        Guard.Against.NotFound(product, new ProductCustomNotFoundException(command.Id));

        var category = await _catalogDbContext.FindCategoryAsync(command.CategoryId);
        Guard.Against.NotFound(category, new CategoryCustomNotFoundException(command.CategoryId));

        var brand = await _catalogDbContext.FindBrandAsync(command.BrandId);
        Guard.Against.NotFound(brand, new BrandCustomNotFoundException(command.BrandId));

        var supplier = await _catalogDbContext.FindSupplierByIdAsync(command.SupplierId);
        Guard.Against.NotFound(supplier, new SupplierCustomNotFoundException(command.SupplierId));

        product!.ChangeCategory(command.CategoryId);
        product.ChangeBrand(command.BrandId);
        product.ChangeSupplier(command.SupplierId);

        product.ChangeDescription(command.Description);
        product.ChangeName(command.Name);
        product.ChangePrice(command.Price);
        product.ChangeSize(command.Size);
        product.ChangeStatus(command.Status);
        product.ChangeDimensions(Dimensions.Create(command.Width, command.Height, command.Depth));
        product.ChangeMaxStockThreshold(command.MaxStockThreshold);
        product.ChangeRestockThreshold(command.RestockThreshold);

        await _catalogDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
