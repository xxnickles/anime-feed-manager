namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class TapErrorExtensionsTests
{
    #region TapError (sync) — Result<T>.TapError(Action<DomainError>)

    [Fact]
    public void Should_Execute_TapError_Action_When_Result_Is_Failure()
    {
        var error = NotFoundError.Create("test error");
        DomainError? captured = null;

        Result<int>.Failure(error)
            .TapError(err => captured = err);

        Assert.Same(error, captured);
    }

    [Fact]
    public void Should_Not_Execute_TapError_Action_When_Result_Is_Success()
    {
        var executed = false;

        Result<int>.Success(42)
            .TapError(err => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public void Should_Return_Original_Failure_Unchanged_From_Sync_TapError()
    {
        var error = NotFoundError.Create("test error");
        var result = Result<int>.Failure(error);
        var tappedResult = result.TapError(err => { });

        Assert.Same(result, tappedResult);
    }

    [Fact]
    public void Should_Return_Original_Success_Unchanged_From_Sync_TapError()
    {
        var result = Result<int>.Success(42);
        var tappedResult = result.TapError(err => { });

        Assert.Same(result, tappedResult);
    }

    #endregion

    #region TapError (async action) — Result<T>.TapError(Func<DomainError, Task>)

    [Fact]
    public async Task Should_Execute_Async_TapError_Action_When_Result_Is_Failure()
    {
        var error = NotFoundError.Create("test error");
        DomainError? captured = null;

        await Result<int>.Failure(error)
            .TapError(async err =>
            {
                await Task.Delay(1);
                captured = err;
            });

        Assert.Same(error, captured);
    }

    [Fact]
    public async Task Should_Not_Execute_Async_TapError_Action_When_Result_Is_Success()
    {
        var executed = false;

        await Result<int>.Success(42)
            .TapError(async err =>
            {
                await Task.Delay(1);
                executed = true;
            });

        Assert.False(executed);
    }

    [Fact]
    public async Task Should_Return_Original_Failure_Unchanged_From_Async_TapError_On_Failure()
    {
        var error = NotFoundError.Create("test error");
        var result = await Result<int>.Failure(error)
            .TapError(err => Task.CompletedTask);

        result.AssertOnError(err => Assert.Same(error, err));
    }

    [Fact]
    public async Task Should_Return_Original_Success_Unchanged_From_Async_TapError_On_Success()
    {
        var result = await Result<int>.Success(42)
            .TapError(err => Task.CompletedTask);

        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    #endregion

    #region TapError (Task receiver, sync action) — Task<Result<T>>.TapError(Action<DomainError>)

    [Fact]
    public async Task Should_Execute_TapError_Action_When_Task_Result_Is_Failure()
    {
        var error = NotFoundError.Create("test error");
        DomainError? captured = null;

        await Task.FromResult(Result<int>.Failure(error))
            .TapError(err => captured = err);

        Assert.Same(error, captured);
    }

    [Fact]
    public async Task Should_Not_Execute_TapError_Action_When_Task_Result_Is_Success()
    {
        var executed = false;

        await Task.FromResult(Result<int>.Success(42))
            .TapError(err => executed = true);

        Assert.False(executed);
    }

    [Fact]
    public async Task Should_Return_Original_Failure_Unchanged_From_Task_TapError_On_Failure()
    {
        var error = NotFoundError.Create("test error");
        var result = await Task.FromResult(Result<int>.Failure(error))
            .TapError(err => { });

        result.AssertOnError(err => Assert.Same(error, err));
    }

    [Fact]
    public async Task Should_Return_Original_Success_Unchanged_From_Task_TapError_On_Success()
    {
        var result = await Task.FromResult(Result<int>.Success(42))
            .TapError(err => { });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    #endregion

    #region TapError (Task receiver, async action) — Task<Result<T>>.TapError(Func<DomainError, Task>)

    [Fact]
    public async Task Should_Execute_Async_TapError_Action_When_Task_Result_Is_Failure()
    {
        var error = NotFoundError.Create("test error");
        DomainError? captured = null;

        await Task.FromResult(Result<int>.Failure(error))
            .TapError(async err =>
            {
                await Task.Delay(1);
                captured = err;
            });

        Assert.Same(error, captured);
    }

    [Fact]
    public async Task Should_Not_Execute_Async_TapError_Action_When_Task_Result_Is_Success()
    {
        var executed = false;

        await Task.FromResult(Result<int>.Success(42))
            .TapError(async err =>
            {
                await Task.Delay(1);
                executed = true;
            });

        Assert.False(executed);
    }

    [Fact]
    public async Task Should_Return_Original_Failure_Unchanged_From_Task_Async_TapError_On_Failure()
    {
        var error = NotFoundError.Create("test error");
        var result = await Task.FromResult(Result<int>.Failure(error))
            .TapError(err => Task.CompletedTask);

        result.AssertOnError(err => Assert.Same(error, err));
    }

    [Fact]
    public async Task Should_Return_Original_Success_Unchanged_From_Task_Async_TapError_On_Success()
    {
        var result = await Task.FromResult(Result<int>.Success(42))
            .TapError(err => Task.CompletedTask);

        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    #endregion

    #region Fluent chain composition

    [Fact]
    public void Should_Preserve_Fluent_Chain_With_TapError()
    {
        var log = new List<string>();

        var result = Result<int>.Failure(NotFoundError.Create("original"))
            .MapError(err => Error.Create($"wrapped: {err.Message}"))
            .TapError(err => log.Add(err.Message))
            .MapError(err => Error.Create($"double-wrapped: {err.Message}"));

        Assert.Single(log);
        Assert.StartsWith("wrapped:", log[0]);
        result.AssertOnError(err => Assert.StartsWith("double-wrapped:", err.Message));
    }

    #endregion
}
