using System.Runtime.CompilerServices;
using Passwordless;

namespace AnimeFeedManager.Features.User.Authentication;

public sealed record PasswordlessError : DomainError
{
    public PasswordlessProblemDetails ProblemDetails { get; }
    private PasswordlessApiException Exception { get; }

    private PasswordlessError(PasswordlessApiException exn,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber) : base(exn.Message, callerMemberName, callerFilePath, callerLineNumber)
    {
        Exception = exn;
        ProblemDetails = exn.Details;
    }

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError(Exception, "{Error}", Exception.Message);
    }

    public static PasswordlessError FromException(PasswordlessApiException exception,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(exception, callerMemberName, callerFilePath, callerLineNumber);
}