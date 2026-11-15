namespace AnimeFeedManager.Features.Tests.Helpers;

/// <summary>Stand-in errors for tests that need a failure but not a particular one.</summary>
public sealed record TestError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}

public sealed record OtherTestError(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogError("{domain_error.message}", Message);
}
