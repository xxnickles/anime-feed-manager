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

    #region AddLogOnFailure async wrapper (Task<Result<T>>)

    [Fact]
    public async Task Should_Run_Log_Action_After_Complete_When_AddLogOnFailure_Called_On_Failure()
    {
        var logActionInvoked = false;
        var logger = new SilentLogger();

        await Task.FromResult(Result<int>.Failure(NotFoundError.Create("failed")))
            .AddLogOnFailure(_ => _ => { logActionInvoked = true; })
            .Complete(logger);

        Assert.True(logActionInvoked);
    }

    [Fact]
    public async Task Should_Not_Run_Log_Action_After_Complete_When_AddLogOnFailure_Called_On_Success()
    {
        var logActionInvoked = false;
        var logger = new SilentLogger();

        await Task.FromResult(Result<int>.Success(42))
            .AddLogOnFailure(_ => _ => { logActionInvoked = true; })
            .Complete(logger);

        Assert.False(logActionInvoked);
    }

    #endregion
}
