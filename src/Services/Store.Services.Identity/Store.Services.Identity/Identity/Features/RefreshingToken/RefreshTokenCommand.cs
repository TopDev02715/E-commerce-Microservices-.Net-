using System.Security.Claims;
using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Security.Jwt;
using Store.Services.Identity.Identity.Exceptions;
using Store.Services.Identity.Identity.Features.GenerateJwtToken;
using Store.Services.Identity.Identity.Features.GenerateRefreshToken;
using Store.Services.Identity.Shared.Exceptions;
using Store.Services.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Store.Services.Identity.Identity.Features.RefreshingToken;

public record RefreshTokenCommand(string AccessTokenData, string RefreshTokenData) : ICommand<RefreshTokenResult>;

internal class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(v => v.AccessTokenData)
            .NotEmpty();

        RuleFor(v => v.RefreshTokenData)
            .NotEmpty();
    }
}

internal class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly UserManager<ApplicationUser> _userManager;

    public RefreshTokenHandler(
        IJwtService jwtService,
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor)
    {
        _jwtService = jwtService;
        _userManager = userManager;
        _commandProcessor = commandProcessor;
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(RefreshTokenCommand));

        // invalid token/signing key was passed and we can't extract user claims
        var userClaimsPrincipal = _jwtService.ValidateToken(request.AccessTokenData);

        if (userClaimsPrincipal is null)
            throw new InvalidTokenException(userClaimsPrincipal);

        var userId = userClaimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.NameId);

        var identityUser = await _userManager.FindByIdAsync(userId);

        if (identityUser == null)
            throw new UserNotFoundException(userId);

        var refreshToken =
            (await _commandProcessor.SendAsync(
                new GenerateRefreshTokenCommand { UserId = identityUser.Id, Token = request.RefreshTokenData },
                cancellationToken)).RefreshToken;

        var jsonWebToken =
            (await _commandProcessor.SendAsync(
                new GenerateJwtTokenCommand(identityUser, refreshToken.Token), cancellationToken)).JsonWebToken;

        return new RefreshTokenResult(identityUser, jsonWebToken, refreshToken.Token);
    }
}

public record RefreshTokenResult
{
    public RefreshTokenResult(ApplicationUser user, JsonWebToken jwtToken, string refreshToken)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Username = user.UserName;
        JsonWebToken = jwtToken;
        RefreshToken = refreshToken;
    }

    public JsonWebToken JsonWebToken { get; }
    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Username { get; }
    public string RefreshToken { get; }
}
