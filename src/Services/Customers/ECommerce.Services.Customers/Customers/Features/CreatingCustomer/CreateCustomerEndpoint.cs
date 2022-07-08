using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Commands;
using BuildingBlocks.Abstractions.Web.MinimalApi;
using SerilogTimings;

namespace ECommerce.Services.Customers.Customers.Features.CreatingCustomer;

public class CreateCustomerEndpoint : IMinimalEndpointConfiguration
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost(CustomersConfigs.CustomersPrefixUri, CreateCustomer)
            .AllowAnonymous()
            .WithTags(CustomersConfigs.Tag)
            .Produces<CreateCustomerResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateCustomer")
            .WithDisplayName("Register New Customer.");

        return builder;
    }

    private static async Task<IResult> CreateCustomer(
        CreateCustomerRequest request,
        ICommandProcessor commandProcessor,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var command = new CreateCustomer(request.Email);

        // https://github.com/serilog/serilog/wiki/Enrichment
        // https://dotnetdocs.ir/Post/34/categorizing-logs-with-serilog-in-aspnet-core
        using (Serilog.Context.LogContext.PushProperty("Endpoint", nameof(CreateCustomerEndpoint)))
        using (Serilog.Context.LogContext.PushProperty("CustomerId", command.Id))
        {
            var result = await commandProcessor.SendAsync(command, cancellationToken);

            return Results.Created($"{CustomersConfigs.CustomersPrefixUri}/{result.CustomerId}", result);
        }
    }
}
