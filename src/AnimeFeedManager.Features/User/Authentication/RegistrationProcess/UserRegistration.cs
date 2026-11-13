using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

public static class UserRegistration
{
    public static Task<Result<UserRegistrationResult, DomainError>> TryToRegister(
        string displayName,
        string email,
        IPasswordlessClient passwordlessClient,
        UserUpdater updater,
        ExistentUserGetterByEmail userGetter,
        DomainCollectionSender domainPostman,
        CancellationToken cancellationToken
    )
    {
        return VerifyData(userGetter, displayName, email, cancellationToken)
            .CreateUser(passwordlessClient, updater, cancellationToken)
            .SendEvents(domainPostman, cancellationToken);
    }


    private static Task<Result<RegistrationProcessData, DomainError>> VerifyData(
        ExistentUserGetterByEmail userGetter,
        string displayName,
        string email,
        CancellationToken cancellationToken)
    {
        return (email, displayName).AsUserRegistration()
            .Bind(userData => userGetter(userData.Email, cancellationToken)
                .Map(user => new RegistrationProcessData(userData, user,
                    new RegisterOptions(userData.UserId, userData.Email)
                    {
                        DisplayName = userData.DisplayName,
                        Aliases = [userData.Email]
                    })));
    }

    public static Task<Result<UserRegistrationResult, DomainError>> CreateUser(
        this Task<Result<RegistrationProcessData, DomainError>> data,
        IPasswordlessClient passwordlessClient,
        UserUpdater updater,
        CancellationToken cancellationToken)
    {
        return data
            .WithOperationName(nameof(CreateUser))
            .Bind(d => TryToCreate(d, passwordlessClient, updater, cancellationToken)
            .Map(token => new UserRegistrationResult(d.Registration.Email, d.Registration.UserId, token)));
    }

    // Try to register in passwordless; then store the new user
    private static Task<Result<RegisterTokenResponse, DomainError>> TryToCreate(
        RegistrationProcessData data,
        IPasswordlessClient passwordlessClient,
        UserUpdater updater,
        CancellationToken cancellationToken)
    {
        return data.StorageStoredUser switch
        {
            ValidStoredUser user => Task
                .FromResult<Result<RegisterTokenResponse, DomainError>>(new AuthError.UserAlreadyExists(user.Email)),
            _ => RegisterInPasswordless(passwordlessClient, data.Options, cancellationToken)
                .MapError(error => (DomainError)error)
                .Bind(token =>
                    updater(data.Registration.Email, data.Registration.UserId, UserRole.User(), cancellationToken)
                        .Map(_ => token))
        };
    }

    private static async Task<Result<RegisterTokenResponse, AuthError>> RegisterInPasswordless(
        IPasswordlessClient passwordlessClient,
        RegisterOptions options,
        CancellationToken token)
    {
        try
        {
            var result = await passwordlessClient.CreateRegisterTokenAsync(options, token);
            return result;
        }
        catch (PasswordlessApiException e)
        {
            return new AuthError.PasswordlessFailure(e);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return new AuthError.RequestFailed(e);
        }
    }
}