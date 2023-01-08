using Bogus;
using ECommerce.Services.Catalogs.Brands;
using ECommerce.Services.Catalogs.Categories;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Suppliers;

namespace ECommerce.Services.Catalogs.IntegrationTests;

public class UnitTest1
{
    [Fact]
    [Trait("Category", "Integration")]
    public void Test1()
    {
        long id = 1;
        // Call for objects that have complex initialization
        var productFaker = new Faker<Product>()
            .CustomInstantiator(faker => Product.Create(
                ProductId.Of(id++),
                Name.Of(faker.Commerce.ProductName()),
                Stock.Of(faker.Random.Int(10, 20), 5, 20),
                ProductStatus.Available,
                Dimensions.Of(faker.Random.Int(10, 50), faker.Random.Int(10, 50), faker.Random.Int(10, 50)),
                Size.Of(faker.PickRandom<string>("M", "S", "L")),
                faker.Random.Enum<ProductColor>(),
                faker.Commerce.ProductDescription(),
                Price.Of(faker.PickRandom<decimal>(100, 200, 500)),
                CategoryId.Of(faker.Random.Long(1, 3)),
                SupplierId.Of(faker.Random.Long(1, 5)),
                BrandId.Of(faker.Random.Long(1, 5))));

        var s = productFaker.Generate(5);
    }
}
