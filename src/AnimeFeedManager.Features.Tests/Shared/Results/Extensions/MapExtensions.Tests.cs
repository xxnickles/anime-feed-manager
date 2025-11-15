using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Features.Tests.Shared.Results.Extensions;

public class MapExtensionsTests
{
    [Fact]
    public void Should_Map_Success_Value_To_New_Type()
    {
        var result = Result<int>.Success(42)
            .Map(x => x.ToString());

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal("42", value));
    }

    [Fact]
    public void Should_Not_Map_When_Result_Is_Failure()
    {
        var error = NotFoundError.Create("Test error");
        var result = Result<int>.Failure(error)
            .Map(x => x.ToString());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Chain_Multiple_Map_Operations()
    {
        var result = Result<int>.Success(5)
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        Assert.True(result.IsSuccess);
        result.AssertOnSuccess(value => Assert.Equal("20", value));
    }

    [Fact]
    public void Should_Preserve_Error_Through_Map_Chain()
    {
        var error = NotFoundError.Create("Test error");
        var result = Result<int>.Failure(error)
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Map_Error_To_New_Error()
    {
        var originalError = NotFoundError.Create("Original error");
        var result = Result<int>.Failure(originalError)
            .MapError(error => new OperationError("MapTest", "Mapped error"));

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Should_Not_Map_Error_When_Result_Is_Success()
    {
        var mapErrorCalled = false;
        var result = Result<int>.Success(42)
            .MapError(error =>
            {
                mapErrorCalled = true;
                return error;
            });

        Assert.True(result.IsSuccess);
        Assert.False(mapErrorCalled);
        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    [Fact]
    public void Should_Transform_Error_Type_With_MapError()
    {
        var originalError = NotFoundError.Create("Not found");
        var result = Result<int>.Failure(originalError)
            .MapError(error => new OperationError("Wrap", $"Wrapped: {error.Message}"));

        Assert.False(result.IsSuccess);
    }
}
