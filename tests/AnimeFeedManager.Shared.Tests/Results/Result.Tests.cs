namespace AnimeFeedManager.Shared.Tests.Results;

public union LookupError(NotFoundError, Error);

public class ResultTests
{
    #region Union Conversions

    [Fact]
    public void Should_Hold_Value_When_Value_Is_Assigned_To_Result()
    {
        Result<int, DomainError> result = 42;

        result.AssertOnSuccess(value => Assert.Equal(42, value));
    }

    [Fact]
    public void Should_Hold_Error_When_Leaf_Error_Is_Assigned_To_Result()
    {
        Result<int, DomainError> result = NotFoundError.Create("gone");

        result.AssertOnError(error => Assert.IsType<NotFoundError>(error));
    }

    #endregion

    #region Pattern Matching

    [Fact]
    public void Should_Match_Value_Case_When_Result_Is_Success()
    {
        var result = Result<int, DomainError>.Success(42);

        var description = Describe(result);

        Assert.Equal("value 42", description);
    }

    [Fact]
    public void Should_Match_Error_Case_When_Result_Is_Failure()
    {
        var result = Result<int, DomainError>.Failure(NotFoundError.Create("gone"));

        var description = Describe(result);

        Assert.Equal("error gone", description);
    }

    [Fact]
    public void Should_Match_Leaf_Case_When_Error_Is_Widened_Into_A_Path_Union()
    {
        var result = Result<int, NotFoundError>.Failure(NotFoundError.Create("gone"))
            .MapError(error => (LookupError)error);

        var description = result switch
        {
            int value => $"value {value}",
            LookupError error => error switch
            {
                NotFoundError => "not found",
                Error => "error"
            }
        };

        Assert.Equal("not found", description);
    }

    [Fact]
    public void Should_Return_False_From_TryGetValue_When_Asking_For_The_Other_Case()
    {
        var result = Result<int, DomainError>.Success(42);

        var hasError = result.TryGetValue(out DomainError _);

        Assert.False(hasError);
    }

    [Fact]
    public void Should_Report_No_Value_When_Success_Value_Is_Null()
    {
        var result = Result<string?, DomainError>.Success(null);

        Assert.False(result.HasValue);
    }

    #endregion

    #region Recovery On A Path Union

    [Fact]
    public void Should_Recover_When_Path_Error_Matches_The_Predicate()
    {
        var result = LookupFailure(NotFoundError.Create("gone"))
            .BindOnErrorWhen(_ => 0, error => error is NotFoundError);

        result.AssertOnSuccess(value => Assert.Equal(0, value));
    }

    [Fact]
    public void Should_Keep_Failure_When_Path_Error_Does_Not_Match_The_Predicate()
    {
        var result = LookupFailure(Error.Create("broken"))
            .BindOnErrorWhen(_ => 0, error => error is NotFoundError);

        result.AssertOnError(error => Assert.True(error is Error));
    }

    #endregion

    #region Logging

    [Fact]
    public void Should_Log_The_Case_Error_When_A_Path_Union_Failure_Is_Flushed()
    {
        var logger = new RecordingLogger();

        LookupFailure(NotFoundError.Create("gone")).FlushLogs(logger);

        Assert.Equal(["gone"], logger.Entries);
    }

    [Fact]
    public void Should_Throw_InvalidOperationException_When_Flushing_A_Failure_That_Is_Not_A_DomainError()
    {
        var result = Result<int, string>.Failure("oops");

        Assert.Throws<InvalidOperationException>(() => result.FlushLogs(new RecordingLogger()));
    }

    #endregion

    #region Test Helpers

    private static string Describe(Result<int, DomainError> result) => result switch
    {
        int value => $"value {value}",
        DomainError error => $"error {error.Message}"
    };

    private static Result<int, LookupError> LookupFailure(LookupError error) =>
        Result<int, LookupError>.Failure(error);

    #endregion
}
