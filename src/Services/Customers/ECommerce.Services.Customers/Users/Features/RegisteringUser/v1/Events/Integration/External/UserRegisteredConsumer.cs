using BuildingBlocks.Abstractions.CQRS.Commands;
using ECommerce.Services.Customers.Customers.Features.CreatingCustomer.v1;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;
using MassTransit;

namespace ECommerce.Services.Customers.Users.Features.RegisteringUser.v1.Events.Integration.External;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly ICommandProcessor _commandProcessor;

    public UserRegisteredConsumer(ICommandProcessor commandProcessor)
    {
        _commandProcessor = commandProcessor;
    }

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var userRegistered = context.Message;
        if (userRegistered.Roles is null || !userRegistered.Roles.Contains(CustomersConstants.Role.User))
            return;

        await _commandProcessor.SendAsync(new CreateCustomer(userRegistered.Email));
    }
}
