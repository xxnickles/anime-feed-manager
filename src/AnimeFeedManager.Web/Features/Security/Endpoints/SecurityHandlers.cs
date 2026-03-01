using System.Security.Claims;
using AnimeFeedManager.Shared.Types;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.User.Authentication.Queries;
using AnimeFeedManager.Features.User.Authentication.RegistrationProcess;
using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using AnimeFeedManager.Web.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Passwordless;

namespace AnimeFeedManager.Web.Features.Security.Endpoints;

internal static class SecurityHandlers
{
    internal static Task<IResult> VerifySignIn(
        [FromQuery] string token,
        IPasswordlessClient passwordlessClient,
        ILogger<LoginPage> logger,
        CancellationToken cancellationToken)
    {
        return passwordlessClient.GetLoginInformation(token, cancellationToken)
            .ToJsonResponse(logger);
    }

    internal static Task<RazorComponentResult> CreateToken(
        [FromForm] RegisterViewModel viewModel,
        ITableClientFactory tableClientFactory,
        IPasswordlessClient passwordlessClient,
        IDomainPostman domainPostman,
        ILogger<RegisterPage> logger,
        CancellationToken cancellationToken)
    {
        return UserRegistration.TryToRegister(
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
            .FlushLogs(logger)
            .ToComponentResult(
                model => [RegistrationForm.SuccessFragment(model)],
                error => [RegistrationForm.ErrorFragment(viewModel, error)]
            );
    }

    internal static async Task<IResult> LoginUser(
        [FromForm] LoginViewModel viewModel,
        HttpContext httpContext,
        ITableClientFactory tableClientFactory,
        ILogger<LoginPage> logger,
        CancellationToken cancellationToken)
    {
        return await viewModel.Id.ParseAsNonEmpty(nameof(viewModel.Id))
            .AsResult()
            .Bind(id => Users.GetById(
                tableClientFactory.TableStorageExistentUserGetterById(),
                id,
                cancellationToken))
            .Bind(TryToCreatePrincipal)
            .FlushLogs(logger)
            .Tap(principal => httpContext.SignInAsync(
                principal,
                new AuthenticationProperties { IsPersistent = true }))
            .MatchToValue<ClaimsPrincipal, IResult>(
                _ => Results.LocalRedirect(
                    string.IsNullOrWhiteSpace(viewModel.ReturnUrl) ? "/" : viewModel.ReturnUrl),
                error => new[] { LoginForm.ErrorFragment(viewModel, error) }.AggregateComponents());
    }

    private static Result<ClaimsPrincipal> TryToCreatePrincipal(User user)
    {
        return user switch
        {
            ValidUser vu => CreatePrincipal(vu),
            _ => Error.Create("Provided user doesn't exist in the system")
        };
    }

    private static ClaimsPrincipal CreatePrincipal(ValidUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email),
            new(CustomClaimTypes.Sub, user.UserId),
            new(ClaimTypes.Role, user.Role)
        };

        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    internal static Task<RazorComponentResult> AddCredential([FromForm] AddCredentialsViewModel viewModel,
        ITableClientFactory tableClientFactory,
        IPasswordlessClient passwordlessClient,
        ILogger<AddCredentialPage> logger,
        CancellationToken cancellationToken)
    {
        return UserCredentialRegistration.TryAddCredential(
                viewModel.Id,
                tableClientFactory.TableStorageExistentUserGetterById(),
                passwordlessClient,
                cancellationToken)
            .Map(result =>
            {
                viewModel.Token = result.Token.Token;
                return viewModel;
            })
            .FlushLogs(logger)
            .ToComponentResult(
                model => [AddCredentialForm.SuccessFragment(model)],
                error => [AddCredentialForm.ErrorFragment(viewModel, error)]
            );
    }
}