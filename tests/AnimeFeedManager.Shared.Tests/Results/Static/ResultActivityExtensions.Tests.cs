using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace AnimeFeedManager.Shared.Tests.Results.Static;

public sealed class ResultActivityExtensionsTests : IDisposable
{
    private const string SourceName = "AnimeFeedManager.Shared.Tests.ResultActivity";
    private static readonly ActivitySource Source = new(SourceName);

    private readonly ActivityListener _listener = new()
    {
        ShouldListenTo = source => source.Name == SourceName,
        Sample = (ref _) => ActivitySamplingResult.AllDataAndRecorded
    };

    public ResultActivityExtensionsTests() => ActivitySource.AddActivityListener(_listener);

    public void Dispose() => _listener.Dispose();

    #region MarkActivityErroredOnError Tests

    [Fact]
    public void Should_Set_Error_Status_And_Type_When_Result_Fails()
    {
        using var activity = StartActivity();

        Result<int>.Failure(Error.Create("storage unavailable")).MarkActivityErroredOnError();

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal("storage unavailable", activity.StatusDescription);
        Assert.Equal(nameof(Error), activity.GetTagItem(Telemetry.ErrorTypeTag));
    }

    [Fact]
    public void Should_Mark_Error_Without_Description_When_Error_Has_No_Message()
    {
        using var activity = StartActivity();

        Result<int>.Failure(HandledError.Create()).MarkActivityErroredOnError();

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Null(activity.StatusDescription);
        Assert.Equal(nameof(HandledError), activity.GetTagItem(Telemetry.ErrorTypeTag));
    }

    [Fact]
    public void Should_Mark_Error_When_Result_Is_Not_Found()
    {
        using var activity = StartActivity();

        Result<int>.Failure(NotFoundError.Create("series missing")).MarkActivityErroredOnError();

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal(nameof(NotFoundError), activity.GetTagItem(Telemetry.ErrorTypeTag));
    }

    [Fact]
    public void Should_Leave_Status_Unset_When_Result_Is_Success()
    {
        using var activity = StartActivity();

        Result<int>.Success(42).MarkActivityErroredOnError();

        AssertUnmarked(activity);
    }

    [Fact]
    public void Should_Leave_Status_Unset_When_Error_Is_Warning()
    {
        using var activity = StartActivity();

        Result<int>.Failure(Warning.Create("nothing to import")).MarkActivityErroredOnError();

        AssertUnmarked(activity);
    }

    [Fact]
    public void Should_Leave_Status_Unset_When_Error_Is_Domain_Validation()
    {
        using var activity = StartActivity();

        Result<int>.Failure(DomainValidationErrors.Create([DomainValidationError.Create("Email", "required")]))
            .MarkActivityErroredOnError();

        AssertUnmarked(activity);
    }

    [Fact]
    public void Should_Leave_Status_Unset_When_Error_Is_Form_Validation()
    {
        using var activity = StartActivity();

        Result<int>.Failure(new FormDataValidationError<string>([new ValidationResult("required", ["Email"])]))
            .MarkActivityErroredOnError();

        AssertUnmarked(activity);
    }

    [Fact]
    public async Task Should_Mark_Error_When_Awaited_Result_Fails()
    {
        using var activity = StartActivity();

        await Task.FromResult(Result<int>.Failure(Error.Create("boom"))).MarkActivityErroredOnError();

        Assert.Equal(ActivityStatusCode.Error, activity.Status);
    }

    [Fact]
    public void Should_Return_Result_Unchanged_When_No_Activity_Is_Current()
    {
        Assert.Null(Activity.Current);

        var result = Result<int>.Failure(Error.Create("boom")).MarkActivityErroredOnError();

        result.AssertError();
    }

    #endregion

    #region Test Helpers

    private static Activity StartActivity() =>
        Source.StartActivity("Test.Operation") ?? throw new InvalidOperationException("Listener not attached");

    private static void AssertUnmarked(Activity activity)
    {
        Assert.Equal(ActivityStatusCode.Unset, activity.Status);
        Assert.Null(activity.GetTagItem(Telemetry.ErrorTypeTag));
    }

    #endregion
}
