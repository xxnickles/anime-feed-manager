using Polly;

namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Base class for queue-driven work. A subclass binds a single command type to a
/// processing routine. Channel capacity, back-pressure policy, and the Polly
/// resilience pipeline are declared as virtual properties so the handler is the
/// sole source of truth for its own queue shape. Subclasses are registered in DI
/// via <c>AddWorkQueueHandler&lt;TCommand, THandler&gt;</c>; the host opens a fresh
/// scope per dequeued command, resolves the handler, and invokes
/// <see cref="Handle"/>. The procedural shell (this class, its ctor, DI wiring)
/// hosts the functional core inside <see cref="Handle"/>.
/// </summary>
public abstract class WorkHandler<TCommand>
{
    /// <summary>
    /// Bounded-channel capacity. When the channel is full, producer behaviour is
    /// governed by <see cref="FullMode"/>.
    /// </summary>
    public virtual int Capacity => 100;

    /// <summary>
    /// What happens when the channel is at capacity and a producer enqueues.
    /// Default <see cref="BoundedChannelFullMode.Wait"/> back-pressures producers;
    /// switch to <see cref="BoundedChannelFullMode.DropOldest"/> for "latest wins"
    /// semantics where old items are stale by the time they would run.
    /// </summary>
    public virtual BoundedChannelFullMode FullMode => BoundedChannelFullMode.Wait;

    /// <summary>
    /// Optional handler-specific Polly resilience pipeline. Null falls back to the
    /// shared default (3-retry exponential backoff with jitter). Override to tune
    /// retry counts, add circuit breakers, or wire a different policy.
    /// </summary>
    public virtual ResiliencePipeline? ResiliencePipeline => null;

    public abstract Task<Result<Unit>> Handle(TCommand command, CancellationToken cancellationToken);
}
