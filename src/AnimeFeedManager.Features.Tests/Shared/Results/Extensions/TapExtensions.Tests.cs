namespace AnimeFeedManager.Features.Tests.Shared.Results.Extensions;

public class TapExtensionsTests
{
    [Fact]
    public void Should_Execute_Tap_Action_When_Result_Is_Success()
    {
        var executed = false;
        var result = Result<int>.Success(42)
            .Tap(x =>
            {
                executed = true;
                Assert.Equal(42, x);
            });

        Assert.True(executed);
        result.AssertSuccess();
    }

    [Fact]
    public void Should_Not_Execute_Tap_Action_When_Result_Is_Failure()
    {
        var executed = false;
        var error = NotFoundError.Create("Test error");
        var result = Result<int>.Failure(error)
            .Tap(x => executed = true);

        Assert.False(executed);
        result.AssertError();
    }

    [Fact]
    public void Should_Return_Original_Result_From_Tap()
    {
        var result = Result<int>.Success(42);
        var tappedResult = result.Tap(x => { });

        Assert.Same(result, tappedResult);
    }

    [Fact]
    public async Task Should_Execute_Tap_Action_When_Async_Result_Is_Success()
    {
        var executed = false;
        var result = await Task.FromResult(Result<int>.Success(42))
            .Tap(x =>
            {
                executed = true;
                Assert.Equal(42, x);
            });

        Assert.True(executed);
        result.AssertSuccess();
    }

    [Fact]
    public async Task Should_Not_Execute_Tap_Action_When_Async_Result_Is_Failure()
    {
        var executed = false;
        var error = NotFoundError.Create("Test error");
        var result = await Task.FromResult(Result<int>.Failure(error))
            .Tap(x => executed = true);

        Assert.False(executed);
        result.AssertError();
    }

    [Fact]
    public async Task Should_Execute_Async_Tap_Action_When_Result_Is_Success()
    {
        var executed = false;
        var result = await Task.FromResult(Result<int>.Success(42))
            .Tap(async x =>
            {
                await Task.Delay(1);
                executed = true;
                Assert.Equal(42, x);
            });

        Assert.True(executed);
        result.AssertSuccess();
    }

    [Fact]
    public async Task Should_Not_Execute_Async_Tap_Action_When_Result_Is_Failure()
    {
        var executed = false;
        var error = NotFoundError.Create("Test error");
        var result = await Task.FromResult(Result<int>.Failure(error))
            .Tap(async x =>
            {
                await Task.Delay(1);
                executed = true;
            });

        Assert.False(executed);
        result.AssertError();
    }

    [Fact]
    public void Should_Preserve_Fluent_Chain_With_Tap()
    {
        var log = new List<int>();

        var result = Result<int>.Success(5)
            .Map(x => x * 2)
            .Tap(x => log.Add(x))
            .Map(x => x + 10)
            .Tap(x => log.Add(x));

        Assert.Equal([10, 20], log);
        result.AssertOnSuccess(value => Assert.Equal(20, value));
    }

    [Fact]
    public async Task Should_Preserve_Async_Fluent_Chain_With_Tap()
    {
        var log = new List<int>();

        var result = await Task.FromResult(Result<int>.Success(5))
            .Map(x => x * 2)
            .Tap(x => log.Add(x))
            .Map(x => x + 10)
            .Tap(async x =>
            {
                await Task.Delay(1);
                log.Add(x);
            });

        Assert.Equal([10, 20], log);
        result.AssertOnSuccess(value => Assert.Equal(20, value));
    }
}
