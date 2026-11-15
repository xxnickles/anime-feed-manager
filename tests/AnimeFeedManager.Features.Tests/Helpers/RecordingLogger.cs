namespace AnimeFeedManager.Features.Tests.Helpers;

/// <summary>Captures formatted log messages; scopes are ignored.</summary>
internal sealed class RecordingLogger : ILogger
{
    public List<string> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) => Entries.Add(formatter(state, exception));
}
