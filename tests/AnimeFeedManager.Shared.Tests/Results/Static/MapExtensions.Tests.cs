namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class MapExtensionsTests
{
    [Fact]
    public void Should_Map_Success_Value_To_New_Type()
    {
        var result = Result<int, DomainError>.Success(42)
            .Map(x => x.ToString());

        result.AssertOnSuccess(value => Assert.Equal("42", value));
    }

    [Fact]
    public void Should_Not_Map_When_Result_Is_Failure()
    {
        var error = new TestError("Test error");
        var result = Result<int, DomainError>.Failure(error)
            .Map(x => x.ToString());

        result.AssertError();
    }

    [Fact]
    public void Should_Chain_Multiple_Map_Operations()
    {
        var result = Result<int, DomainError>.Success(5)
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        result.AssertOnSuccess(value => Assert.Equal("20", value));
    }

    [Fact]
    public void Should_Preserve_Error_Through_Map_Chain()
    {
        var error = new TestError("Test error");
        var result = Result<int, DomainError>.Failure(error)
            .Map(x => x * 2)
            .Map(x => x + 10)
            .Map(x => x.ToString());

        result.AssertError();
    }

    [Fact]
    public void Should_Change_Error_Type_When_MapError_Runs_On_Failure()
    {
        Result<int, OtherTestError> result = Result<int, DomainError>.Failure(new TestError("Not found"))
            .MapError(error => new OtherTestError($"Wrapped: {error.Message}"));

        result.AssertOnError(error => Assert.Equal("Wrapped: Not found", error.Message));
    }

    [Fact]
    public void Should_Not_Map_Error_When_Result_Is_Success()
    {
        var mapErrorCalled = false;
        var result = Result<int, DomainError>.Success(42)
            .MapError(error =>
            {
                mapErrorCalled = true;
                return error;
            });

        Assert.False(mapErrorCalled);
        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    [Fact]
    public void Should_Keep_Trace_Context_When_MapError_Changes_Error_Type()
    {
        var logger = new RecordingLogger();

        Result<int, DomainError>.Failure(new TestError("Not found"))
            .AddLog(log => log.LogInformation("staged"))
            .WithLogProperty("Key", "value")
            .MapError(error => new OtherTestError($"Wrapped: {error.Message}"))
            .FlushLogs(logger);

        Assert.Equal(["staged", "Wrapped: Not found"], logger.Entries);
        Assert.Contains(new KeyValuePair<string, object>("Key", "value"), logger.ScopeProperties);
    }

    [Fact]
    public void Should_Keep_Trace_Context_When_MapError_Runs_On_Success()
    {
        var logger = new RecordingLogger();

        Result<int, DomainError>.Success(42)
            .AddLog(log => log.LogInformation("staged"))
            .MapError(error => new OtherTestError(error.Message))
            .FlushLogs(logger);

        Assert.Equal(["staged"], logger.Entries);
    }
}
