namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides helper extension methods for constructing <see cref="Result{T}"/> instances, including both successful and failed results.
/// </summary>
public static class ResultExtensions
{
    extension<T>(DomainError error)
    {
        /// <summary>
        /// Converts a <see cref="DomainError"/> into a failed <see cref="Result{T}"/>.
        /// </summary>
        /// <returns>A failed <see cref="Result{T}"/> containing the domain error.</returns>
        private Result<T> AsFailure() => Result<T>.Failure(error);
        
        /// <summary>
        /// Converts a <see cref="DomainError"/> into a <see cref="Task{TResult}"/> that represents a failed <see cref="Result{T}"/>.
        /// </summary>
        /// <returns>A completed task containing a failed <see cref="Result{T}"/> with the domain error.</returns>
        public Task<Result<T>> AsTaskFailure() => Task.FromResult(error.AsFailure<T>());
    }
}