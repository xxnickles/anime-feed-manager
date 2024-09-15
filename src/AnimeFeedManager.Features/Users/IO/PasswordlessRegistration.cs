using AnimeFeedManager.Features.Users.Types;
using Passwordless;
using Passwordless.Models;

namespace AnimeFeedManager.Features.Users.IO;

public interface IPasswordlessRegistration
{
    Task<Either<DomainError, RegisterTokenResponse>> Register(UserRegistration userData,
        CancellationToken token);
}

public class PasswordlessRegistration : IPasswordlessRegistration
{
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IUserStore _userStore;

    public PasswordlessRegistration(
        IPasswordlessClient passwordlessClient,
        IUserStore userStore)
    {
        _passwordlessClient = passwordlessClient;
        _userStore = userStore;
    }

    public Task<Either<DomainError, RegisterTokenResponse>> Register(UserRegistration userData,
        CancellationToken token)
    {
        return CheckUser(userData, token)
            .BindAsync(options => Registration(options, userData.Email, userData.UserId, token));
    }

    private async Task<Either<DomainError, RegisterTokenResponse>> Registration(RegisterOptions options, Email email, UserId userId,
        CancellationToken token)
    {
        try
        {
            var result = await _passwordlessClient.CreateRegisterTokenAsync(options, token);
            return await _userStore.AddUser(userId, email, token).MapAsync(_ => result);
        }
        catch (PasswordlessApiException e)
        {
            return PasswordlessError.FromException(e);
        }
        catch (Exception ex)
        {
            return ExceptionError.FromException(ex);
        }
    }

    private Task<Either<DomainError, RegisterOptions>> CheckUser(UserRegistration data,
        CancellationToken token)
    {
        return _userStore.CheckEmailExits(data.Email, token)
            .MapAsync(_ => new RegisterOptions(data.UserId, data.Email)
            {
                DisplayName = data.DisplayName,
                Aliases = [data.Email]
            });
    }
}