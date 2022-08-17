using BuildingBlocks.Abstractions.CQRS.Queries;

namespace ECommerce.Services.Identity.Identity.Features.GettingClaims;

public class GetClaimsQuery : IQuery<GetClaimsQueryResult>
{
}

public class GetClaimsQueryHandler : IQueryHandler<GetClaimsQuery, GetClaimsQueryResult>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetClaimsQueryHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<GetClaimsQueryResult> Handle(GetClaimsQuery request, CancellationToken cancellationToken)
    {
        var claims = _httpContextAccessor.HttpContext?.User.Claims.Select(x => new ClaimDto
        {
            Type = x.Type, Value = x.Value
        });

        return Task.FromResult(new GetClaimsQueryResult(claims));
    }
}
