namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class BindOnErrorExtensionsTests
{
    #region BindOnError (sync) — success passthrough

    [Fact]
    public void Should_Return_Original_Success_When_BindOnError_Called_On_Success()
    {
        var binderCalled = false;
        var result = Result<int>.Success(42)
            .BindOnError(err =>
            {
                binderCalled = true;
                return Result<int>.Success(0);
            });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
    }

    #endregion

    #region BindOnError (sync) — failure paths

    [Fact]
    public void Should_Call_Binder_With_Error_When_BindOnError_Called_On_Failure()
    {
        var error = NotFoundError.Create("original error");
        DomainError? capturedError = null;

        Result<int>.Failure(error)
            .BindOnError(err =>
            {
                capturedError = err;
                return Result<int>.Success(99);
            });

        Assert.Same(error, capturedError);
    }

    [Fact]
    public void Should_Return_Success_When_BindOnError_Binder_Returns_Success()
    {
        var error = NotFoundError.Create("recoverable error");
        var result = Result<int>.Failure(error)
            .BindOnError(_ => Result<int>.Success(99));

        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public void Should_Return_Failure_When_BindOnError_Binder_Returns_Failure()
    {
        var originalError = NotFoundError.Create("original");
        var recoveryError = Error.Create("recovery failed too");
        var result = Result<int>.Failure(originalError)
            .BindOnError(_ => Result<int>.Failure(recoveryError));

        result.AssertOnError(err => Assert.Same(recoveryError, err));
    }

    [Fact]
    public void Should_Merge_TraceContexts_When_BindOnError_Called_On_Failure()
    {
        var originalCallLog = new List<string>();
        var binderCallLog = new List<string>();

        var failureWithLog = Result<int>.Failure(NotFoundError.Create("err"))
            .AddLog(_ => originalCallLog.Add("original"));

        var result = failureWithLog.BindOnError(_ =>
            Result<int>.Success(42).AddLog(_ => binderCallLog.Add("binder")));

        var logger = new CapturingLogger();
        result.Complete(logger);

        Assert.Contains("original", originalCallLog);
        Assert.Contains("binder", binderCallLog);
    }

    #endregion

    #region BindOnError (async binder on sync Result<T>)

    [Fact]
    public async Task Should_Return_Original_Success_When_Async_BindOnError_Called_On_Success()
    {
        var binderCalled = false;
        var result = await Result<int>.Success(42)
            .BindOnError(err =>
            {
                binderCalled = true;
                return Task.FromResult(Result<int>.Success(0));
            });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
    }

    [Fact]
    public async Task Should_Call_Async_Binder_With_Error_When_BindOnError_Called_On_Failure()
    {
        var error = NotFoundError.Create("original error");
        DomainError? capturedError = null;

        await Result<int>.Failure(error)
            .BindOnError(err =>
            {
                capturedError = err;
                return Task.FromResult(Result<int>.Success(0));
            });

        Assert.Same(error, capturedError);
    }

    [Fact]
    public async Task Should_Return_Success_When_Async_BindOnError_Binder_Returns_Success()
    {
        var result = await Result<int>.Failure(NotFoundError.Create("err"))
            .BindOnError(_ => Task.FromResult(Result<int>.Success(99)));

        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public async Task Should_Return_Failure_When_Async_BindOnError_Binder_Returns_Failure()
    {
        var recoveryError = Error.Create("recovery failed");
        var result = await Result<int>.Failure(NotFoundError.Create("original"))
            .BindOnError(_ => Task.FromResult(Result<int>.Failure(recoveryError)));

        result.AssertOnError(err => Assert.Same(recoveryError, err));
    }

    [Fact]
    public async Task Should_Merge_TraceContexts_When_Async_BindOnError_Called_On_Failure()
    {
        var originalCallLog = new List<string>();
        var binderCallLog = new List<string>();

        var failureWithLog = Result<int>.Failure(NotFoundError.Create("err"))
            .AddLog(_ => originalCallLog.Add("original"));

        var result = await failureWithLog.BindOnError(_ =>
            Task.FromResult(Result<int>.Success(42).AddLog(_ => binderCallLog.Add("binder"))));

        var logger = new CapturingLogger();
        result.Complete(logger);

        Assert.Contains("original", originalCallLog);
        Assert.Contains("binder", binderCallLog);
    }

    #endregion

    #region BindOnErrorWhen (sync) — success passthrough

    [Fact]
    public void Should_Return_Original_Success_And_Not_Call_Binder_Or_Predicate_When_BindOnErrorWhen_Called_On_Success()
    {
        var binderCalled = false;
        var predicateCalled = false;

        var result = Result<int>.Success(42)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(0);
                },
                err =>
                {
                    predicateCalled = true;
                    return true;
                });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    #endregion

    #region BindOnErrorWhen (sync) — failure + predicate true

    [Fact]
    public void Should_Call_Binder_When_BindOnErrorWhen_Predicate_Returns_True()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("recoverable");

        var result = Result<int>.Failure(error)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(99);
                },
                err => true);

        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    #endregion

    #region BindOnErrorWhen (sync) — failure + predicate false

    [Fact]
    public void Should_Return_Original_Failure_And_Not_Call_Binder_When_BindOnErrorWhen_Predicate_Returns_False()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("non-recoverable");

        var result = Result<int>.Failure(error)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(99);
                },
                err => false);

        Assert.False(binderCalled);
        result.AssertOnError(err => Assert.Same(error, err));
    }

    [Fact]
    public void Should_Filter_By_Error_Type_Using_BindOnErrorWhen_Predicate()
    {
        var notFoundError = NotFoundError.Create("missing item");
        var binderCalled = false;

        var result = Result<int>.Failure(notFoundError)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(0);
                },
                err => err is NotFoundError);

        Assert.True(binderCalled);
        result.AssertSuccess();
    }

    [Fact]
    public void Should_Not_Call_Binder_When_Error_Type_Does_Not_Match_BindOnErrorWhen_Predicate()
    {
        var genericError = Error.Create("generic error");
        var binderCalled = false;

        var result = Result<int>.Failure(genericError)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(0);
                },
                err => err is NotFoundError);

        Assert.False(binderCalled);
        result.AssertOnError(err => Assert.Same(genericError, err));
    }

    #endregion

    #region BindOnErrorWhen (async binder on sync Result<T>)

    [Fact]
    public async Task Should_Return_Original_Success_And_Not_Call_Async_Binder_Or_Predicate_When_BindOnErrorWhen_Called_On_Success()
    {
        var binderCalled = false;
        var predicateCalled = false;

        var result = await Result<int>.Success(42)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(0));
                },
                err =>
                {
                    predicateCalled = true;
                    return true;
                });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    [Fact]
    public async Task Should_Call_Async_Binder_When_BindOnErrorWhen_Predicate_Returns_True()
    {
        var binderCalled = false;

        var result = await Result<int>.Failure(NotFoundError.Create("err"))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(99));
                },
                err => true);

        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public async Task Should_Return_Original_Failure_And_Not_Call_Async_Binder_When_BindOnErrorWhen_Predicate_Returns_False()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("non-recoverable");

        var result = await Result<int>.Failure(error)
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(99));
                },
                err => false);

        Assert.False(binderCalled);
        result.AssertOnError(err => Assert.Same(error, err));
    }

    #endregion

    #region Task<Result<T>> BindOnError wrappers

    [Fact]
    public async Task Should_Return_Original_Success_When_Task_BindOnError_Called_On_Success()
    {
        var binderCalled = false;
        var result = await Task.FromResult(Result<int>.Success(42))
            .BindOnError(err =>
            {
                binderCalled = true;
                return Result<int>.Success(0);
            });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
    }

    [Fact]
    public async Task Should_Call_Binder_When_Task_BindOnError_Called_On_Failure()
    {
        var error = NotFoundError.Create("original");
        DomainError? capturedError = null;

        await Task.FromResult(Result<int>.Failure(error))
            .BindOnError(err =>
            {
                capturedError = err;
                return Result<int>.Success(0);
            });

        Assert.Same(error, capturedError);
    }

    [Fact]
    public async Task Should_Return_Success_When_Task_BindOnError_Binder_Returns_Success()
    {
        var result = await Task.FromResult(Result<int>.Failure(NotFoundError.Create("err")))
            .BindOnError(_ => Result<int>.Success(99));

        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public async Task Should_Return_Failure_When_Task_BindOnError_Binder_Returns_Failure()
    {
        var recoveryError = Error.Create("recovery failed");
        var result = await Task.FromResult(Result<int>.Failure(NotFoundError.Create("original")))
            .BindOnError(_ => Result<int>.Failure(recoveryError));

        result.AssertOnError(err => Assert.Same(recoveryError, err));
    }

    [Fact]
    public async Task Should_Return_Original_Success_When_Task_Async_BindOnError_Called_On_Success()
    {
        var binderCalled = false;
        var result = await Task.FromResult(Result<int>.Success(42))
            .BindOnError(err =>
            {
                binderCalled = true;
                return Task.FromResult(Result<int>.Success(0));
            });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
    }

    [Fact]
    public async Task Should_Return_Success_When_Task_Async_BindOnError_Binder_Returns_Success()
    {
        var result = await Task.FromResult(Result<int>.Failure(NotFoundError.Create("err")))
            .BindOnError(_ => Task.FromResult(Result<int>.Success(99)));

        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    #endregion

    #region Task<Result<T>> BindOnErrorWhen wrappers

    [Fact]
    public async Task Should_Return_Original_Success_When_Task_BindOnErrorWhen_Called_On_Success()
    {
        var binderCalled = false;
        var predicateCalled = false;

        var result = await Task.FromResult(Result<int>.Success(42))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(0);
                },
                err =>
                {
                    predicateCalled = true;
                    return true;
                });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    [Fact]
    public async Task Should_Call_Binder_When_Task_BindOnErrorWhen_Predicate_Returns_True()
    {
        var binderCalled = false;

        var result = await Task.FromResult(Result<int>.Failure(NotFoundError.Create("err")))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(99);
                },
                err => true);

        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public async Task Should_Return_Original_Failure_When_Task_BindOnErrorWhen_Predicate_Returns_False()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("non-recoverable");

        var result = await Task.FromResult(Result<int>.Failure(error))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Result<int>.Success(99);
                },
                err => false);

        Assert.False(binderCalled);
        result.AssertOnError(err => Assert.Same(error, err));
    }

    [Fact]
    public async Task Should_Return_Original_Success_When_Task_Async_BindOnErrorWhen_Called_On_Success()
    {
        var binderCalled = false;
        var predicateCalled = false;

        var result = await Task.FromResult(Result<int>.Success(42))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(0));
                },
                err =>
                {
                    predicateCalled = true;
                    return true;
                });

        result.AssertOnSuccess(value => Assert.Equal(42, value));
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    [Fact]
    public async Task Should_Call_Async_Binder_When_Task_BindOnErrorWhen_Predicate_Returns_True()
    {
        var binderCalled = false;

        var result = await Task.FromResult(Result<int>.Failure(NotFoundError.Create("err")))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(99));
                },
                err => true);

        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(99, value));
    }

    [Fact]
    public async Task Should_Return_Original_Failure_When_Task_Async_BindOnErrorWhen_Predicate_Returns_False()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("non-recoverable");

        var result = await Task.FromResult(Result<int>.Failure(error))
            .BindOnErrorWhen(
                err =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int>.Success(99));
                },
                err => false);

        Assert.False(binderCalled);
        result.AssertOnError(err => Assert.Same(error, err));
    }

    #endregion

    #region Test Helpers

    private sealed class CapturingLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) { }
    }

    #endregion
}
