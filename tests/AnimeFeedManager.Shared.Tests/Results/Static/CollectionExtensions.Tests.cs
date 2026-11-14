namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class CollectionExtensionsTests
{
    [Fact]
    public void Should_Preserve_IsFailure_Property()
    {
        var success = Result<int, DomainError>.Success(42);
        var failure = Result<int, DomainError>.Failure(new TestError("Error"));

        Assert.False(success.IsFailure);
        Assert.True(failure.IsFailure);
    }

    #region Collect Tests

    [Fact]
    public void Should_Return_Completed_When_Collecting_And_All_Results_Succeed()
    {
        var results = new[] { Result<int, TestError>.Success(1), Result<int, TestError>.Success(2) };

        var collected = results.Collect(items => items.ToList(), AllFailed);

        collected.AssertOnSuccess(bulk =>
            Assert.Equal([1, 2], Assert.IsType<BulkResult<List<int>, TestError>.Completed>(bulk).Value));
    }

    [Fact]
    public void Should_Return_Partial_With_Typed_Errors_When_Collecting_And_Some_Results_Fail()
    {
        var failure = new TestError("Error");
        var results = new[]
        {
            Result<int, TestError>.Success(1),
            Result<int, TestError>.Failure(failure),
            Result<int, TestError>.Success(3)
        };

        var collected = results.Collect(items => items.ToList(), AllFailed);

        collected.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<BulkResult<List<int>, TestError>.Partial>(bulk);
            Assert.Equal([1, 3], partial.Value);
            Assert.Equal([failure], partial.Errors);
        });
    }

    [Fact]
    public void Should_Fail_With_The_Callers_Error_Built_From_Every_Item_When_Collecting_And_All_Results_Fail()
    {
        var results = new[]
        {
            Result<int, TestError>.Failure(new TestError("Error 1")),
            Result<int, TestError>.Failure(new TestError("Error 2"))
        };

        var collected = results.Collect(items => items.ToList(), AllFailed);

        collected.AssertOnError(error => Assert.Equal(new OtherTestError("Error 1, Error 2"), error));
    }

    [Fact]
    public void Should_Return_Completed_When_Collecting_An_Empty_Collection()
    {
        var collected = Array.Empty<Result<int, TestError>>().Collect(items => items.ToList(), AllFailed);

        collected.AssertOnSuccess(bulk =>
            Assert.Empty(Assert.IsType<BulkResult<List<int>, TestError>.Completed>(bulk).Value));
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Collecting()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Success(1).AddLogOnSuccess(_ => log => log.LogInformation("first")),
            Result<int, TestError>.Failure(new TestError("gone"))
                .AddLogOnFailure(error => log => log.LogWarning("{Message}", error.Message))
        };

        results.Collect(items => items.ToList(), AllFailed).FlushLogs(logger);

        Assert.Equal(["first", "gone"], logger.Entries);
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Collecting_And_All_Results_Fail()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Failure(new TestError("gone"))
                .AddLogOnFailure(error => log => log.LogWarning("{Message}", error.Message))
        };

        results.Collect(items => items.ToList(), AllFailed).FlushLogs(logger);

        Assert.Contains("gone", logger.Entries);
    }

    [Fact]
    public void Should_Keep_Log_Properties_Staged_On_Items_When_Collecting()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Success(1)
                .WithLogProperty("First", "a")
                .AddLogOnSuccess(_ => log => log.LogInformation("first")),
            Result<int, TestError>.Failure(new TestError("gone")).WithLogProperty("Second", "b")
        };

        results.Collect(items => items.ToList(), AllFailed).FlushLogs(logger);

        Assert.Contains(new KeyValuePair<string, object>("First", "a"), logger.ScopeProperties);
        Assert.Contains(new KeyValuePair<string, object>("Second", "b"), logger.ScopeProperties);
    }

    #endregion

    #region CollectAll Tests

    [Fact]
    public void Should_Return_The_Flattened_Values_When_Collecting_All_And_Every_Result_Succeeds()
    {
        var results = new[] { Result<int, TestError>.Success(1), Result<int, TestError>.Success(2) };

        var collected = results.CollectAll(items => items.Sum(), AllFailed);

        collected.AssertOnSuccess(value => Assert.Equal(3, value));
    }

    [Fact]
    public void Should_Fail_With_The_Callers_Error_Built_From_The_Failures_When_Collecting_All_And_Some_Fail()
    {
        var results = new[]
        {
            Result<int, TestError>.Success(1),
            Result<int, TestError>.Failure(new TestError("gone"))
        };

        var collected = results.CollectAll(items => items.Sum(), AllFailed);

        collected.AssertOnError(error => Assert.Equal(new OtherTestError("gone"), error));
    }

    [Fact]
    public void Should_Return_The_Flattened_Empty_Value_When_Collecting_All_From_An_Empty_Collection()
    {
        var collected = Array.Empty<Result<int, TestError>>().CollectAll(items => items.Sum(), AllFailed);

        collected.AssertOnSuccess(value => Assert.Equal(0, value));
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Collecting_All_And_Some_Fail()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Success(1).AddLogOnSuccess(_ => log => log.LogInformation("kept")),
            Result<int, TestError>.Failure(new TestError("gone"))
        };

        results.CollectAll(items => items.Sum(), AllFailed).FlushLogs(logger);

        Assert.Equal(["kept", "gone"], logger.Entries);
    }

    #endregion

    #region Merge Tests

    [Fact]
    public void Should_Return_Completed_When_Merging_Bulks_Without_Errors()
    {
        var merged = new[] { Completed(1), Completed(2) }.Merge(values => values.Sum(), AllFailed);

        merged.AssertOnSuccess(bulk =>
            Assert.Equal(3, Assert.IsType<BulkResult<int, TestError>.Completed>(bulk).Value));
    }

    [Fact]
    public void Should_Concatenate_Item_Errors_When_Merging_A_Partial_Bulk()
    {
        var first = new TestError("first");
        var second = new TestError("second");

        var merged = new[] { Partial(1, first), Completed(2), Partial(3, second) }
            .Merge(values => values.Sum(), AllFailed);

        merged.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<BulkResult<int, TestError>.Partial>(bulk);
            Assert.Equal(6, partial.Value);
            Assert.Equal([first, second], partial.Errors);
        });
    }

    [Fact]
    public void Should_Keep_A_Failed_Bulks_Error_As_An_Item_Error_When_Merging_With_A_Success()
    {
        var lost = new TestError("lost");

        var merged = new[] { Completed(1), Failed(lost) }.Merge(values => values.Sum(), AllFailed);

        merged.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<BulkResult<int, TestError>.Partial>(bulk);
            Assert.Equal(1, partial.Value);
            Assert.Equal([lost], partial.Errors);
        });
    }

    [Fact]
    public void Should_Fail_With_The_Callers_Error_Built_From_Every_Bulk_When_Merging_Only_Failed_Bulks()
    {
        var merged = new[] { Failed(new TestError("first")), Failed(new TestError("second")) }
            .Merge(values => values.Sum(), AllFailed);

        merged.AssertOnError(error => Assert.Equal(new OtherTestError("first, second"), error));
    }

    [Fact]
    public void Should_Return_Completed_When_Merging_No_Bulks()
    {
        var merged = Array.Empty<Result<BulkResult<int, TestError>, TestError>>()
            .Merge(values => values.Sum(), AllFailed);

        merged.AssertOnSuccess(bulk =>
            Assert.Equal(0, Assert.IsType<BulkResult<int, TestError>.Completed>(bulk).Value));
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Bulks_When_Merging()
    {
        var logger = new RecordingLogger();
        var bulks = new[]
        {
            Completed(1).AddLogOnSuccess(_ => log => log.LogInformation("kept")),
            Failed(new TestError("gone")).AddLogOnFailure(_ => log => log.LogInformation("failed"))
        };

        bulks.Merge(values => values.Sum(), AllFailed).FlushLogs(logger);

        Assert.Equal(["kept", "failed"], logger.Entries);
    }

    #endregion

    #region CollectSuccesses Tests

    [Fact]
    public void Should_Flatten_Only_Successes_When_Collecting_Successes_With_Some_Failures()
    {
        var results = new[]
        {
            Result<int, TestError>.Success(1),
            Result<int, TestError>.Failure(new TestError("Error")),
            Result<int, TestError>.Success(3)
        };

        var collected = results.CollectSuccesses(items => items.ToList());

        collected.AssertOnSuccess(values => Assert.Equal([1, 3], values));
    }

    [Fact]
    public void Should_Succeed_With_Empty_Value_When_Collecting_Successes_And_All_Fail()
    {
        var results = new[]
        {
            Result<int, TestError>.Failure(new TestError("Error 1")),
            Result<int, TestError>.Failure(new TestError("Error 2"))
        };

        var collected = results.CollectSuccesses(items => items.ToList());

        collected.AssertOnSuccess(Assert.Empty);
    }

    [Fact]
    public void Should_Stage_Each_Failure_Log_When_Collecting_Successes()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Failure(new TestError("first")),
            Result<int, TestError>.Success(2),
            Result<int, TestError>.Failure(new TestError("second"))
        };

        results.CollectSuccesses(items => items.ToList()).FlushLogs(logger);

        Assert.Equal(["first", "second"], logger.Entries);
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Collecting_Successes()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, TestError>.Success(1).AddLogOnSuccess(_ => log => log.LogInformation("staged"))
        };

        results.CollectSuccesses(items => items.ToList()).FlushLogs(logger);

        Assert.Equal(["staged"], logger.Entries);
    }

    #endregion

    #region Test Helpers

    private static OtherTestError AllFailed(ImmutableArray<TestError> errors) =>
        new(string.Join(", ", errors.Select(error => error.Message)));

    private static Result<BulkResult<int, TestError>, TestError> Completed(int value) =>
        Result<BulkResult<int, TestError>, TestError>.Success(new BulkResult<int, TestError>.Completed(value));

    private static Result<BulkResult<int, TestError>, TestError> Partial(int value, TestError error) =>
        Result<BulkResult<int, TestError>, TestError>.Success(new BulkResult<int, TestError>.Partial(value, [error]));

    private static Result<BulkResult<int, TestError>, TestError> Failed(TestError error) =>
        Result<BulkResult<int, TestError>, TestError>.Failure(error);

    #endregion
}
