using BuildingBlocks.Abstractions.CQRS.Event.Internal;
using BuildingBlocks.Core.Persistence.EfCore;
using Store.Services.Customers.Customers.Models;
using Store.Services.Customers.RestockSubscriptions.Models.Write;
using Store.Services.Customers.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Store.Services.Customers.Shared.Data;

public class CustomersDbContext : EfDbContextBase, ICustomersDbContext
{
    public const string DefaultSchema = "customer";

    public CustomersDbContext(DbContextOptions options) : base(options)
    {
    }

    public CustomersDbContext(DbContextOptions options, IDomainEventPublisher domainEventPublisher)
        : base(options, domainEventPublisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension(EfConstants.UuidGenerator);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<RestockSubscription> RestockSubscriptions => Set<RestockSubscription>();
}
