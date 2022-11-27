using AutoBogus;
using ECommerce.Services.Shared.Identity.Users.Events.v1.Integration;

namespace Tests.Shared.Mocks.Customers.Events;

// beside of auto-generated fields data we can set rules for some fields
public class AutoFakeUserRegisteredV1 : AutoFaker<UserRegisteredV1>
{
    public AutoFakeUserRegisteredV1()
    {
        RuleFor(r => r.Roles, r => new List<string> {"user"});
    }
}
