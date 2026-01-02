using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication;

public sealed record PasswordlessError : DomainError
{
    public PasswordlessProblemDetails ProblemDetails { get; }
    private PasswordlessApiException Exception { get; }

    private PasswordlessError(PasswordlessApiException exn) : base(exn.Message)
    {
        Exception = exn;
        ProblemDetails = exn.Details;
    }

    public override Action<ILogger> LogAction() => logger =>
    {
        logger.LogError(Exception, "{Error}", Exception.Message);
    };

    public static PasswordlessError FromException(PasswordlessApiException exception) => new(exception);
}