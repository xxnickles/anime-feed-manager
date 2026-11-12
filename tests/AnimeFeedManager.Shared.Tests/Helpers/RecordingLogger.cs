namespace AnimeFeedManager.Shared.Tests.Helpers;

/// <summary>Captures formatted log messages and scope properties.</summary>
internal sealed class RecordingLogger : ILogger
{
    public List<string> Entries { get; } = [];
    public List<KeyValuePair<string, object>> ScopeProperties { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        if (state is IEnumerable<KeyValuePair<string, object>> properties)
            ScopeProperties.AddRange(properties);
        return null;
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Entries.Add(formatter(state, exception));
}
