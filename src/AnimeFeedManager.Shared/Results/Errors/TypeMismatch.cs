namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record TypeMismatch(
    Type TargetType,
    object Received)
    : DomainError("Type Mismatch")
{
    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(
            "{domain_error.message}. Expected an object of type {domain_error.subject}, but got {domain_error.actual_type}", Message,
            TargetType.FullName, Received.GetType().FullName);
}