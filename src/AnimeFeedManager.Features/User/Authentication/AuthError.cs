using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication;

public closed record AuthError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);

    public sealed record PasswordlessFailure(PasswordlessApiException Exception) : AuthError(Exception.Message)
    {
        public PasswordlessProblemDetails ProblemDetails => Exception.Details;

        public override Action<ILogger> LogAction() => logger => logger.LogError(Exception, "{domain_error.message}", Message);
    }

    public sealed record RequestFailed(Exception Exception) : AuthError(Exception.Message)
    {
        public override Action<ILogger> LogAction() => logger => logger.LogError(Exception, "{domain_error.message}", Message);
    }

    /// <summary>Passwordless rejected the sign-in token.</summary>
    public sealed record UserNotVerified() : AuthError("User not found");

    public sealed record UserNotFound() : AuthError("User doesn't exist in the system");

    public sealed record UserAlreadyExists(Email Email) : AuthError($"Email '{Email}' already exist in the system");
}
