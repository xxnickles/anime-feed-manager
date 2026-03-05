using AnimeFeedManager.Features.User.Authentication.Storage.Stores;
using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.RegistrationProcess;

public static class UserCredentialRegistration
{
    /// <summary>
    ///  Creates a new Passwordless credential for an existent user
    /// </summary>
    /// <param name="id"></param>
    /// <param name="userGetter"></param>
    /// <param name="passwordlessClient"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static Task<Result<UserRegistrationResult>> TryAddCredential(
        string id,
        ExistentUserGetterById userGetter,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
       return id.ParseAsNonEmpty("Display Name")
            .AsResult()
            .WithOperationName(nameof(UserCredentialRegistration))
            .AddLog((logger) => logger.LogInformation("Trying to add credential for user {id}", id))
            .Bind(safeId => userGetter(safeId, cancellationToken))
            .Bind(user => TryToCreate(user, passwordlessClient, cancellationToken));
    }

    private static Task<Result<UserRegistrationResult>> TryToCreate(
        StoredUser storedUser,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken
    )
    {
        return storedUser switch
        {
            ValidStoredUser u => TryToGetCredential(u, passwordlessClient, cancellationToken),
            _ => Task.FromResult<Result<UserRegistrationResult>>(Error.Create("User doesn't exist in the system"))
        };
    }

    private static async Task<Result<UserRegistrationResult>> TryToGetCredential(
        ValidStoredUser storedUser,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await passwordlessClient.CreateRegisterTokenAsync(new RegisterOptions(storedUser.UserId, storedUser.Email)
            {
                Aliases = [storedUser.Email]
            }, cancellationToken);

            return new UserRegistrationResult(storedUser.Email, storedUser.UserId, result);
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