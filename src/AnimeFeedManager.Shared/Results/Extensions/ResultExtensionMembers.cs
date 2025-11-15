using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Extensions;

public static class ResultExtensionMembers
{
    /// <summary>
    /// Extension members for <see cref="Task{TResult}"/> of <see cref="Result{T}"/> - provides async operations for Result chaining
    /// </summary>
    extension<T>(Task<Result<T>> resultTask)
    {
        /// <summary>
        /// Matches the result to execute one of two actions based on success or failure
        /// </summary>
        /// <param name="onSuccess">Action to execute on success</param>
        /// <param name="onError">Action to execute on error</param>
        public async Task Match(Action<T> onSuccess, Action<DomainError> onError)
            => (await resultTask).Match(onSuccess, onError);

        /// <summary>
        /// Matches the result to execute one of two async actions based on success or failure
        /// </summary>
        /// <param name="onSuccess">Async action to execute on success</param>
        /// <param name="onError">Async action to execute on error</param>
        public async Task Match(Func<T, Task> onSuccess, Func<DomainError, Task> onError)
            => await (await resultTask).Match(onSuccess, onError);

        /// <summary>
        /// Matches the result and returns a value based on success or failure
        /// </summary>
        /// <typeparam name="TTarget">The type of value to return</typeparam>
        /// <param name="onSuccess">Function to execute on success</param>
        /// <param name="onError">Function to execute on error</param>
        /// <returns>The value returned by either onSuccess or onError</returns>
        public async Task<TTarget> MatchToValue<TTarget>(
            Func<T, TTarget> onSuccess,
            Func<DomainError, TTarget> onError)
            => (await resultTask).MatchToValue(onSuccess, onError);

        /// <summary>
        /// Maps the success value to a new type
        /// </summary>
        /// <typeparam name="TTarget">The type to map to</typeparam>
        /// <param name="mapper">Function to transform the success value</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped value</returns>
        public async Task<Result<TTarget>> Map<TTarget>(Func<T, TTarget> mapper)
            => (await resultTask).Map(mapper);

        /// <summary>
        /// Maps the error to a new error
        /// </summary>
        /// <param name="mapper">Function to transform the error</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped error</returns>
        public async Task<Result<T>> MapError(Func<DomainError, DomainError> mapper)
            => (await resultTask).MapError(mapper);

        /// <summary>
        /// Maps the error to a new error asynchronously
        /// </summary>
        /// <param name="mapper">Async function to transform the error</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped error</returns>
        public async Task<Result<T>> MapError(Func<DomainError, Task<DomainError>> mapper)
            => await (await resultTask).MapError(mapper);

        /// <summary>
        /// Binds the success value to a new <see cref="Result{T}"/>
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result</typeparam>
        /// <param name="binder">Function to bind the success value</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder</returns>
        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Result<TTarget>> binder)
            => (await resultTask).Bind(binder);

        /// <summary>
        /// Binds the success value to a new <see cref="Result{T}"/> asynchronously
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result</typeparam>
        /// <param name="binder">Async function to bind the success value</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder</returns>
        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder)
            => await (await resultTask).Bind(binder);

        /// <summary>
        /// Executes a side effect action on the success value without modifying the result.
        /// Useful for logging, debugging, or other side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Action to execute on the success value</param>
        /// <returns>The original result unchanged</returns>
        public async Task<Result<T>> Tap(Action<T> action)
            => (await resultTask).Tap(action);

        /// <summary>
        /// Executes an async side effect action on the success value without modifying the result.
        /// Useful for async logging, debugging, or other side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Async action to execute on the success value</param>
        /// <returns>The original result unchanged</returns>
        public async Task<Result<T>> Tap(Func<T, Task> action)
        {
            var r = await resultTask;
            if (r.IsSuccess)
                await r.Match(action, _ => Task.CompletedTask);
            return r;
        }

        /// <summary>
        /// Conditionally applies the binder only if the predicate is true.
        /// If predicate is false, returns success with the original value.
        /// </summary>
        /// <param name="binder">Async function to bind the success value</param>
        /// <param name="predicate">Function to test whether to apply the binder</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder if predicate is true, otherwise the original value</returns>
        public Task<Result<T>> BindWhen(
            Func<T, Task<Result<T>>> binder,
            Func<T, bool> predicate)
            => resultTask.Bind(v =>
                predicate(v) ? binder(v) : Task.FromResult(Result<T>.Success(v)));

        /// <summary>
        /// Logs errors using the provided <see cref="ILogger"/>
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <returns>The same result after logging any errors</returns>
        public async Task<Result<T>> LogErrors(ILogger logger)
            => (await resultTask).LogErrors(logger);
    }

    /// <summary>
    /// Extension members for <see cref="IEnumerable{T}"/> of <see cref="Result{T}"/> - provides collection operations
    /// </summary>
    extension<T>(IEnumerable<Result<T>> results)
    {
        /// <summary>
        /// Flattens a collection of <see cref="Result{T}"/> into a single result containing a list.
        /// Returns success only if all results are successful, otherwise returns an <see cref="AggregatedError"/>.
        /// </summary>
        /// <returns>A <see cref="Result{T}"/> containing an <see cref="ImmutableList{T}"/> of all success values, or an aggregated error</returns>
        public Result<ImmutableList<T>> Flatten()
        {
            var oks = results.Count(x => x.IsSuccess);
            if (oks == results.Count())
            {
                var finalResult = results.Select(r => r.MatchToValue(
                    v => v,
                    _ => throw new InvalidOperationException("List contains errors which should not be possible")
                ));

                return finalResult.ToImmutableList();
            }

            var errorType = oks switch
            {
                0 => FailureType.Total,
                _ => FailureType.Partial
            };

            var errors = results.Where(r => !r.IsSuccess).Select(r => r.MatchToValue(
                _ => throw new InvalidOperationException("List contains valid values which should not be possible"),
                error => error
            ));

            return new AggregatedError(errors.ToImmutableList(), errorType);
        }

        /// <summary>
        /// Gets only the successful values from the collection, discarding failures
        /// </summary>
        /// <returns>An <see cref="ImmutableList{T}"/> containing only successful values</returns>
        public ImmutableList<T> GetSuccessValues()
        {
            return results.Where(r => r.IsSuccess).Select(r => r.MatchToValue(
                v => v,
                _ => throw new InvalidOperationException("List contains errors which should not be possible")
            )).ToImmutableList();
        }
    }

    /// <summary>
    /// Extension members for <see cref="IEnumerable{T}"/> of <see cref="Task{TResult}"/> of <see cref="Result{T}"/> - provides async collection operations
    /// </summary>
    extension<T>(IEnumerable<Task<Result<T>>> results)
    {
        /// <summary>
        /// Flattens a collection of async results into a single result containing a list
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with an <see cref="ImmutableList{T}"/></returns>
        public async Task<Result<ImmutableList<T>>> Flatten()
            => (await Task.WhenAll(results.ToArray())).Flatten();
    }

    /// <summary>
    /// Extension members for <see cref="Task{TResult}"/> of <see cref="IEnumerable{T}"/> of <see cref="Result{T}"/> - provides async collection operations
    /// </summary>
    extension<T>(Task<IEnumerable<Result<T>>> results)
    {
        /// <summary>
        /// Flattens an async collection of results into a single result containing a list
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with an <see cref="ImmutableList{T}"/></returns>
        public async Task<Result<ImmutableList<T>>> Flatten()
            => (await results).Flatten();
    }

    /// <summary>
    /// Extension members for <see cref="Result{T}"/> instance - provides instance-level operations
    /// </summary>
    extension<T>(Result<T> result)
    {
        /// <summary>
        /// Gets whether the result represents a failure (opposite of <see cref="Result{T}.IsSuccess"/>)
        /// </summary>
        public bool IsFailure => !result.IsSuccess;

        /// <summary>
        /// Executes a side effect action on the success value without modifying the result.
        /// Useful for logging, debugging, or other side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Action to execute on the success value</param>
        /// <returns>The original result unchanged</returns>
        public Result<T> Tap(Action<T> action)
        {
            if (result.IsSuccess)
                result.Match(action, _ => { });
            return result;
        }

        /// <summary>
        /// Conditionally applies the binder only if the predicate is true.
        /// If predicate is false, returns success with the original value.
        /// </summary>
        /// <param name="binder">Function to bind the success value</param>
        /// <param name="predicate">Function to test whether to apply the binder</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder if predicate is true, otherwise the original value</returns>
        public Result<T> BindWhen(
            Func<T, Result<T>> binder,
            Func<T, bool> predicate)
            => result.Bind(v => predicate(v) ? binder(v) : Result<T>.Success(v));

        /// <summary>
        /// Logs errors using the provided <see cref="ILogger"/>
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <returns>The same result after logging any errors</returns>
        public Result<T> LogErrors(ILogger logger)
        {
            return result.MapError(error =>
            {
                error.LogError(logger);
                return error;
            });
        }
    }
}
