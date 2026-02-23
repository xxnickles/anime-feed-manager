namespace AnimeFeedManager.Features.Tests.Shared.Results.Static;

public class CollectionExtensionsTests
{
    [Fact]
    public void Should_Return_CompletedBulkResult_When_All_Results_Succeed()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var completed = Assert.IsType<CompletedBulkResult<List<int>>>(bulk);
            Assert.Equal([1, 2, 3], completed.Value);
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

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnError(error =>
        {
            var aggregated = Assert.IsType<AggregatedError>(error);
            Assert.Equal(3, aggregated.Errors.Length);
        });
    }

    [Fact]
    public void Should_Return_PartialSuccessBulkResult_When_Some_Results_Fail()
    {
        // Partial success is a SUCCESS result, not an error — key semantic of BulkResult
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Failure(NotFoundError.Create("Error 1")),
            Result<int>.Success(3)
        };

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<PartialSuccessBulkResult<List<int>>>(bulk);
            Assert.Equal([1, 3], partial.Value);
            Assert.Equal(1, partial.Errors.Length);
        });
    }

    [Fact]
    public void Should_Return_CompletedBulkResult_For_Empty_Collection()
    {
        var results = Array.Empty<Result<int>>();

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var completed = Assert.IsType<CompletedBulkResult<List<int>>>(bulk);
            Assert.Empty(completed.Value);
        });
    }

    [Fact]
    public void Should_Apply_FlattenFunc_To_Collected_Successes()
    {
        var results = new[]
        {
            Result<int>.Success(10),
            Result<int>.Success(20),
            Result<int>.Success(30)
        };

        var flattened = results.Flatten(items => items.Sum());

        flattened.AssertOnSuccess(bulk =>
        {
            var completed = Assert.IsType<CompletedBulkResult<int>>(bulk);
            Assert.Equal(60, completed.Value);
        });
    }

    [Fact]
    public void Should_Partial_Value_Contain_Only_Successful_Items()
    {
        var results = new[]
        {
            Result<int>.Success(2),
            Result<int>.Failure(NotFoundError.Create("Error")),
            Result<int>.Success(4),
            Result<int>.Failure(NotFoundError.Create("Error 2"))
        };

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<PartialSuccessBulkResult<List<int>>>(bulk);
            Assert.Equal([2, 4], partial.Value);
            Assert.Equal(2, partial.Errors.Length);
        });
    }

    [Fact]
    public void Should_Preserve_Transformed_Values_After_Map_And_Flatten()
    {
        var numbers = new[] { 1, 2, 3, 4, 5 };

        var flattened = numbers
            .Select(n => Result<int>.Success(n))
            .Select(r => r.Map(x => x * 2))
            .Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var completed = Assert.IsType<CompletedBulkResult<List<int>>>(bulk);
            Assert.Equal([2, 4, 6, 8, 10], completed.Value);
        });
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
