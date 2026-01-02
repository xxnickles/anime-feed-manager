using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for <see cref="Task{TResult}"/> of <see cref="Result{T}"/>,
/// enabling async operations in result chains without explicit awaiting.
/// These are async wrappers that delegate to <see cref="ResultExtensionMembers"/>.
/// </summary>
public static class ResultTaskExtensionMembers
{
    extension<T>(Task<Result<T>> resultTask)
    {
        // ──────────────────────────────────────────────────────────────────
        // Transformation - Map
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Transforms the success value using the provided mapper function.
        /// Async wrapper for <see cref="ResultExtensionMembers.Map{T,TTarget}(Result{T}, Func{T, TTarget})"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type to map to.</typeparam>
        /// <param name="mapper">Function to transform the success value.</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped value.</returns>
        public async Task<Result<TTarget>> Map<TTarget>(Func<T, TTarget> mapper)
            => (await resultTask).Map(mapper);

        /// <summary>
        /// Transforms the error using the provided mapper function.
        /// Async wrapper for <see cref="ResultExtensionMembers.MapError{T}(Result{T}, Func{DomainError, DomainError})"/>.
        /// </summary>
        /// <param name="mapper">Function to transform the error.</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped error.</returns>
        public async Task<Result<T>> MapError(Func<DomainError, DomainError> mapper)
            => (await resultTask).MapError(mapper);

        /// <summary>
        /// Transforms the error using the provided async mapper function.
        /// Async wrapper for <see cref="ResultExtensionMembers.MapError{T}(Result{T}, Func{DomainError, Task{DomainError}})"/>.
        /// </summary>
        /// <param name="mapper">Async function to transform the error.</param>
        /// <returns>A <see cref="Result{T}"/> with the mapped error.</returns>
        public async Task<Result<T>> MapError(Func<DomainError, Task<DomainError>> mapper)
            => await (await resultTask).MapError(mapper);

        // ──────────────────────────────────────────────────────────────────
        // Chaining - Bind
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Chains a new operation that returns a <see cref="Result{T}"/>.
        /// Async wrapper for <see cref="ResultExtensionMembers.Bind{T,TTarget}(Result{T}, Func{T, Result{TTarget}})"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result.</typeparam>
        /// <param name="binder">Function to bind the success value.</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder.</returns>
        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Result<TTarget>> binder)
            => (await resultTask).Bind(binder);

        /// <summary>
        /// Chains a new async operation that returns a <see cref="Result{T}"/>.
        /// Async wrapper for <see cref="ResultExtensionMembers.Bind{T,TTarget}(Result{T}, Func{T, Task{Result{TTarget}}})"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result.</typeparam>
        /// <param name="binder">Async function to bind the success value.</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder.</returns>
        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder)
            => await (await resultTask).Bind(binder);

        /// <summary>
        /// Conditionally chains a new operation based on a predicate.
        /// Async wrapper for <see cref="ResultExtensionMembers.BindWhen{T}(Result{T}, Func{T, Result{T}}, Func{T, bool})"/>.
        /// </summary>
        /// <param name="binder">Async function to bind the success value.</param>
        /// <param name="predicate">Function to test whether to apply the binder.</param>
        /// <returns>A new <see cref="Result{T}"/> from the binder if predicate is true, otherwise the original value.</returns>
        public Task<Result<T>> BindWhen(
            Func<T, Task<Result<T>>> binder,
            Func<T, bool> predicate)
            => resultTask.Bind(v =>
                predicate(v) ? binder(v) : Task.FromResult(Result<T>.Success(v)));

        // ──────────────────────────────────────────────────────────────────
        // Side Effects - Tap
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a side effect action on the success value without modifying the result.
        /// Async wrapper for <see cref="ResultExtensionMembers.Tap{T}(Result{T}, Action{T})"/>.
        /// </summary>
        /// <param name="action">Action to execute on the success value.</param>
        /// <returns>The original result unchanged.</returns>
        public async Task<Result<T>> Tap(Action<T> action)
            => (await resultTask).Tap(action);

        /// <summary>
        /// Executes an async side effect action on the success value without modifying the result.
        /// Async wrapper for <see cref="ResultExtensionMembers.Tap{T}(Result{T}, Func{T, Task})"/>.
        /// </summary>
        /// <param name="action">Async action to execute on the success value.</param>
        /// <returns>The original result unchanged.</returns>
        public async Task<Result<T>> Tap(Func<T, Task> action)
        {
            var r = await resultTask;
            if (r.IsSuccess)
                await r.Tap(action);
            return r;
        }

        // ──────────────────────────────────────────────────────────────────
        // Terminal - Match
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Pattern matches on the result, executing one of two actions based on success or failure.
        /// Async wrapper for <see cref="ResultExtensionMembers.Match{T}(Result{T}, Action{T}, Action{DomainError})"/>.
        /// </summary>
        /// <param name="onSuccess">Action to execute on success.</param>
        /// <param name="onError">Action to execute on error.</param>
        public async Task Match(Action<T> onSuccess, Action<DomainError> onError)
            => (await resultTask).Match(onSuccess, onError);

        /// <summary>
        /// Pattern matches on the result, executing one of two async actions based on success or failure.
        /// Async wrapper for <see cref="ResultExtensionMembers.Match{T}(Result{T}, Func{T, Task}, Func{DomainError, Task})"/>.
        /// </summary>
        /// <param name="onSuccess">Async action to execute on success.</param>
        /// <param name="onError">Async action to execute on error.</param>
        public async Task Match(Func<T, Task> onSuccess, Func<DomainError, Task> onError)
            => await (await resultTask).Match(onSuccess, onError);

        /// <summary>
        /// Pattern matches on the result and returns a value based on success or failure.
        /// Async wrapper for <see cref="ResultExtensionMembers.MatchToValue{T,TTarget}(Result{T}, Func{T, TTarget}, Func{DomainError, TTarget})"/>.
        /// </summary>
        /// <typeparam name="TTarget">The type of value to return.</typeparam>
        /// <param name="onSuccess">Function to execute on success.</param>
        /// <param name="onError">Function to execute on error.</param>
        /// <returns>The value returned by either onSuccess or onError.</returns>
        public async Task<TTarget> MatchToValue<TTarget>(
            Func<T, TTarget> onSuccess,
            Func<DomainError, TTarget> onError)
            => (await resultTask).MatchToValue(onSuccess, onError);

        /// <summary>
        /// Terminates a result chain. Use after <see cref="WriteLogs"/> when no further processing is needed.
        /// Async wrapper for <see cref="ResultExtensionMembers.Done{T}(Result{T})"/>.
        /// </summary>
        public async Task Done() => (await resultTask).Done();

        // ──────────────────────────────────────────────────────────────────
        // Logging - Trace Context
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a log action that will only be executed if the result is successful.
        /// Async wrapper for <see cref="ResultExtensionMembers.AddLogOnSuccess{T}(Result{T}, Func{T, Action{ILogger}})"/>.
        /// </summary>
        /// <param name="logAction">Function that takes the success value and returns a log action.</param>
        /// <returns>The result with the conditionally added log action.</returns>
        public async Task<Result<T>> AddLogOnSuccess(Func<T, Action<ILogger>> logAction)
            => (await resultTask).AddLogOnSuccess(logAction);

        /// <summary>
        /// Adds an "OperationName" property to the trace context.
        /// Async wrapper for <see cref="ResultExtensionMembers.WithOperationName{T}(Result{T}, string)"/>.
        /// </summary>
        /// <param name="value">The operation name.</param>
        /// <returns>The result with the operation name property.</returns>
        public async Task<Result<T>> WithOperationName(string value)
            => (await resultTask).WithLogProperty("OperationName", value);

        /// <summary>
        /// Adds a property to the trace context that will be included in the logging scope.
        /// Async wrapper for <see cref="ResultExtensionMembers.WithLogProperty{T}(Result{T}, string, object)"/>.
        /// </summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The result with the added property.</returns>
        public async Task<Result<T>> WithLogProperty(string key, object value)
            => (await resultTask).WithLogProperty(key, value);

        /// <summary>
        /// Adds multiple properties to the trace context that will be included in the logging scope.
        /// Async wrapper for <see cref="ResultExtensionMembers.WithLogProperties{T}(Result{T}, IEnumerable{KeyValuePair{string, object}})"/>.
        /// </summary>
        /// <param name="props">The properties to add.</param>
        /// <returns>The result with the added properties.</returns>
        public async Task<Result<T>> WithLogProperties(IEnumerable<KeyValuePair<string, object>> props)
            => (await resultTask).WithLogProperties(props);

        /// <summary>
        /// Writes all accumulated logs to the provided logger.
        /// Async wrapper for <see cref="ResultExtensionMembers.WriteLogs{T}(Result{T}, ILogger)"/>.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <returns>The result unchanged (for chaining).</returns>
        public async Task<Result<T>> WriteLogs(ILogger logger)
            => (await resultTask).WriteLogs(logger);
    }
}
