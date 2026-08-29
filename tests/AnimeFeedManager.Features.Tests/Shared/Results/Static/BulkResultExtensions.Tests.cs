namespace AnimeFeedManager.Features.Tests.Shared.Results.Static;

public class BulkResultExtensionsTests
{
    private sealed class TrackingLogger : ILogger
    {
        public int CallCount { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => CallCount++;
    }

    [Fact]
    public void Should_Call_OnCompleted_For_CompletedBulkResult()
    {
        var logger = new TrackingLogger();
        int? capturedValue = null;

        var bulk = new CompletedBulkResult<int>(42);
        bulk.LogResults(logger, (value, _) => capturedValue = value);

        Assert.Equal(42, capturedValue);
    }

    [Fact]
    public void Should_Call_OnCompleted_For_PartialSuccessBulkResult()
    {
        var logger = new TrackingLogger();
        int? capturedValue = null;

        var bulk = new PartialSuccessBulkResult<int>(7, [NotFoundError.Create("err")]);
        bulk.LogResults(logger, (value, _) => capturedValue = value);

        Assert.Equal(7, capturedValue);
    }

    [Fact]
    public void Should_Log_Each_Error_In_PartialSuccessBulkResult()
    {
        var logger = new TrackingLogger();

        var bulk = new PartialSuccessBulkResult<int>(0, [
            NotFoundError.Create("err1"),
            NotFoundError.Create("err2")
        ]);

        bulk.LogResults(logger, (_, _) => { });

        Assert.Equal(2, logger.CallCount);
    }
}
