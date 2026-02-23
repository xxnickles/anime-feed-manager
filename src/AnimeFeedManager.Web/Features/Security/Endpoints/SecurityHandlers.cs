using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.User.Authentication.Queries;
using AnimeFeedManager.Features.User.Authentication.RegistrationProcess;
using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using AnimeFeedManager.Web.Common;
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