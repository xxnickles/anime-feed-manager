using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Subscriptions.Feed.Events;

namespace AnimeFeedManager.Features.Tests.Infrastructure.Messaging;

public class MessagingExtensionsTests
{
    #region SendOnError Tests

    [Fact]
    public async Task Should_Not_Send_Error_Events_When_Result_Is_Success()
    {
        var postman = new TestDomainPostman();

        var result = await Task.FromResult(Result<int>.Success(42))
            .SendOnError(postman, [ErrorEvent()], CancellationToken.None);

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.Empty(postman.Sent);
    }

    [Fact]
    public async Task Should_Send_Error_Events_And_Keep_Original_Error_When_Result_Fails()
    {
        var postman = new TestDomainPostman();
        var errorEvent = ErrorEvent();

        var result = await Failure("original failure")
            .SendOnError(postman, [errorEvent], CancellationToken.None);

        result.AssertOnError(error => Assert.Equal("original failure", Assert.IsType<Error>(error).Message));
        Assert.Equal([errorEvent], postman.Sent);
    }

    [Fact]
    public async Task Should_Keep_Original_Error_When_Error_Events_Fail_To_Send()
    {
        var postman = new TestDomainPostman { FailSend = true };

        var result = await Failure("original failure")
            .SendOnError(postman, [ErrorEvent()], CancellationToken.None);

        result.AssertOnError(error => Assert.Equal("original failure", Assert.IsType<Error>(error).Message));
    }

    [Fact]
    public async Task Should_Log_Original_Error_And_Send_Failure_When_Error_Events_Fail_To_Send()
    {
        var postman = new TestDomainPostman { FailSend = true };
        var logger = new RecordingLogger();

        (await Failure("original failure")
                .SendOnError(postman, [ErrorEvent()], CancellationToken.None))
            .FlushLogs(logger);

        Assert.Equal(2, logger.Entries.Count);
        Assert.Contains(logger.Entries, e => e.Message.Contains("original failure"));
        Assert.Contains(logger.Entries, e => e.Message.Contains("queue unavailable") && e.Exception is InvalidOperationException);
    }

    [Fact]
    public async Task Should_Log_Only_Original_Error_When_Error_Events_Are_Sent()
    {
        var postman = new TestDomainPostman();
        var logger = new RecordingLogger();

        (await Failure("original failure")
                .SendOnError(postman, [ErrorEvent()], CancellationToken.None))
            .FlushLogs(logger);

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("original failure", entry.Message);
    }

    #endregion

    #region Test Helpers

    private static Task<Result<int>> Failure(string message) =>
        Task.FromResult(Result<int>.Failure(Error.Create(message)));

    private static DomainMessage ErrorEvent() => new RunFeedNotification();

    private sealed class TestDomainPostman
    {
        public List<DomainMessage> Sent { get; } = [];
        public bool FailSend { get; init; }

        private DomainCollectionSender Delegate => (messages, _) =>
        {
            if (FailSend)
                return Task.FromResult<Result<Unit>>(
                    MessagesNotDelivered.Create(new InvalidOperationException("queue unavailable"), messages));

            Sent.AddRange(messages);
            return Task.FromResult(Result<Unit>.Success(new Unit()));
        };

        public static implicit operator DomainCollectionSender(TestDomainPostman wrapper) => wrapper.Delegate;
    }

    private sealed record LogEntry(string Message, Exception? Exception);

    private sealed class RecordingLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            Entries.Add(new LogEntry(formatter(state, exception), exception));
    }

    #endregion
}
