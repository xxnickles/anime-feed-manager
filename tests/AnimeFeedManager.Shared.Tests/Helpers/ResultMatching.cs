namespace AnimeFeedManager.Shared.Tests.Helpers;

internal static class ResultMatching
{
    extension<T, TError>(Result<T, TError> result)
    {
        internal void AssertOnSuccess(Action<T> successAssert) =>
            result.Match(successAssert, _ => Assert.Fail("Result was not a success"));

        internal void AssertOnSuccess(Func<T, Task> successAssert) => _ = result.Match(
            successAssert, _ =>
            {
                Assert.Fail("Result was not a success");
                return Task.CompletedTask;
            });

        internal void AssertSuccess() =>
            result.Match(_ => Assert.True(true), _ => Assert.Fail("Result was not a success"));

        internal void AssertOnError(Action<TError> errorAssert) =>
            result.Match(_ => Assert.Fail("Result was not an error"), errorAssert);

        internal void AssertError() =>
            result.Match(_ => Assert.Fail("Result was not an error"), _ => Assert.True(true));
    }
}
