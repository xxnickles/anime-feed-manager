namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class LogFactoriesTests
{
    private sealed class SilentLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) { }
    }

    [Fact]
    public void Should_Log_Invoke_LogAction_With_Value()
    {
        var logger = new SilentLogger();
        int? capturedValue = null;

        var factory = LogFactories.Log<int>((value, _) => capturedValue = value);
        factory(123)(logger);

        Assert.Equal(123, capturedValue);
    }
}
