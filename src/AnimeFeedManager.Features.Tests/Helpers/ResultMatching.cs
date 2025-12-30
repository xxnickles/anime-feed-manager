namespace AnimeFeedManager.Features.Tests.Helpers;

internal static class ResultMatching
    {
        /// <summary>
        /// Provides assertion extensions for the <see cref="Result{T}"/> type.
        /// </summary>
        /// <param name="result">The result to assert on.</param>
        /// <typeparam name="T">The type of the success value.</typeparam>
        extension<T>(Result<T> result)
        {
            /// <summary>
            /// Asserts that a result is successful and executes the provided assertion on the success value.
            /// </summary>
            /// <param name="successAssert">The assertion to execute on the success value.</param>
            internal void AssertOnSuccess(Action<T> successAssert) =>
                result.Match(successAssert, _ => Assert.Fail("Result was not a success"));

            /// <summary>
            /// Asserts that a result is successful and executes the provided asynchronous assertion on the success value.
            /// </summary>
            /// <param name="successAssert">The asynchronous assertion to execute on the success value.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            internal void AssertOnSuccess(Func<T, Task> successAssert) => _ = result.Match(
                successAssert, _ =>
                {
                    Assert.Fail("Result was not a success");
                    return Task.CompletedTask;
                });


            /// <summary>
            /// Asserts that the result is a success.
            /// </summary>
            internal void AssertSuccess() =>
                result.Match(_ => Assert.True(true), _ => Assert.Fail("Result was not a success"));

            /// <summary>
            /// Asserts that a result is an error and executes the provided assertion on the <see cref="DomainError"/>.
            /// </summary>
            /// <param name="errorAssert">The assertion to execute on the error.</param>
            internal void AssertOnError(Action<DomainError> errorAssert) =>
                result.Match(_ => Assert.Fail("Result was not an error"), errorAssert);

            /// <summary>
            /// Asserts that the result is an error.
            /// </summary>
            internal void AssertError() =>
                result.Match(_ => Assert.Fail("Result was not an error"), _ => Assert.True(true));
        }
    }