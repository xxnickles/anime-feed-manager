namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Heterogeneous registration element collected by <see cref="WorkQueueHostedService"/>.
/// Only the closure is type-erased — its body captures the concrete <c>TCommand</c>,
/// <c>THandler</c>, scope factory, and default pipeline from the registration site,
/// so no <see cref="object"/> data ever flows through the host and the host needs
/// no DI plumbing of its own to invoke the drain.
/// </summary>
internal sealed record WorkQueueDrainOp(
    string HandlerName,
    Func<CancellationToken, Task> Drain);
