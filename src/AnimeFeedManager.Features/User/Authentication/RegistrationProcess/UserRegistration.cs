using AnimeFeedManager.Features.User.Storage;
using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.User.RegistrationProcess;

public static class UserRegistration
{
    public static Task<Result<RegistrationProcessData>> VerifyData(
        ExistentUserGetterByEmail userGetter,
        string displayName,
        string email,
        CancellationToken cancellationToken)
    {
        return (displayName, email).AsUserRegistration()
            .Bind(userData => userGetter(userData.Email, cancellationToken)
                .Map(user => new RegistrationProcessData(userData, user,
                    new RegisterOptions(userData.UserId, userData.Email)
                    {
                        DisplayName = userData.DisplayName,
                        Aliases = [userData.Email]
                    })));
    }

    public static Task<Result<UserRegistrationResult>> CreateUser(
        this Task<Result<RegistrationProcessData>> data,
        IPasswordlessClient passwordlessClient,
        UserUpdater updater,
        CancellationToken cancellationToken)
    {
        return data.Bind(d => TryToCreate(d, passwordlessClient, updater, cancellationToken)
            .Map(token => new UserRegistrationResult(d.Registration.Email, d.Registration.UserId, token)));
    }

    // Try to register in passwordless; then store the new user
    private static Task<Result<RegisterTokenResponse>> TryToCreate(
        RegistrationProcessData data,
        IPasswordlessClient passwordlessClient,
        UserUpdater updater,
        CancellationToken cancellationToken)
    {
        return data.StorageUser switch
        {
            ValidUser user => Task.FromResult(Result<RegisterTokenResponse>.Failure(
                new OperationError(nameof(CreateUser), $"Email '{user.Email}' already exist in the system"))),
            _ => RegisterInPasswordless(passwordlessClient, data.Options, cancellationToken)
                .Bind(token =>
                    updater(data.Registration.Email, data.Registration.UserId, UserRole.User(), cancellationToken)
                        .Map(_ => token))
        };
    }

    private static async Task<Result<RegisterTokenResponse>> RegisterInPasswordless(
        IPasswordlessClient passwordlessClient,
        RegisterOptions options,
        CancellationToken token)
    {
        try
        {
            var result = await passwordlessClient.CreateRegisterTokenAsync(options, token);
            return Result<RegisterTokenResponse>.Success(result);
        }
        catch (PasswordlessApiException e)
        {
            return Result<RegisterTokenResponse>.Failure(PasswordlessError.FromException(e));
        }
        catch (Exception e)
        {
            return Result<RegisterTokenResponse>.Failure(ExceptionError.FromException(e));
        }
    }
}