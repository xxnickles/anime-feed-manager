namespace AnimeFeedManager.Shared.Tests.Results.Errors;

public union ItemError(TestError, OtherTestError);

public class BulkErrorTests
{
    [Fact]
    public void Should_Log_The_Message_Then_Each_Failure()
    {
        var logger = new RecordingLogger();
        var error = new BulkError<TestError>.Incomplete("incomplete", [new TestError("err1"), new TestError("err2")]);

        error.WriteError(logger);

        Assert.Equal(["incomplete", "err1", "err2"], logger.Entries);
    }

    [Fact]
    public void Should_Expose_Union_Item_Errors_As_Their_Domain_Errors()
    {
        BulkError error = new BulkError<ItemError>.AllFailed([new TestError("first"), new OtherTestError("second")]);

        Assert.Equal(["first", "second"], error.Failures.Select(failure => failure.Message));
    }
}
