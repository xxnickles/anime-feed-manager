using AnimeFeedManager.Features.User.Authentication.Queries;
using AnimeFeedManager.Features.User.Authentication.RegistrationProcess;
using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using AnimeFeedManager.Web.Common;
using Passwordless;

namespace AnimeFeedManager.Web.Features.Security;

internal static class Endpoints
{
    internal static void MapSecurityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/verify-signin", (
                [FromQuery] string token,
                IPasswordlessClient passwordlessClient,
                ILogger<LoginPage> logger,
                CancellationToken cancellationToken) =>
            passwordlessClient.GetLoginInformation(token, cancellationToken)
                .ToJsonResponse(logger));

        group.MapPost("/create-token", (
            [FromForm] RegisterViewModel viewModel,
            ITableClientFactory tableClientFactory,
            IPasswordlessClient passwordlessClient,
            ILogger<Register> logger,
            CancellationToken cancellationToken
        ) => UserRegistration.TryToRegister(
                viewModel.DisplayName,
                viewModel.Email,
                passwordlessClient,
                tableClientFactory.TableStorageUserUpdater(),
                tableClientFactory.TableStorageExistentUserGetterByEmail(),
                cancellationToken)
            .Map(result =>
            {
                viewModel.Token = result.Token.Token;
                viewModel.UserId = result.UserId;
                return viewModel;
            })
            .LogErrors(logger)
            .ToComponentResult(
                model => [RegistrationForm.SuccessFragment(model)],
                error => [RegistrationForm.ErrorFragment(viewModel, error)]
            ));
        
        
        group.MapPost("/add-credential", (
            [FromForm] AddCredentialsViewModel viewModel,
            ITableClientFactory tableClientFactory,
            IPasswordlessClient passwordlessClient,
            ILogger<AddCredential> logger,
            CancellationToken cancellationToken
        ) => UserCredentialRegistration.TryAddCredential(
                viewModel.Id,
                tableClientFactory.TableStorageExistentUserGetterById(),
                passwordlessClient,
                cancellationToken)
            .Map(result =>
            {
                viewModel.Token = result.Token.Token;
                return viewModel;
            })
            .LogErrors(logger)
            .ToComponentResult(
                model => [AddCredentialForm.SuccessFragment(model)],
                error => [AddCredentialForm.ErrorFragment(viewModel, error)]
            ));
    }
}