namespace AnimeFeedManager.Features.Tests.Helpers;

internal static class ResultMatching
{
    /// <summary>
    /// Asserts that a result is successful and executes the provided assertion on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to assert on.</param>
    /// <param name="successAssert">The assertion to execute on the success value.</param>

    internal static void AssertOnSuccess<T>(this Result<T> result, Action<T> successAssert) =>
        result.Match(successAssert, _ => Assert.Fail("Result was not a success"));

    /// <summary>
    /// Asserts that a result is successful and executes the provided asynchronous assertion on the success value.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to assert on.</param>
    /// <param name="successAssert">The asynchronous assertion to execute on the success value.</param>
    /// <returns>A task representing the asynchronous operation.</returns>

    internal static void AssertOnSuccess<T>(this Result<T> result, Func<T, Task> successAssert) => result.Match(
        successAssert, _ =>
        {
            Assert.Fail("Result was not a success");
            return Task.CompletedTask;
        });
}