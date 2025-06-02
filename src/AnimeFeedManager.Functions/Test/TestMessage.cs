namespace AnimeFeedManager.Functions.Test;

public sealed record TestMessage(Season Season, Year Year) : DomainMessage(new Box(TargetQueue))
{
    public const string TargetQueue = "test-queue";
}