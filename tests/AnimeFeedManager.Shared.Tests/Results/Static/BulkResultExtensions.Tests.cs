namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class BulkResultExtensionsTests
{
    #region Errors Tests

    [Fact]
    public void Should_Expose_No_Errors_When_Bulk_Is_Completed()
    {
        BulkResult<int, TestError> bulk = new BulkResult<int, TestError>.Completed(42);

        Assert.Empty(bulk.Errors);
    }

    [Fact]
    public void Should_Expose_The_Failed_Items_When_Bulk_Is_Partial()
    {
        var failure = new TestError("err");
        BulkResult<int, TestError> bulk = new BulkResult<int, TestError>.Partial(7, [failure]);

        Assert.Equal([failure], bulk.Errors);
    }

    [Fact]
    public void Should_Log_Each_Error_When_Logging_Errors_Of_A_Partial_Bulk()
    {
        var logger = new RecordingLogger();
        BulkResult<int, TestError> bulk = new BulkResult<int, TestError>.Partial(5, [new TestError("err1"), new TestError("err2")]);

        bulk.LogErrors(logger);

        Assert.Equal(["err1", "err2"], logger.Entries);
    }

    [Fact]
    public void Should_Log_Nothing_When_Logging_Errors_Of_A_Completed_Bulk()
    {
        var logger = new RecordingLogger();
        BulkResult<int, TestError> bulk = new BulkResult<int, TestError>.Completed(42);

        bulk.LogErrors(logger);

        Assert.Empty(logger.Entries);
    }

    #endregion

    #region RequireAll Tests

    [Fact]
    public void Should_Return_The_Value_When_Requiring_All_Of_A_Completed_Bulk()
    {
        var required = new[] { Result<int, TestError>.Success(1), Result<int, TestError>.Success(2) }
            .Collect(items => items.Sum())
            .RequireAll("incomplete");

        required.AssertOnSuccess(value => Assert.Equal(3, value));
    }

    [Fact]
    public void Should_Fail_With_Incomplete_When_Requiring_All_Of_A_Partial_Bulk()
    {
        var failure = new TestError("err");

        var required = new[] { Result<int, TestError>.Success(1), Result<int, TestError>.Failure(failure) }
            .Collect(items => items.Sum())
            .RequireAll("incomplete");

        required.AssertOnError(error =>
        {
            var incomplete = Assert.IsType<BulkError<TestError>.Incomplete>(error);
            Assert.Equal("incomplete", incomplete.Message);
            Assert.Equal([failure], incomplete.Errors);
        });
    }

    [Fact]
    public void Should_Keep_AllFailed_When_Requiring_All_Of_A_Bulk_Where_All_Failed()
    {
        var required = new[] { Result<int, TestError>.Failure(new TestError("err")) }
            .Collect(items => items.Sum())
            .RequireAll("incomplete");

        required.AssertOnError(error => Assert.IsType<BulkError<TestError>.AllFailed>(error));
    }

    [Fact]
    public void Should_Keep_Log_Actions_Staged_On_Items_When_Requiring_All()
    {
        var logger = new RecordingLogger();

        new[]
            {
                Result<int, TestError>.Success(1).AddLogOnSuccess(_ => log => log.LogInformation("staged")),
                Result<int, TestError>.Failure(new TestError("err"))
            }
            .Collect(items => items.Sum())
            .RequireAll("incomplete")
            .FlushLogs(logger);

        Assert.Contains("staged", logger.Entries);
    }

    #endregion
}
