using AnimeFeedManager.Old.Common.Domain.Errors;
using AnimeFeedManager.Old.Features.Users.Types;
using Passwordless;

namespace AnimeFeedManager.Old.Features.Users.IO;

public interface IPasswordlessLogin
{
    Task<Either<DomainError, VerifiedUser>> GetLoginInformation(string token,
        CancellationToken cancellationToken);
    
    Task<Either<DomainError, string>> GetUserInfo(UserId userId,
        CancellationToken cancellationToken);
}

public class PasswordlessLogin : IPasswordlessLogin
{
    private readonly IPasswordlessClient _passwordlessClient;
    private readonly IUserRoleGetter _userRoleGetter;

    public PasswordlessLogin(
        IPasswordlessClient passwordlessClient,
        IUserRoleGetter userRoleGetter)
    {
        _passwordlessClient = passwordlessClient;
        _userRoleGetter = userRoleGetter;
    }

    public Task<Either<DomainError, VerifiedUser>> GetLoginInformation(string token,
        CancellationToken cancellationToken)
    {
        return GetUser(token, cancellationToken);
    }

    public Task<Either<DomainError, string>> GetUserInfo(UserId userId, CancellationToken cancellationToken)
    {
        return _userRoleGetter.GetUserRole(userId, cancellationToken);
    }


    private async Task<Either<DomainError, VerifiedUser>> GetUser(string token, CancellationToken cancellationToken)
    {
        try
        {
            var verifiedUser = await _passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser.Success ? verifiedUser : NotFoundError.Create("User has not been registered");
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
}