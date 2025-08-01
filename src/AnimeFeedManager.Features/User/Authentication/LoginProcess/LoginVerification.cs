using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication.LoginProcess;

public static class LoginVerification
{
    public static async Task<Result<VerifiedUser>> VerifyUser(
        PasswordlessClient passwordlessClient,
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var verifiedUser = await passwordlessClient.VerifyAuthenticationTokenAsync(token, cancellationToken);
            return verifiedUser.Success
                ? verifiedUser
                : NotFoundError.Create("User not found");
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