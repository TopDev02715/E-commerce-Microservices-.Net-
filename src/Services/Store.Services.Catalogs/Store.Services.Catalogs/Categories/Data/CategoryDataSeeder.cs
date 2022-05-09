using BuildingBlocks.Abstractions.Persistence;
using Store.Services.Catalogs.Shared.Contracts;

namespace Store.Services.Catalogs.Categories.Data;

public class CategoryDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _dbContext;

    public CategoryDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Categories.AnyAsync())
            return;

        await _dbContext.Categories.AddRangeAsync(new List<Category>
        {
            Category.Create(1, "Electronics", "0001", "All electronic goods"),
            Category.Create(2, "Clothing", "0002", "All clothing goods"),
            Category.Create(3, "Books", "0003", "All books"),
        });
        await _dbContext.SaveChangesAsync();
    }
}
