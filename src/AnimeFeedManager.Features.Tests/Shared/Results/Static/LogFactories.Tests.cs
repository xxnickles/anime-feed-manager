namespace AnimeFeedManager.Features.Tests.Shared.Results.Static;

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
    public void Should_LogBulkResult_Invoke_OnCompleted_For_CompletedBulkResult()
    {
        var logger = new SilentLogger();
        int? capturedValue = null;

        var factory = LogFactories.LogBulkResult<int>((value, _) => capturedValue = value);
        factory(new CompletedBulkResult<int>(99))(logger);

        Assert.Equal(99, capturedValue);
    }

    [Fact]
    public void Should_LogBulkResult_Invoke_OnCompleted_For_PartialSuccessBulkResult()
    {
        var logger = new SilentLogger();
        int? capturedValue = null;

        var factory = LogFactories.LogBulkResult<int>((value, _) => capturedValue = value);
        factory(new PartialSuccessBulkResult<int>(5, [NotFoundError.Create("partial")]))(logger);

        Assert.Equal(5, capturedValue);
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
