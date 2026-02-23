namespace AnimeFeedManager.Features.Infrastructure.Email;

/// <summary>
/// Domain error for email sending failures
/// </summary>
public sealed record EmailSendError : DomainError
{
    private EmailSendError(
        string errorMessage) : base(errorMessage)
    {
    }

    public static EmailSendError Create(
        string message) => new(message);


    public override Action<ILogger> LogAction() =>
        logger => logger.LogError("Failed to send email: {Message}", Message);
}

/// <summary>
/// Domain error for email template rendering failures
/// </summary>
public sealed record EmailRenderError : DomainError
{
    private EmailRenderError(
        string errorMessage) : base(errorMessage)
    {
    }

    public static EmailRenderError Create(
        string message) =>
        new(message);


    public override Action<ILogger> LogAction() => logger =>
        logger.LogError("Failed to render email template: {Message}", Message);
}