using Ardalis.GuardClauses;
using BuildingBlocks.Abstractions.CQRS.Command;
using BuildingBlocks.Abstractions.CQRS.Query;
using BuildingBlocks.Abstractions.Persistence;
using BuildingBlocks.Core.Exception;
using BuildingBlocks.Security.Jwt;
using Store.Services.Identity.Identity.Exceptions;
using Store.Services.Identity.Identity.Features.GenerateJwtToken;
using Store.Services.Identity.Identity.Features.GenerateRefreshToken;
using Store.Services.Identity.Shared.Exceptions;
using Store.Services.Identity.Shared.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Store.Services.Identity.Identity.Features.Login;

public record Login(string UserNameOrEmail, string Password, bool Remember) :
    ICommand<LoginResponse>, ITxRequest;

internal class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().WithMessage("UserNameOrEmail cannot be empty");
        RuleFor(x => x.Password).NotEmpty().WithMessage("password cannot be empty");
    }
}

internal class LoginHandler : ICommandHandler<Login, LoginResponse>
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<LoginHandler> _logger;
    private readonly IQueryProcessor _queryProcessor;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ICommandProcessor commandProcessor,
        IQueryProcessor queryProcessor,
        IJwtService jwtService,
        IOptions<JwtOptions> jwtOptions,
        SignInManager<ApplicationUser> signInManager,
        ILogger<LoginHandler> logger)
    {
        _userManager = userManager;
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
        _jwtService = jwtService;
        _signInManager = signInManager;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(Login request, CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(Login));

        var identityUser = await _userManager.FindByNameAsync(request.UserNameOrEmail) ??
                           await _userManager.FindByEmailAsync(request.UserNameOrEmail);

        Guard.Against.Null(identityUser, new UserNotFoundException(request.UserNameOrEmail));

        var signinResult = await _signInManager.PasswordSignInAsync(
            request.UserNameOrEmail,
            request.Password,
            request.Remember,
            false);

        if (signinResult.IsNotAllowed)
        {
            if (!await _userManager.IsEmailConfirmedAsync(identityUser))
                throw new EmailNotConfirmedException(identityUser.Email);

            if (!await _userManager.IsPhoneNumberConfirmedAsync(identityUser))
                throw new PhoneNumberNotConfirmedException(identityUser.PhoneNumber);
        }
        else if (signinResult.IsLockedOut)
        {
            throw new UserLockedException(identityUser.Id.ToString());
        }
        else if (signinResult.RequiresTwoFactor)
        {
            throw new RequiresTwoFactorException("Require two factor authentication.");
        }
        else if (!signinResult.Succeeded)
        {
            throw new PasswordIsInvalidException("Password is invalid.");
        }

        var refreshToken =
            (await _commandProcessor.SendAsync(
                new GenerateRefreshTokenCommand {UserId = identityUser.Id},
                cancellationToken)).RefreshToken;

        var accessToken =
            await _commandProcessor.SendAsync(
                new GenerateJwtTokenCommand(identityUser, refreshToken.Token),
                cancellationToken);

        _logger.LogInformation("User with ID: {ID} has been authenticated", identityUser.Id);

        // we can don't return value from command and get token from a short term session in our request with `TokenStorageService`
        return new LoginResponse(identityUser, accessToken, refreshToken.Token);
    }
}

public class LoginResponse

{
    public LoginResponse(ApplicationUser user, string accessToken, string refreshToken)
    {
        UserId = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Username = user.UserName;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
    }

    public Guid UserId { get; }
    public string AccessToken { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Username { get; }
    public string RefreshToken { get; }
}
