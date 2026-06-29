using System.Security.Claims;
using AnimeFeedManager.Features.Auth.Login;
using AnimeFeedManager.Features.Auth.Registration;
using AnimeFeedManager.Features.Auth.Storage;
using AnimeFeedManager.Infrastructure.Cosmos;
using AnimeFeedManager.Infrastructure.Eventing;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Errors;
using AnimeFeedManager.Shared.Results.Static;
using AnimeFeedManager.Shared.Types;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Passwordless;

namespace AnimeFeedManager.Web.Features.Security.Endpoints;

/// <summary>
/// Passkey auth endpoints. htmx posts the forms here; the handlers compose the Auth domain flows
/// (delegates built locally from <see cref="ICosmosContainerFactory"/> / <see cref="EventBus"/>),
/// flush the accumulated result logs, then render a form fragment — or, for login, issue the auth
/// cookie and redirect. Antiforgery stays on (forms carry the token via <c>AntiforgeryToken</c>).
/// </summary>
internal static class SecurityEndpoints
{
    internal static IEndpointRouteBuilder MapSecurityEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/create-token", CreateToken);
        routes.MapPost("/add-credential", AddCredential);
        routes.MapPost("/login", LoginUser);
        return routes;
    }

    private static Task<IResult> CreateToken(
        [FromForm] RegisterViewModel viewModel,
        ICosmosContainerFactory containerFactory,
        IPasswordlessClient passwordlessClient,
        EventBus eventBus,
        ILogger<RegistrationForm> logger,
        CancellationToken cancellationToken) =>
        UserRegistration.TryToRegister(
                viewModel.DisplayName,
                viewModel.Email,
                passwordlessClient,
                containerFactory.UserAccountUpserterHandler(),
                containerFactory.UserByEmailGetterHandler(),
                containerFactory.UsersIndexRegistrarHandler(),
                eventBus.Publish,
                cancellationToken)
            .FlushLogs(logger)
            .MatchToValue<UserRegistrationResult, IResult>(
                result =>
                {
                    viewModel.Token = result.Token.Token;
                    viewModel.UserId = result.UserId;
                    return Render<RegistrationForm>(viewModel);
                },
                error => Render<RegistrationForm>(viewModel, error));

    private static Task<IResult> AddCredential(
        [FromForm] AddCredentialsViewModel viewModel,
        ICosmosContainerFactory containerFactory,
        IPasswordlessClient passwordlessClient,
        ILogger<AddCredentialForm> logger,
        CancellationToken cancellationToken) =>
        UserCredentialRegistration.TryAddCredential(
                viewModel.Id,
                containerFactory.UserAccountGetterHandler(),
                passwordlessClient,
                cancellationToken)
            .FlushLogs(logger)
            .MatchToValue<UserRegistrationResult, IResult>(
                result =>
                {
                    viewModel.Token = result.Token.Token;
                    return Render<AddCredentialForm>(viewModel);
                },
                error => Render<AddCredentialForm>(viewModel, error));

    // Verifies the Passwordless token SERVER-SIDE and derives the user id from the verified result —
    // the client never supplies an identity, so a forged/arbitrary id can't mint a session.
    private static Task<IResult> LoginUser(
        [FromForm] LoginViewModel viewModel,
        HttpContext httpContext,
        IPasswordlessClient passwordlessClient,
        ICosmosContainerFactory containerFactory,
        ILogger<LoginForm> logger,
        CancellationToken cancellationToken) =>
        LoginVerification.VerifyUser(passwordlessClient, viewModel.Token, cancellationToken)
            .Bind(verified => verified.UserId.ParseAsNonEmpty("UserId").AsResult())
            .Bind(userId => containerFactory.UserAccountGetterHandler()(userId, cancellationToken))
            .Bind(TryToCreatePrincipal)
            .FlushLogs(logger)
            .Tap(principal => httpContext.SignInAsync(
                principal, new AuthenticationProperties { IsPersistent = true }))
            .MatchToValue<ClaimsPrincipal, IResult>(
                _ => Results.LocalRedirect(
                    string.IsNullOrWhiteSpace(viewModel.ReturnUrl) ? "/" : viewModel.ReturnUrl),
                error => Render<LoginForm>(viewModel, error));

    private static Result<ClaimsPrincipal> TryToCreatePrincipal(StoredUser user) =>
        user switch
        {
            ValidStoredUser valid => CreatePrincipal(valid),
            _ => Error.Create("Provided user doesn't exist in the system")
        };

    private static ClaimsPrincipal CreatePrincipal(ValidStoredUser storedUser)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, storedUser.Email),
            new(ClaimTypes.Email, storedUser.Email),
            new(CustomClaimTypes.Sub, storedUser.UserId),
            new(ClaimTypes.Role, storedUser.Role)
        ];

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    private static IResult Render<TForm>(object model, DomainError? error = null) where TForm : IComponent =>
        new RazorComponentResult<TForm>(new Dictionary<string, object?>
        {
            ["Model"] = model,
            ["Error"] = error
        });
}
