using System.Security.Claims;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.User.Authentication.LoginProcess;
using AnimeFeedManager.Features.User.Authentication.Queries;
using AnimeFeedManager.Features.User.Authentication.RegistrationProcess;
using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Web.Htmx.Static;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Passwordless;

namespace AnimeFeedManager.Web.Features.Security.Endpoints;

internal static class SecurityHandlers
{
    private static readonly ActivitySource WebSource = new(Telemetry.WebSecuritySource);
    private static readonly ActivitySource AuthSource = new(Telemetry.UserAuthenticationSource);

    internal static async Task<RazorComponentResult> CreateToken(
        [FromForm] RegisterViewModel viewModel,
        ITableClientFactory tableClientFactory,
        IPasswordlessClient passwordlessClient,
        IDomainPostman domainPostman,
        ILogger<RegisterPage> logger,
        CancellationToken cancellationToken)
    {
        using var webActivity = WebSource.StartActivity("Web.Security");
        using var authActivity = AuthSource.StartActivity("User.Authentication");
        return await UserRegistration.TryToRegister(
                viewModel.DisplayName,
                viewModel.Email,
                passwordlessClient,
                tableClientFactory.TableStorageUserUpdater(),
                tableClientFactory.TableStorageExistentUserGetterByEmail(),
                domainPostman.SendMessages,
                cancellationToken)
            .Map(result =>
            {
                viewModel.Token = result.Token.Token;
                viewModel.UserId = result.UserId;
                return viewModel;
            })
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                model => [RegistrationForm.SuccessFragment(model)],
                error => [RegistrationForm.ErrorFragment(viewModel, error)]
            );
    }

    internal static async Task<IResult> LoginUser(
        [FromForm] LoginViewModel viewModel,
        HttpContext httpContext,
        IPasswordlessClient passwordlessClient,
        ITableClientFactory tableClientFactory,
        ILogger<LoginPage> logger,
        CancellationToken cancellationToken)
    {
        using var webActivity = WebSource.StartActivity("Web.Security");
        using var authActivity = AuthSource.StartActivity("User.Authentication");
        return await LoginVerification.VerifyUser(passwordlessClient, viewModel.Token, cancellationToken)
            .Bind(verified => verified.UserId.ParseAsNonEmpty("UserId").AsResult())
            .Bind(id => Users.GetById(
                tableClientFactory.TableStorageExistentUserGetterById(),
                id,
                cancellationToken))
            .Bind(TryToCreatePrincipal)
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .Tap(principal => httpContext.SignInAsync(
                principal,
                new AuthenticationProperties { IsPersistent = true }))
            .MatchToValue<ClaimsPrincipal, IResult>(
                _ => Redirect(httpContext, LocalReturnUrl(viewModel.ReturnUrl)),
                error => new[] { LoginForm.ErrorFragment(viewModel, error) }.AggregateComponents());
    }

    // The CancellationToken keeps the signature off the (HttpContext) => Task RequestDelegate shape,
    // so it binds as a route handler whose IResult is written (ASP0016).
    internal static async Task<IResult> Logout(
        [FromQuery] string? returnUrl,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(httpContext, LocalReturnUrl(returnUrl));
    }

    // Auth-state transitions full-reload (HX-Redirect) for htmx so the shell rebuilds with the
    // now-authenticated nav; a normal browser request gets a local redirect.
    private static IResult Redirect(HttpContext httpContext, string localPath)
    {
        if (!httpContext.IsHtmxRequest())
            return Results.LocalRedirect(localPath);

        httpContext.Response.HxRedirect(localPath);
        return Results.Ok();
    }

    // Guards against open redirects: only same-site absolute paths are honoured.
    private static string LocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//")
            ? returnUrl
            : "/";

    private static Result<ClaimsPrincipal> TryToCreatePrincipal(StoredUser user)
    {
        return user switch
        {
            ValidStoredUser vu => CreatePrincipal(vu),
            _ => Error.Create("Provided user doesn't exist in the system")
        };
    }

    private static ClaimsPrincipal CreatePrincipal(ValidStoredUser storedUser)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, storedUser.Email),
            new(ClaimTypes.Email, storedUser.Email),
            new(CustomClaimTypes.Sub, storedUser.UserId),
            new(ClaimTypes.Role, storedUser.Role)
        };

        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    internal static async Task<RazorComponentResult> AddCredential([FromForm] AddCredentialsViewModel viewModel,
        ITableClientFactory tableClientFactory,
        IPasswordlessClient passwordlessClient,
        ILogger<AddCredentialPage> logger,
        CancellationToken cancellationToken)
    {
        using var webActivity = WebSource.StartActivity("Web.Security");
        using var authActivity = AuthSource.StartActivity("User.Authentication");
        return await UserCredentialRegistration.TryAddCredential(
                viewModel.Id,
                tableClientFactory.TableStorageExistentUserGetterById(),
                passwordlessClient,
                cancellationToken)
            .Map(result =>
            {
                viewModel.Token = result.Token.Token;
                return viewModel;
            })
            .MarkActivityErroredOnError()
            .FlushLogs(logger)
            .ToComponentResult(
                model => [AddCredentialForm.SuccessFragment(model)],
                error => [AddCredentialForm.ErrorFragment(viewModel, error)]
            );
    }
}