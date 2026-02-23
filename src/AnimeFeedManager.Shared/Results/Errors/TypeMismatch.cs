namespace AnimeFeedManager.Shared.Results.Errors;

public sealed record TypeMismatch(
    Type TargetType,
    object Received)
    : DomainError("Type Mismatch")
{
    public override Action<ILogger> LogAction() => logger =>
        logger.LogError(
            "{Message}. Expected an object of type {TargetType}, but got {Type}", Message,
            TargetType.FullName, Received.GetType().FullName);
}