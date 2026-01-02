using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for <see cref="DomainError"/>,
/// enabling error-to-result conversions and direct error logging.
/// </summary>
public static class DomainErrorExtensionMembers
{
    // ──────────────────────────────────────────────────────────────────
    // Result Conversion
    // ──────────────────────────────────────────────────────────────────

    extension<T>(DomainError error)
    {
        /// <summary>
        /// Converts a <see cref="DomainError"/> into a failed <see cref="Result{T}"/>.
        /// </summary>
        /// <returns>A failed <see cref="Result{T}"/> containing the domain error.</returns>
        private Result<T> AsFailure() => Result<T>.Failure(error);

        /// <summary>
        /// Converts a <see cref="DomainError"/> into a <see cref="Task{TResult}"/> containing a failed <see cref="Result{T}"/>.
        /// Useful for returning errors from async methods without additional wrapping.
        /// </summary>
        /// <returns>A completed task containing a failed <see cref="Result{T}"/> with the domain error.</returns>
        public Task<Result<T>> AsTaskFailure() => Task.FromResult(error.AsFailure<T>());
    }

    // ──────────────────────────────────────────────────────────────────
    // Logging
    // ──────────────────────────────────────────────────────────────────

    extension(DomainError error)
    {
        /// <summary>
        /// Writes the error directly to the provided logger using the error's defined log action.
        /// Use this for immediate error logging outside of a result chain.
        /// For logging within result chains, prefer <see cref="ResultExtensionMembers.WriteLogs{T}"/>.
        /// </summary>
        /// <param name="logger">The logger to write the error to.</param>
        public void WriteError(ILogger logger) => error.LogAction().Invoke(logger);
    }
}
