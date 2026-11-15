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
}
