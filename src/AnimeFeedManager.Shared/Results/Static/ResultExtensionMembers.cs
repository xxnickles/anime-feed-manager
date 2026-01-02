using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for <see cref="Result{T}"/> enabling functional composition,
/// transformation, matching, and logging operations in synchronous contexts.
/// </summary>
public static class ResultExtensionMembers
{
    extension<T>(Result<T> result)
    {
        // ──────────────────────────────────────────────────────────────────
        // Properties
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets whether the result represents a failure (opposite of <see cref="Result{T}.IsSuccess"/>).
        /// </summary>
        public bool IsFailure => !result.IsSuccess;

        // ──────────────────────────────────────────────────────────────────
        // Transformation - Map
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Transforms the success value using the provided mapper function.
        /// If the result is a failure, the error is propagated unchanged.
        /// </summary>
        /// <typeparam name="TTarget">The type to transform to.</typeparam>
        /// <param name="mapper">Function to transform the success value.</param>
        /// <returns>A new <see cref="Result{T}"/> with the transformed value or the original error.</returns>
        public Result<TTarget> Map<TTarget>(Func<T, TTarget> mapper) =>
            result.IsSuccess
                ? new Result<TTarget>(mapper(result.ResultValue!), null, true, result.TraceContext)
                : new Result<TTarget>(default, result.ErrorValue!, false, result.TraceContext);

        /// <summary>
        /// Transforms the success value using the provided async mapper function.
        /// If the result is a failure, the error is propagated unchanged.
        /// </summary>
        /// <typeparam name="TTarget">The type to transform to.</typeparam>
        /// <param name="mapper">Async function to transform the success value.</param>
        /// <returns>A new <see cref="Result{T}"/> with the transformed value or the original error.</returns>
        public async Task<Result<TTarget>> Map<TTarget>(Func<T, Task<TTarget>> mapper) =>
            result.IsSuccess
                ? new Result<TTarget>(await mapper(result.ResultValue!), null, true, result.TraceContext)
                : new Result<TTarget>(default, result.ErrorValue!, false, result.TraceContext);

        /// <summary>
        /// Transforms the error using the provided mapper function.
        /// If the result is a success, it is returned unchanged.
        /// </summary>
        /// <param name="mapper">Function to transform the error.</param>
        /// <returns>A new <see cref="Result{T}"/> with the transformed error or the original success value.</returns>
        public Result<T> MapError(Func<DomainError, DomainError> mapper) =>
            result.IsSuccess
                ? result
                : result with {ErrorValue = mapper(result.ErrorValue!)};

        /// <summary>
        /// Transforms the error using the provided async mapper function.
        /// If the result is a success, it is returned unchanged.
        /// </summary>
        /// <param name="mapper">Async function to transform the error.</param>
        /// <returns>A new <see cref="Result{T}"/> with the transformed error or the original success value.</returns>
        public async Task<Result<T>> MapError(Func<DomainError, Task<DomainError>> mapper) =>
            result.IsSuccess
                ? result
                : result with {ErrorValue = await mapper(result.ErrorValue!)};

        // ──────────────────────────────────────────────────────────────────
        // Chaining - Bind
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Chains a new operation that returns a <see cref="Result{T}"/>.
        /// If the current result is a failure, the error is propagated without executing the binder.
        /// Trace contexts from both results are merged.
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result.</typeparam>
        /// <param name="binder">Function that takes the success value and returns a new result.</param>
        /// <returns>The result from the binder or the propagated error.</returns>
        public Result<TTarget> Bind<TTarget>(Func<T, Result<TTarget>> binder)
        {
            if (!result.IsSuccess)
                return new Result<TTarget>(default, result.ErrorValue, false, result.TraceContext);

            var next = binder(result.ResultValue!);
            return next with {TraceContext = result.TraceContext.Merge(next.TraceContext)};
        }

        /// <summary>
        /// Chains a new async operation that returns a <see cref="Result{T}"/>.
        /// If the current result is a failure, the error is propagated without executing the binder.
        /// Trace contexts from both results are merged.
        /// </summary>
        /// <typeparam name="TTarget">The type of the new result.</typeparam>
        /// <param name="binder">Async function that takes the success value and returns a new result.</param>
        /// <returns>The result from the binder or the propagated error.</returns>
        public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder)
        {
            if (!result.IsSuccess)
                return new Result<TTarget>(default, result.ErrorValue, false, result.TraceContext);

            var next = await binder(result.ResultValue!);
            return next with {TraceContext = result.TraceContext.Merge(next.TraceContext)};
        }

        /// <summary>
        /// Conditionally chains a new operation based on a predicate.
        /// If the predicate returns false, the original value is returned as success.
        /// </summary>
        /// <param name="binder">Function to bind the success value.</param>
        /// <param name="predicate">Function to test whether to apply the binder.</param>
        /// <returns>The result from the binder if predicate is true, otherwise the original value.</returns>
        public Result<T> BindWhen(
            Func<T, Result<T>> binder,
            Func<T, bool> predicate)
            => result.Bind(v => predicate(v) ? binder(v) : Result<T>.Success(v));

        // ──────────────────────────────────────────────────────────────────
        // Side Effects - Tap
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Executes a side effect action on the success value without modifying the result.
        /// Useful for logging, debugging, or other side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Action to execute on the success value.</param>
        /// <returns>The original result unchanged.</returns>
        public Result<T> Tap(Action<T> action)
        {
            if (result.IsSuccess)
                action(result.ResultValue!);

            return result;
        }

        /// <summary>
        /// Executes an async side effect action on the success value without modifying the result.
        /// Useful for async logging, debugging, or other side effects in a fluent chain.
        /// </summary>
        /// <param name="action">Async action to execute on the success value.</param>
        /// <returns>The original result unchanged.</returns>
        public async Task<Result<T>> Tap(Func<T, Task> action)
        {
            if (result.IsSuccess)
                await action(result.ResultValue!);
            return result;
        }

        // ──────────────────────────────────────────────────────────────────
        // Terminal - Match
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Pattern matches on the result, executing one of two actions based on success or failure.
        /// </summary>
        /// <param name="onOk">Action to execute on success.</param>
        /// <param name="onError">Action to execute on error.</param>
        public void Match(Action<T> onOk, Action<DomainError> onError)
        {
            if (result.IsSuccess)
                onOk(result.ResultValue!);
            else
                onError(result.ErrorValue!);
        }

        /// <summary>
        /// Pattern matches on the result, executing one of two async actions based on success or failure.
        /// </summary>
        /// <param name="onOk">Async action to execute on success.</param>
        /// <param name="onError">Async action to execute on error.</param>
        public async Task Match(Func<T, Task> onOk, Func<DomainError, Task> onError)
        {
            if (result.IsSuccess)
                await onOk(result.ResultValue!);
            else
                await onError(result.ErrorValue!);
        }

        /// <summary>
        /// Pattern matches on the result and returns a value based on success or failure.
        /// </summary>
        /// <typeparam name="TTarget">The type of value to return.</typeparam>
        /// <param name="onOk">Function to execute on success.</param>
        /// <param name="onError">Function to execute on error.</param>
        /// <returns>The value returned by either onOk or onError.</returns>
        public TTarget MatchToValue<TTarget>(Func<T, TTarget> onOk, Func<DomainError, TTarget> onError) =>
            result.IsSuccess
                ? onOk(result.ResultValue!)
                : onError(result.ErrorValue!);

        /// <summary>
        /// Terminates a result chain. Use after <see cref="WriteLogs"/> when no further processing is needed.
        /// This is a no-op that exists for fluent API readability.
        /// </summary>
        public void Done()
        {
        }

        // ──────────────────────────────────────────────────────────────────
        // Logging - Trace Context
        // ──────────────────────────────────────────────────────────────────

        /// <summary>
        /// Adds a log action to the result's trace context.
        /// The action will be executed when <see cref="WriteLogs"/> is called.
        /// </summary>
        /// <param name="logAction">The logging action to add.</param>
        /// <returns>The result with the added log action.</returns>
        public Result<T> AddLog(Action<ILogger> logAction) =>
            result with {TraceContext = result.TraceContext.AddLog(logAction)};

        /// <summary>
        /// Adds a log action that will only be executed if the result is successful.
        /// The action receives the success value to include in the log message.
        /// </summary>
        /// <param name="logAction">Function that takes the success value and returns a log action.</param>
        /// <returns>The result with the conditionally added log action.</returns>
        public Result<T> AddLogOnSuccess(Func<T, Action<ILogger>> logAction) =>
            result.IsSuccess
                ? result with {TraceContext = result.TraceContext.AddLog(logAction(result.ResultValue!))}
                : result;

        /// <summary>
        /// Adds a log action that will only be executed if the result is a failure.
        /// The action receives the error to include in the log message.
        /// </summary>
        /// <param name="logAction">Function that takes the error and returns a log action.</param>
        /// <returns>The result with the conditionally added log action.</returns>
        public Result<T> AddLogOnFailure(Func<DomainError, Action<ILogger>> logAction) =>
            result.IsSuccess
                ? result
                : result with {TraceContext = result.TraceContext.AddLog(logAction(result.ErrorValue!))};

        /// <summary>
        /// Adds a property to the trace context that will be included in the logging scope.
        /// </summary>
        /// <param name="key">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <returns>The result with the added property.</returns>
        public Result<T> WithLogProperty(string key, object value) =>
            result with {TraceContext = result.TraceContext.WithProperty(key, value)};

        /// <summary>
        /// Adds an "OperationName" property to the trace context.
        /// Convenience method for <see cref="WithLogProperty"/> with a standard key.
        /// </summary>
        /// <param name="value">The operation name.</param>
        /// <returns>The result with the operation name property.</returns>
        public Result<T> WithOperationName(string value) => result.WithLogProperty("OperationName", value);

        /// <summary>
        /// Adds multiple properties to the trace context that will be included in the logging scope.
        /// </summary>
        /// <param name="props">The properties to add.</param>
        /// <returns>The result with the added properties.</returns>
        public Result<T> WithLogProperties(IEnumerable<KeyValuePair<string, object>> props) =>
            result with {TraceContext = result.TraceContext.WithProperties(props)};

        /// <summary>
        /// Writes all accumulated logs to the provided logger.
        /// On failure, the error's log action is automatically included.
        /// Properties are written as a logging scope.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <returns>The result unchanged (for chaining).</returns>
        public Result<T> WriteLogs(ILogger logger)
        {
            var contextToWrite = result.IsSuccess
                ? result.TraceContext
                : result.TraceContext.AddLog(result.ErrorValue!.LogAction());

            contextToWrite.Write(logger);
            return result;
        }
    }
}