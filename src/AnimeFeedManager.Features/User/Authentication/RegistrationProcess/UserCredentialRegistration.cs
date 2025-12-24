using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

public static class UserCredentialRegistration
{
    public static Task<Result<UserRegistrationResult>> TryAddCredential(
        string id,
        ExistentUserGetterById userGetter,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
       return id.ParseAsNonEmpty("Display Name")
            .AsResult()
            .Bind(safeId => userGetter(safeId, cancellationToken))
            .Bind(user => TryToCreate(user, passwordlessClient, cancellationToken));
    }

    private static Task<Result<UserRegistrationResult>> TryToCreate(
        Storage.Stores.User user,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken
    )
    {
        return user switch
        {
            ValidUser u => TryToGetCredential(u, passwordlessClient, cancellationToken),
            _ => Task.FromResult<Result<UserRegistrationResult>>(Error.Create("User doesn't exist in the system")
                .WithOperationName(nameof(UserCredentialRegistration)))
        };
    }

    private static async Task<Result<UserRegistrationResult>> TryToGetCredential(
        ValidUser user,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await passwordlessClient.CreateRegisterTokenAsync(new RegisterOptions(user.UserId, user.Email)
            {
                Aliases = [user.Email]
            }, cancellationToken);

            return new UserRegistrationResult(user.Email, user.UserId, result);
        }
        catch (PasswordlessApiException e)
        {
            return PasswordlessError.FromException(e);
        }
        catch (Exception e)
        {
            return ExceptionError.FromException(e);
        }
    }
}