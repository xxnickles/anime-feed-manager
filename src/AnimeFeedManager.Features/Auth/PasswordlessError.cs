using Passwordless;

namespace AnimeFeedManager.Features.Auth;

/// <summary>
/// Wraps a <see cref="PasswordlessApiException"/> as a domain error, preserving the
/// <see cref="PasswordlessProblemDetails"/> for callers that need the status/title.
/// </summary>
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
        logger.LogError(Exception, "{Error}", Exception.Message);

    public static PasswordlessError FromException(PasswordlessApiException exception) => new(exception);
}
