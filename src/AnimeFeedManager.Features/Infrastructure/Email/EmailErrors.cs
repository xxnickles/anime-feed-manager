namespace AnimeFeedManager.Features.Infrastructure.Email;

/// <summary>
/// Domain error for email sending failures
/// </summary>
public sealed record EmailSendError : DomainError
{
    private EmailSendError(
        string message,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber) : base(message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }

    public static EmailSendError Create(
        string message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(message, callerMemberName, callerFilePath, callerLineNumber);

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("Failed to send email: {Message}", Message);
    }
}

/// <summary>
/// Domain error for email template rendering failures
/// </summary>
public sealed record EmailRenderError : DomainError
{
    private EmailRenderError(
        string message,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber) : base(message, callerMemberName, callerFilePath, callerLineNumber)
    {
    }

    public static EmailRenderError Create(
        string message,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0) =>
        new(message, callerMemberName, callerFilePath, callerLineNumber);

    protected override void LoggingBehavior(ILogger logger)
    {
        logger.LogError("Failed to render email template: {Message}", Message);
    }
}
