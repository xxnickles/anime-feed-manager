namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class CollectionExtensionsTests
{
    [Fact]
    public void Should_Return_CompletedBulkResult_When_All_Results_Succeed()
    {
        var results = new[]
        {
            Result<int, DomainError>.Success(1),
            Result<int, DomainError>.Success(2),
            Result<int, DomainError>.Success(3)
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
            Result<int, DomainError>.Failure(new TestError("Error 1")),
            Result<int, DomainError>.Failure(new TestError("Error 2")),
            Result<int, DomainError>.Failure(new TestError("Error 3"))
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
            Result<int, DomainError>.Success(1),
            Result<int, DomainError>.Failure(new TestError("Error 1")),
            Result<int, DomainError>.Success(3)
        };

        var flattened = results.Flatten(items => items.ToList());

        flattened.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<PartialSuccessBulkResult<List<int>>>(bulk);
            Assert.Equal([1, 3], partial.Value);
            Assert.Single(partial.Errors);
        });
    }

    [Fact]
    public void Should_Return_CompletedBulkResult_For_Empty_Collection()
    {
        var results = Array.Empty<Result<int, DomainError>>();

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
            Result<int, DomainError>.Success(10),
            Result<int, DomainError>.Success(20),
            Result<int, DomainError>.Success(30)
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
            Result<int, DomainError>.Success(2),
            Result<int, DomainError>.Failure(new TestError("Error")),
            Result<int, DomainError>.Success(4),
            Result<int, DomainError>.Failure(new TestError("Error 2"))
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
            .Select(n => Result<int, DomainError>.Success(n))
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
        var success = Result<int, DomainError>.Success(42);
        var failure = Result<int, DomainError>.Failure(new TestError("Error"));

        Assert.False(success.IsFailure);
        Assert.True(failure.IsFailure);
    }

    [Fact]
    public void Should_Aggregate_Derived_Errors_When_Flattening_Results_Of_A_Narrower_Error_Type()
    {
        var results = new[]
        {
            Result<int, MissingEntry>.Success(1),
            Result<int, MissingEntry>.Failure(new MissingEntry("key"))
        };

        var flattened = results.Flatten(values => values.Sum());

        flattened.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<PartialSuccessBulkResult<int>>(bulk);
            Assert.Equal(1, partial.Value);
            Assert.IsType<MissingEntry>(Assert.Single(partial.Errors));
        });
    }

    #region Collect Tests

    [Fact]
    public void Should_Return_Completed_When_Collecting_And_All_Results_Succeed()
    {
        var results = new[] { Result<int, TestError>.Success(1), Result<int, TestError>.Success(2) };

        var collected = results.Collect(items => items.ToList());

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

        var collected = results.Collect(items => items.ToList());

        collected.AssertOnSuccess(bulk =>
        {
            var partial = Assert.IsType<BulkResult<List<int>, TestError>.Partial>(bulk);
            Assert.Equal([1, 3], partial.Value);
            Assert.Equal([failure], partial.Errors);
        });
    }

    [Fact]
    public void Should_Fail_With_AllFailed_When_Collecting_And_All_Results_Fail()
    {
        var results = new[]
        {
            Result<int, TestError>.Failure(new TestError("Error 1")),
            Result<int, TestError>.Failure(new TestError("Error 2"))
        };

        var collected = results.Collect(items => items.ToList());

        collected.AssertOnError(error =>
            Assert.Equal(["Error 1", "Error 2"], error.Errors.Select(item => item.Message)));
    }

    [Fact]
    public void Should_Return_Completed_When_Collecting_An_Empty_Collection()
    {
        var collected = Array.Empty<Result<int, TestError>>().Collect(items => items.ToList());

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

        results.Collect(items => items.ToList()).FlushLogs(logger);

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

        results.Collect(items => items.ToList()).FlushLogs(logger);

        Assert.Contains("gone", logger.Entries);
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

    #region Trace Context Tests

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Flattening()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, DomainError>.Success(1).AddLogOnSuccess(_ => log => log.LogInformation("first")),
            Result<int, DomainError>.Success(2).AddLogOnSuccess(_ => log => log.LogInformation("second"))
        };

        results.Flatten(items => items.ToList()).FlushLogs(logger);

        Assert.Equal(["first", "second"], logger.Entries);
    }

    [Fact]
    public void Should_Keep_Log_Properties_Staged_On_Items_When_Flattening()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, DomainError>.Success(1)
                .WithLogProperty("First", "a")
                .AddLogOnSuccess(_ => log => log.LogInformation("first")),
            Result<int, DomainError>.Success(2).WithLogProperty("Second", "b")
        };

        results.Flatten(items => items.ToList()).FlushLogs(logger);

        Assert.Contains(new KeyValuePair<string, object>("First", "a"), logger.ScopeProperties);
        Assert.Contains(new KeyValuePair<string, object>("Second", "b"), logger.ScopeProperties);
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Failed_Items_When_Flattening()
    {
        var logger = new RecordingLogger();
        var results = new[]
        {
            Result<int, DomainError>.Success(1),
            Result<int, DomainError>.Failure(new TestError("gone"))
                .AddLogOnFailure(error => log => log.LogWarning("{Message}", error.Message))
        };

        results.Flatten(items => items.ToList()).FlushLogs(logger);

        Assert.Contains("gone", logger.Entries);
    }

    #endregion
}
