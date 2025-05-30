namespace AnimeFeedManager.Features.Tests.Helpers;

internal static class ResultMatching
{
    internal static void AssertOnSuccess<T>(this Result<T> result, Action<T> successAssert) => result.Match(successAssert, _ => Assert.Fail("Result was not a success"));
}