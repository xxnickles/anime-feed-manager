using System.Net;
using AnimeFeedManager.Infrastructure.Cosmos.Concurrency;
using AnimeFeedManager.Infrastructure.Cosmos.Results;
using AnimeFeedManager.Shared.Results;
using AnimeFeedManager.Shared.Results.Static;
using Microsoft.Azure.Cosmos;

namespace AnimeFeedManager.Infrastructure.Tests.Cosmos.Concurrency;

public class UntilWrittenTests
{
    #region First attempt succeeds

    [Fact]
    public async Task Should_Return_Success_Without_Retrying_When_First_Attempt_Succeeds()
    {
        using var probe = new ActivityProbe();
        var calls = 0;

        var result = await OptimisticConcurrency.UntilWritten(
            (Func<CancellationToken, Task<Result<int>>>)ReadModifyWrite, TestContext.Current.CancellationToken, RetryOnPrecondition);

        Assert.Equal(1, calls);
        result.Match(
            onOk: value => Assert.Equal(42, value),
            onError: error => Assert.Fail($"Expected success but got: {error.Message}"));

        // No 412 → no retry → the contention tag is absent, keeping uncontended spans clean.
        Assert.Null(probe.Retries);
        return;

        Task<Result<int>> ReadModifyWrite(CancellationToken _)
        {
            calls++;
            return Task.FromResult<Result<int>>(42);
        }
    }

    #endregion

    #region Retries on precondition failure

    [Fact]
    public async Task Should_Retry_Until_Success_When_Precondition_Fails()
    {
        using var probe = new ActivityProbe();
        var calls = 0;
        Func<CancellationToken, Task<Result<int>>> readModifyWrite = _ =>
        {
            calls++;
            // Two 412s, then the write lands.
            return calls <= 2
                ? Task.FromResult<Result<int>>(PreconditionFailed())
                : Task.FromResult<Result<int>>(42);
        };

        var result = await OptimisticConcurrency.UntilWritten(
            readModifyWrite, TestContext.Current.CancellationToken, RetryOnPrecondition);

        Assert.Equal(3, calls);
        result.Match(
            onOk: value => Assert.Equal(42, value),
            onError: error => Assert.Fail($"Expected success after retries but got: {error.Message}"));

        // The two 412 retries are accumulated onto the span.
        Assert.Equal(2, probe.Retries);
    }

    #endregion

    #region Returns failures outside the retry set without retrying

    [Fact]
    public async Task Should_Return_Immediately_When_Failure_Is_Not_Precondition()
    {
        using var probe = new ActivityProbe();
        var nonRetryable = CosmosError(HttpStatusCode.ServiceUnavailable);
        var calls = 0;

        var result = await OptimisticConcurrency.UntilWritten(
            (Func<CancellationToken, Task<Result<int>>>)ReadModifyWrite, TestContext.Current.CancellationToken, RetryOnPrecondition);

        Assert.Equal(1, calls);
        result.Match(
            onOk: value => Assert.Fail($"Expected the non-412 failure to surface, but got success: {value}"),
            onError: error =>
            {
                var cosmosError = Assert.IsType<CosmosResponseError>(error);
                Assert.Equal(HttpStatusCode.ServiceUnavailable, cosmosError.StatusCode);
            });

        // A status outside the retry set is not a retry, so the contention tag stays absent.
        Assert.Null(probe.Retries);

        Task<Result<int>> ReadModifyWrite(CancellationToken _)
        {
            calls++;
            return Task.FromResult<Result<int>>(nonRetryable);
        }
    }

    [Fact]
    public async Task Should_Return_Immediately_When_Conflict_Is_Not_In_The_Retry_Set()
    {
        using var probe = new ActivityProbe();
        var conflict = CosmosError(HttpStatusCode.Conflict);
        var calls = 0;

        // A 409 against a 412-only retry set is terminal — it must surface, not loop. (Retrying a
        // sustained 409 here would never converge and hang, the failure mode this case guards.)
        var result = await OptimisticConcurrency.UntilWritten(
            (Func<CancellationToken, Task<Result<int>>>)ReadModifyWrite, TestContext.Current.CancellationToken, RetryOnPrecondition);

        Assert.Equal(1, calls);
        result.Match(
            onOk: value => Assert.Fail($"Expected the 409 to surface as terminal, but got success: {value}"),
            onError: error =>
            {
                var cosmosError = Assert.IsType<CosmosResponseError>(error);
                Assert.Equal(HttpStatusCode.Conflict, cosmosError.StatusCode);
            });

        Assert.Null(probe.Retries);
        return;

        Task<Result<int>> ReadModifyWrite(CancellationToken _)
        {
            calls++;
            return Task.FromResult<Result<int>>(conflict);
        }
    }

    #endregion

    #region Retries a status that is in the set

    [Fact]
    public async Task Should_Retry_Conflict_When_It_Is_In_The_Retry_Set()
    {
        using var probe = new ActivityProbe();
        var calls = 0;
        Func<CancellationToken, Task<Result<int>>> readModifyWrite = _ =>
        {
            calls++;
            // A create-race 409 on the first attempt, then the write lands.
            return calls <= 1
                ? Task.FromResult<Result<int>>(CosmosError(HttpStatusCode.Conflict))
                : Task.FromResult<Result<int>>(9);
        };

        var result = await OptimisticConcurrency.UntilWritten(
            readModifyWrite, TestContext.Current.CancellationToken, RetryOnPreconditionOrConflict);

        Assert.Equal(2, calls);
        result.Match(
            onOk: value => Assert.Equal(9, value),
            onError: error => Assert.Fail($"Expected success after the 409 retry but got: {error.Message}"));

        Assert.Equal(1, probe.Retries);
    }

    #endregion

    #region Accumulates onto an existing count

    [Fact]
    public async Task Should_Accumulate_Retries_Onto_An_Existing_Tag()
    {
        using var probe = new ActivityProbe();
        // Simulate a prior guarded write on the same span (e.g. the primary leg) having retried 3×.
        probe.Activity.SetTag(OptimisticConcurrency.PreconditionRetriesTag, 3);

        var calls = 0;

        var result = await OptimisticConcurrency.UntilWritten(
            (Func<CancellationToken, Task<Result<int>>>)ReadModifyWrite, TestContext.Current.CancellationToken, RetryOnPrecondition);

        result.Match(
            onOk: value => Assert.Equal(7, value),
            onError: error => Assert.Fail($"Expected success after a retry but got: {error.Message}"));

        // The one new retry sums onto the prior count (3 + 1) rather than overwriting it.
        Assert.Equal(4, probe.Retries);
        return;

        Task<Result<int>> ReadModifyWrite(CancellationToken _)
        {
            calls++;
            return calls <= 1
                ? Task.FromResult<Result<int>>(PreconditionFailed())
                : Task.FromResult<Result<int>>(7);
        }
    }

    #endregion

    #region Sustained contention is broken only by cancellation

    [Fact]
    public async Task Should_Throw_OperationCanceledException_When_Contention_Is_Sustained()
    {
        using var probe = new ActivityProbe();
        using var cts = new CancellationTokenSource();
        var calls = 0;
        Func<CancellationToken, Task<Result<int>>> readModifyWrite = _ =>
        {
            calls++;
            // 412 forever; the operator cancels to escape the storm on the third attempt.
            if (calls == 3) cts.Cancel();
            return Task.FromResult<Result<int>>(PreconditionFailed());
        };

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => OptimisticConcurrency.UntilWritten(readModifyWrite, cts.Token, RetryOnPrecondition));

        Assert.Equal(3, calls);
        // The count is stamped by the finally even though the loop exits via the cancellation throw.
        Assert.Equal(3, probe.Retries);
    }

    #endregion

    #region Test Helpers

    private static readonly HttpStatusCode[] RetryOnPrecondition = [HttpStatusCode.PreconditionFailed];
    private static readonly HttpStatusCode[] RetryOnPreconditionOrConflict =
        [HttpStatusCode.PreconditionFailed, HttpStatusCode.Conflict];

    private static CosmosResponseError PreconditionFailed() => CosmosError(HttpStatusCode.PreconditionFailed);

    private static CosmosResponseError CosmosError(HttpStatusCode statusCode) =>
        CosmosResponseError.Create(
            new CosmosException(
                message: $"cosmos {statusCode}",
                statusCode: statusCode,
                subStatusCode: 0,
                activityId: "test-activity",
                requestCharge: 0),
            new PartitionKey("x"),
            id: "x",
            container: "x");

    #endregion
}
