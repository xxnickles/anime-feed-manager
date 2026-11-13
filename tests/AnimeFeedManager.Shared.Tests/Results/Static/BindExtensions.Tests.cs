namespace AnimeFeedManager.Shared.Tests.Results.Static;

public closed record CatalogFailure(string Message) : DomainError(Message)
{
    public override Action<ILogger> LogAction() => logger => logger.LogWarning("{domain_error.message}", Message);
}

public sealed record MissingEntry(string Key) : CatalogFailure($"{Key} is missing");

public class BindExtensionsTests
{
    #region Bind

    [Fact]
    public void Should_Bind_Success_Value_To_New_Result()
    {
        var result = Result<int, DomainError>.Success(42)
            .Bind(x => Result<string, DomainError>.Success(x.ToString()));

        result.AssertOnSuccess(value => Assert.Equal("42", value));
    }

    [Fact]
    public void Should_Not_Bind_When_Result_Is_Failure()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("Test error");
        var result = Result<int, DomainError>.Failure(error)
            .Bind(x =>
            {
                binderCalled = true;
                return Result<string, DomainError>.Success(x.ToString());
            });

        result.AssertError();
        Assert.False(binderCalled);
    }

    [Fact]
    public void Should_Short_Circuit_On_First_Bind_Failure()
    {
        var secondBinderCalled = false;
        var result = Result<int, DomainError>.Success(42)
            .Bind(x => Result<int, DomainError>.Failure(NotFoundError.Create("First error")))
            .Bind(x =>
            {
                secondBinderCalled = true;
                return Result<int, DomainError>.Success(x * 2);
            });

        result.AssertError();
        Assert.False(secondBinderCalled);
    }

    [Fact]
    public void Should_Chain_Multiple_Bind_Operations()
    {
        var result = Result<int, DomainError>.Success(5)
            .Bind(x => Result<int, DomainError>.Success(x * 2))
            .Bind(x => Result<int, DomainError>.Success(x + 10))
            .Bind(x => Result<string, DomainError>.Success(x.ToString()));

        result.AssertOnSuccess(value => Assert.Equal("20", value));
    }

    #endregion

    #region Covariant Steps

    [Fact]
    public void Should_Widen_Step_Failure_When_Step_Error_Derives_From_Chain_Error()
    {
        Result<string, DomainError> result = Result<int, DomainError>.Success(1)
            .Bind(_ => Result<string, MissingEntry>.Failure(new MissingEntry("key")));

        result.AssertOnError(error => Assert.Equal(new MissingEntry("key"), error));
    }

    [Fact]
    public void Should_Pass_Step_Value_When_A_Narrower_Step_Succeeds()
    {
        Result<string, DomainError> result = Result<int, DomainError>.Success(1)
            .Bind(x => Result<string, MissingEntry>.Success(x.ToString()));

        result.AssertOnSuccess(value => Assert.Equal("1", value));
    }

    [Fact]
    public void Should_Bind_Leaf_Step_When_Chain_Error_Is_Its_Family()
    {
        Result<string, CatalogFailure> result = Result<int, CatalogFailure>.Success(1)
            .Bind(_ => Result<string, MissingEntry>.Failure(new MissingEntry("key")));

        result.AssertOnError(error => Assert.IsType<MissingEntry>(error));
    }

    [Fact]
    public void Should_Merge_Trace_Contexts_When_Step_Error_Is_Widened()
    {
        var logger = new RecordingLogger();

        Result<int, DomainError>.Success(1)
            .AddLog(log => log.LogInformation("chain"))
            .Bind(_ => Result<string, MissingEntry>.Failure(new MissingEntry("key"))
                .AddLog(log => log.LogInformation("step")))
            .FlushLogs(logger);

        Assert.Equal(["chain", "step", "key is missing"], logger.Entries);
    }

    [Fact]
    public async Task Should_Widen_Step_Failure_When_Async_Binder_Returns_A_Narrower_Error()
    {
        Result<string, DomainError> result = await Result<int, DomainError>.Success(1)
            .Bind(_ => Task.FromResult(Result<string, MissingEntry>.Failure(new MissingEntry("key"))));

        result.AssertOnError(error => Assert.IsType<MissingEntry>(error));
    }

    [Fact]
    public async Task Should_Widen_Step_Failure_When_Binding_On_A_Task_Result()
    {
        Result<string, DomainError> result = await Task.FromResult(Result<int, DomainError>.Success(1))
            .Bind(_ => Task.FromResult(Result<string, MissingEntry>.Failure(new MissingEntry("key"))));

        result.AssertOnError(error => Assert.IsType<MissingEntry>(error));
    }

    #endregion

    #region BindWhen

    [Fact]
    public void Should_Execute_BindWhen_Only_When_Predicate_Is_True()
    {
        var binderCalled = false;
        var result = Result<int, DomainError>.Success(10)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int, DomainError>.Success(x * 2);
                },
                x => x > 5);

        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(20, value));
    }

    [Fact]
    public void Should_Skip_BindWhen_When_Predicate_Is_False()
    {
        var binderCalled = false;
        var result = Result<int, DomainError>.Success(3)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int, DomainError>.Success(x * 2);
                },
                x => x > 5);

        Assert.False(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(3, value));
    }

    [Fact]
    public void Should_Not_Execute_BindWhen_When_Result_Is_Failure()
    {
        var binderCalled = false;
        var predicateCalled = false;
        var error = NotFoundError.Create("Test error");
        var result = Result<int, DomainError>.Failure(error)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int, DomainError>.Success(x * 2);
                },
                x =>
                {
                    predicateCalled = true;
                    return true;
                });

        result.AssertError();
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    [Fact]
    public void Should_Use_BindWhen_For_Conditional_Validation()
    {
        var validatePositive = (int x) =>
            x > 0
                ? Result<int, DomainError>.Success(x)
                : Result<int, DomainError>.Failure(DomainValidationErrors.Create([
                    DomainValidationError.Create("value", "Must be positive")
                ]));

        var result = Result<int, DomainError>.Success(10)
            .BindWhen(validatePositive, x => x != 0);

        result.AssertOnSuccess(value => Assert.Equal(10, value));
    }

    #endregion

    #region Covariant BindWhen

    [Fact]
    public void Should_Widen_BindWhen_Step_Failure_When_Step_Error_Derives_From_Chain_Error()
    {
        Result<int, DomainError> result = Result<int, DomainError>.Success(1)
            .BindWhen(_ => Result<int, MissingEntry>.Failure(new MissingEntry("key")), _ => true);

        result.AssertOnError(error => Assert.IsType<MissingEntry>(error));
    }

    [Fact]
    public async Task Should_Widen_BindWhen_Step_Failure_When_Binding_On_A_Task_Result()
    {
        Result<int, DomainError> result = await Task.FromResult(Result<int, DomainError>.Success(1))
            .BindWhen(_ => Task.FromResult(Result<int, MissingEntry>.Failure(new MissingEntry("key"))), _ => true);

        result.AssertOnError(error => Assert.IsType<MissingEntry>(error));
    }

    [Fact]
    public async Task Should_Map_Without_Binding_When_Task_BindWhen_Predicate_Is_False()
    {
        var binderCalled = false;

        var result = await Task.FromResult(Result<int, DomainError>.Success(3))
            .BindWhen(x =>
                {
                    binderCalled = true;
                    return Task.FromResult(Result<int, MissingEntry>.Success(x));
                },
                x => x.ToString(),
                _ => false);

        Assert.False(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal("3", value));
    }

    #endregion
}
