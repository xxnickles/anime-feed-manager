namespace AnimeFeedManager.Features.Tests.Shared.Results.Extensions;

public class CollectionExtensionsTests
{
    [Fact]
    public void Should_Flatten_All_Success_Results()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        var flattened = results.Flatten();

        flattened.AssertOnSuccess(list =>
        {
            Assert.Equal(3, list.Count);
            Assert.Equal([1, 2, 3], list);
        });
    }

    [Fact]
    public void Should_Return_AggregatedError_When_All_Results_Fail()
    {
        var results = new[]
        {
            Result<int>.Failure(NotFoundError.Create("Error 1")),
            Result<int>.Failure(NotFoundError.Create("Error 2")),
            Result<int>.Failure(NotFoundError.Create("Error 3"))
        };

        var flattened = results.Flatten();
        flattened.AssertError();
    }

    [Fact]
    public void Should_Return_AggregatedError_When_Some_Results_Fail()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Failure(NotFoundError.Create("Error 1")),
            Result<int>.Success(3)
        };

        var flattened = results.Flatten();
        flattened.AssertError();
    }

    [Fact]
    public void Should_Flatten_Empty_Collection()
    {
        var results = Array.Empty<Result<int>>();

        var flattened = results.Flatten();

        flattened.AssertOnSuccess(list => Assert.Empty(list));
    }

    [Fact]
    public void Should_GetSuccessValues_From_Mixed_Results()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Failure(NotFoundError.Create("Error")),
            Result<int>.Success(3),
            Result<int>.Failure(NotFoundError.Create("Error 2")),
            Result<int>.Success(5)
        };

        var successValues = results.GetSuccessValues();

        Assert.Equal(3, successValues.Count);
        Assert.Equal([1, 3, 5], successValues);
    }

    [Fact]
    public void Should_GetSuccessValues_From_All_Success_Results()
    {
        var results = new[]
        {
            Result<int>.Success(10),
            Result<int>.Success(20),
            Result<int>.Success(30)
        };

        var successValues = results.GetSuccessValues();

        Assert.Equal(3, successValues.Count);
        Assert.Equal([10, 20, 30], successValues);
    }

    [Fact]
    public void Should_GetSuccessValues_Return_Empty_When_All_Failures()
    {
        var results = new[]
        {
            Result<int>.Failure(NotFoundError.Create("Error 1")),
            Result<int>.Failure(NotFoundError.Create("Error 2"))
        };

        var successValues = results.GetSuccessValues();

        Assert.Empty(successValues);
    }

    [Fact]
    public void Should_GetSuccessValues_From_Empty_Collection()
    {
        var results = Array.Empty<Result<int>>();

        var successValues = results.GetSuccessValues();

        Assert.Empty(successValues);
    }

    [Fact]
    public void Should_Use_Flatten_After_Map_Operations()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        var results = numbers
            .Select(n => Result<int>.Success(n))
            .Select(r => r.Map(x => x * 2))
            .Flatten();

        results.AssertOnSuccess(list => Assert.Equal([2, 4, 6, 8, 10], list));
    }

    [Fact]
    public void Should_Preserve_IsFailure_Property()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure(NotFoundError.Create("Error"));

        Assert.False(success.IsFailure);
        Assert.True(failure.IsFailure);
    }
}
