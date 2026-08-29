namespace AnimeFeedManager.Features.Tests.Shared.Results.Static;

public class MatchExtensionsTests
{
    [Fact]
    public void Should_Execute_OnSuccess_Action_When_Result_Is_Success()
    {
        var successCalled = false;
        var errorCalled = false;

        Result<int>.Success(42).Match(
            onOk: value =>
            {
                successCalled = true;
                Assert.Equal(42, value);
            },
            onError: error => errorCalled = true);

        Assert.True(successCalled);
        Assert.False(errorCalled);
    }

    [Fact]
    public void Should_Execute_OnError_Action_When_Result_Is_Failure()
    {
        var successCalled = false;
        var errorCalled = false;
        var testError = NotFoundError.Create("Test error");

        Result<int>.Failure(testError).Match(
            onOk: value => successCalled = true,
            onError: error =>
            {
                errorCalled = true;
                Assert.Equal(testError, error);
            });

        Assert.False(successCalled);
        Assert.True(errorCalled);
    }

    [Fact]
    public void Should_Return_Success_Value_With_MatchToValue()
    {
        var result = Result<int>.Success(42)
            .MatchToValue(
                onOk: value => $"Success: {value}",
                onError: error => $"Error: {error.Message}");

        Assert.Equal("Success: 42", result);
    }

    [Fact]
    public void Should_Return_Error_Value_With_MatchToValue()
    {
        var error = NotFoundError.Create("Not found");
        var result = Result<int>.Failure(error)
            .MatchToValue(
                onOk: value => $"Success: {value}",
                onError: error => $"Error: {error.Message}");

        Assert.Equal("Error: Not found", result);
    }

    [Fact]
    public void Should_Use_MatchToValue_For_Type_Conversion()
    {
        var result = Result<int>.Success(42)
            .MatchToValue(
                onOk: value => value,
                onError: error => -1);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Should_Use_MatchToValue_For_Default_On_Error()
    {
        var error = NotFoundError.Create("Not found");
        var result = Result<int>.Failure(error)
            .MatchToValue(
                onOk: value => value,
                onError: error => 0);

        Assert.Equal(0, result);
    }

    [Fact]
    public void Should_Use_Match_For_Side_Effects()
    {
        var log = new List<string>();

        Result<int>.Success(42).Match(
            onOk: value => log.Add($"Processed: {value}"),
            onError: error => log.Add($"Failed: {error.Message}"));

        Assert.Single(log);
        Assert.Equal("Processed: 42", log[0]);
    }

    [Fact]
    public void Should_Match_At_End_Of_Pipeline()
    {
        var result = new List<string>();

        Result<int>.Success(5)
            .Map(x => x * 2)
            .Bind(x => Result<int>.Success(x + 10))
            .Match(
                onOk: value => result.Add($"Final: {value}"),
                onError: error => result.Add($"Error: {error.Message}"));

        Assert.Single(result);
        Assert.Equal("Final: 20", result[0]);
    }
}
