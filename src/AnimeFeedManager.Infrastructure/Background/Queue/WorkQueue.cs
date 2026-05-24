namespace AnimeFeedManager.Infrastructure.Background.Queue;

/// <summary>
/// Producer-side façade over an in-process bounded <see cref="Channel{T}"/>.
/// Inject <c>WorkQueue&lt;TCommand&gt;</c> into a feature service to enqueue work
/// for the queue's registered handler. One instance per command type, created at
/// registration time using the handler's declared capacity and back-pressure
/// policy.
/// </summary>
public sealed class WorkQueue<TCommand>(Channel<TCommand> channel)
{
    /// <summary>
    /// Hand off <paramref name="command"/> to the queue. When the channel is full,
    /// the call awaits according to the handler's <c>FullMode</c> (back-pressure
    /// under <c>Wait</c>; silent eviction under <c>DropOldest</c>).
    /// </summary>
    public ValueTask Enqueue(TCommand command, CancellationToken cancellationToken = default)
        => channel.Writer.WriteAsync(command, cancellationToken);

    internal ChannelReader<TCommand> Reader => channel.Reader;
}
