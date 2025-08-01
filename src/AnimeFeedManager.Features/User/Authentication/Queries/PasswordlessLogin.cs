using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.Queries;

public static class PasswordlessLogin
{
    public static async Task<Result<VerifiedUser>> GetLoginInformation(
        this IPasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser.Success ? verifiedUser : NotFoundError.Create("User has not been registered");
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