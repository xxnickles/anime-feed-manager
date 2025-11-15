using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Features.Tests.Shared.Results.Extensions;

public class BindExtensionsTests
{
    [Fact]
    public void Should_Bind_Success_Value_To_New_Result()
    {
        var result = Result<int>.Success(42)
            .Bind(x => Result<string>.Success(x.ToString()));

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal("42", value));
    }

    [Fact]
    public void Should_Not_Bind_When_Result_Is_Failure()
    {
        var binderCalled = false;
        var error = NotFoundError.Create("Test error");
        var result = Result<int>.Failure(error)
            .Bind(x =>
            {
                binderCalled = true;
                return Result<string>.Success(x.ToString());
            });

        Assert.False(result.IsSuccess);
        Assert.False(binderCalled);
    }

    [Fact]
    public void Should_Short_Circuit_On_First_Bind_Failure()
    {
        var secondBinderCalled = false;
        var result = Result<int>.Success(42)
            .Bind(x => Result<int>.Failure(NotFoundError.Create("First error")))
            .Bind(x =>
            {
                secondBinderCalled = true;
                return Result<int>.Success(x * 2);
            });

        Assert.False(result.IsSuccess);
        Assert.False(secondBinderCalled);
    }

    [Fact]
    public void Should_Chain_Multiple_Bind_Operations()
    {
        var result = Result<int>.Success(5)
            .Bind(x => Result<int>.Success(x * 2))
            .Bind(x => Result<int>.Success(x + 10))
            .Bind(x => Result<string>.Success(x.ToString()));

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal("20", value));
    }

    [Fact]
    public void Should_Execute_BindWhen_Only_When_Predicate_Is_True()
    {
        var binderCalled = false;
        var result = Result<int>.Success(10)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int>.Success(x * 2);
                },
                x => x > 5);

        Assert.True(result.IsSuccess);
        Assert.True(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(20, value));
    }

    [Fact]
    public void Should_Skip_BindWhen_When_Predicate_Is_False()
    {
        var binderCalled = false;
        var result = Result<int>.Success(3)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int>.Success(x * 2);
                },
                x => x > 5);

        Assert.True(result.IsSuccess);
        Assert.False(binderCalled);
        result.AssertOnSuccess(value => Assert.Equal(3, value));
    }

    [Fact]
    public void Should_Not_Execute_BindWhen_When_Result_Is_Failure()
    {
        var binderCalled = false;
        var predicateCalled = false;
        var error = NotFoundError.Create("Test error");
        var result = Result<int>.Failure(error)
            .BindWhen(
                x =>
                {
                    binderCalled = true;
                    return Result<int>.Success(x * 2);
                },
                x =>
                {
                    predicateCalled = true;
                    return true;
                });

        Assert.False(result.IsSuccess);
        Assert.False(binderCalled);
        Assert.False(predicateCalled);
    }

    [Fact]
    public void Should_Use_BindWhen_For_Conditional_Validation()
    {
        var ValidatePositive = (int x) =>
            x > 0
                ? Result<int>.Success(x)
                : Result<int>.Failure(DomainValidationErrors.Create([
                    DomainValidationError.Create("value", "Must be positive")
                ]));

        var result = Result<int>.Success(10)
            .BindWhen(ValidatePositive, x => x != 0);

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal(10, value));
    }
}
