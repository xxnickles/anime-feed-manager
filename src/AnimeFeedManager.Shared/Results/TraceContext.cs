using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results;

public sealed record TraceContext
{
    public static  TraceContext Empty { get; } = new([], ImmutableDictionary<string, object>.Empty);

    internal  ImmutableList<Action<ILogger>> LogActions { get; init; } = [];
    internal  ImmutableDictionary<string, object> Properties { get; init; } = [];

    internal TraceContext(
        ImmutableList<Action<ILogger>> logActions,
        ImmutableDictionary<string, object> properties)
    {
        LogActions = logActions;
        Properties = properties;
    }
}