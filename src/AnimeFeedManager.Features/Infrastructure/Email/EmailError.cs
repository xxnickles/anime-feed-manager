namespace AnimeFeedManager.Features.Infrastructure.Email;

public closed record EmailError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);

    public sealed record RenderFailed(Exception Exception) : EmailError($"Template rendering failed: {Exception.Message}")
    {
        public override Action<ILogger> LogAction() => logger =>
            logger.LogError(Exception, "Failed to render email template: {domain_error.message}", Message);
    }

    public sealed record SendFailed(Exception Exception) : EmailError($"SMTP send failed: {Exception.Message}")
    {
        public override Action<ILogger> LogAction() => logger =>
            logger.LogError(Exception, "Failed to send email: {domain_error.message}", Message);
    }
}
