using System.Diagnostics;
using AnimeFeedManager.Features.Auth.Storage;
using AnimeFeedManager.Shared;
using Passwordless;

namespace AnimeFeedManager.Features.Auth.Registration;

/// <summary>
/// Adds another passkey to an existing user: look the account up by id, then mint a fresh
/// Passwordless register-token the browser uses to create the new credential.
/// </summary>
public static class UserCredentialRegistration
{
    private static readonly ActivitySource Source = new(Telemetry.AuthSource);

    public static async Task<Result<UserRegistrationResult>> TryAddCredential(
        string id,
        UserAccountGetter getById,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
        using var activity = Source.StartActivity("Auth.AddCredential");
        return await id.ParseAsNonEmpty(nameof(id)).AsResult()
            .WithOperationName(nameof(TryAddCredential))
            .Bind(safeId => getById(safeId, cancellationToken))
            .Bind(stored => stored switch
            {
                ValidStoredUser user => CreateToken(user, passwordlessClient, cancellationToken),
                _ => Task.FromResult<Result<UserRegistrationResult>>(
                    Error.Create("User doesn't exist in the system"))
            })
            .MarkActivityErroredOnError();
    }

    private static async Task<Result<UserRegistrationResult>> CreateToken(
        ValidStoredUser user,
        IPasswordlessClient passwordlessClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await passwordlessClient.CreateRegisterTokenAsync(
                new RegisterOptions(user.UserId, user.Email) { Aliases = [user.Email] },
                cancellationToken);
            return new UserRegistrationResult(user.Email, user.UserId, token);
        }
        catch (PasswordlessApiException e)
        {
            return PasswordlessError.FromException(e);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return ExceptionError.FromException(e);
        }
    }
}
