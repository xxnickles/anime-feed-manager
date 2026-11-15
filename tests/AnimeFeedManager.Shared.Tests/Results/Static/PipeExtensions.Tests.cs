namespace AnimeFeedManager.Shared.Tests.Results.Static;

public class PipeExtensionsTests
{
    #region Pipe

    [Fact]
    public void Should_Pass_Value_To_Next_Function()
    {
        var result = 21.Pipe(value => value * 2);

        Assert.Equal(42, result);
    }

    [Fact]
    public void Should_Enter_A_Result_Chain_When_Piped_To_Success()
    {
        var result = "catalog".Pipe(Result<string, DomainError>.Success);

        result.AssertOnSuccess(value => Assert.Equal("catalog", value));
    }

    #endregion

    #region Then

    [Fact]
    public async Task Should_Pass_Task_Value_To_Next_Function()
    {
        var result = await Task.FromResult(21).Then(value => value * 2);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Should_Enter_A_Result_Chain_When_Task_Is_Piped_To_Success()
    {
        var result = await Task.FromResult("catalog").Then(Result<string, DomainError>.Success);

        result.AssertOnSuccess(value => Assert.Equal("catalog", value));
    }

    [Fact]
    public async Task Should_Propagate_Exception_Without_Calling_Next_When_Task_Faults()
    {
        var nextCalled = false;
        var faulted = Task.FromException<int>(new InvalidOperationException("faulted"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => faulted.Then(value =>
        {
            nextCalled = true;
            return value;
        }));

        Assert.False(nextCalled);
    }

    #endregion
}
