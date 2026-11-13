namespace AnimeFeedManager.Shared.Results.Static;

// ReSharper disable NullableWarningSuppressionIsUsed
public static class ResultExtensionMembers
{
    extension<T, TError>(Result<T, TError> result)
    {
        public bool IsFailure => !result.IsSuccess;

        public Result<TTarget, TError> Map<TTarget>(Func<T, TTarget> mapper) =>
            result.IsSuccess
                ? Result<TTarget, TError>.Success(mapper(result.ResultValue!), result.TraceContext)
                : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

        public async Task<Result<TTarget, TError>> Map<TTarget>(Func<T, Task<TTarget>> mapper) =>
            result.IsSuccess
                ? Result<TTarget, TError>.Success(await mapper(result.ResultValue!), result.TraceContext)
                : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

        /// <summary>Changes the error type; widens into a path union with <c>MapError(e => (PathError)e)</c>.</summary>
        public Result<T, TNew> MapError<TNew>(Func<TError, TNew> mapper) =>
            result.IsSuccess
                ? Result<T, TNew>.Success(result.ResultValue!, result.TraceContext)
                : Result<T, TNew>.Failure(mapper(result.ErrorValue!), result.TraceContext);

        public async Task<Result<T, TNew>> MapError<TNew>(Func<TError, Task<TNew>> mapper) =>
            result.IsSuccess
                ? Result<T, TNew>.Success(result.ResultValue!, result.TraceContext)
                : Result<T, TNew>.Failure(await mapper(result.ErrorValue!), result.TraceContext);

        /// <summary>Runs <paramref name="binder"/> on success; a step error deriving from <typeparamref name="TError"/> widens into it, and both trace contexts are merged.</summary>
        public Result<TTarget, TError> Bind<TTarget, TStepError>(Func<T, Result<TTarget, TStepError>> binder)
            where TStepError : TError =>
            result.IsSuccess
                ? Continue<TTarget, TError, TStepError>(result.TraceContext, binder(result.ResultValue!))
                : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

        public async Task<Result<TTarget, TError>> Bind<TTarget, TStepError>(Func<T, Task<Result<TTarget, TStepError>>> binder)
            where TStepError : TError =>
            result.IsSuccess
                ? Continue<TTarget, TError, TStepError>(result.TraceContext, await binder(result.ResultValue!))
                : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

        public Result<T, TError> BindWhen<TStepError>(Func<T, Result<T, TStepError>> binder, Func<T, bool> predicate)
            where TStepError : TError =>
            result.IsSuccess && predicate(result.ResultValue!) ? result.Bind(binder) : result;

        /// <summary>Runs <paramref name="binder"/> on failure; the trace contexts of both results are merged.</summary>
        public Result<T, TError> BindOnError(Func<TError, Result<T, TError>> binder)
        {
            if (result.IsSuccess)
                return result;

            var next = binder(result.ErrorValue!);
            return next with { TraceContext = result.TraceContext.Merge(next.TraceContext) };
        }

        public async Task<Result<T, TError>> BindOnError(Func<TError, Task<Result<T, TError>>> binder)
        {
            if (result.IsSuccess)
                return result;

            var next = await binder(result.ErrorValue!);
            return next with { TraceContext = result.TraceContext.Merge(next.TraceContext) };
        }

        public Result<T, TError> BindOnErrorWhen(Func<TError, Result<T, TError>> binder, Func<TError, bool> predicate) =>
            result.IsFailure && predicate(result.ErrorValue!) ? result.BindOnError(binder) : result;

        public async Task<Result<T, TError>> BindOnErrorWhen(
            Func<TError, Task<Result<T, TError>>> binder,
            Func<TError, bool> predicate) =>
            result.IsFailure && predicate(result.ErrorValue!) ? await result.BindOnError(binder) : result;

        public Result<T, TError> Tap(Action<T> action)
        {
            if (result.IsSuccess)
                action(result.ResultValue!);
            return result;
        }

        public async Task<Result<T, TError>> Tap(Func<T, Task> action)
        {
            if (result.IsSuccess)
                await action(result.ResultValue!);
            return result;
        }

        public Result<T, TError> TapError(Action<TError> action)
        {
            if (result.IsFailure)
                action(result.ErrorValue!);
            return result;
        }

        public async Task<Result<T, TError>> TapError(Func<TError, Task> action)
        {
            if (result.IsFailure)
                await action(result.ErrorValue!);
            return result;
        }

        public void Match(Action<T> onOk, Action<TError> onError)
        {
            if (result.IsSuccess)
                onOk(result.ResultValue!);
            else
                onError(result.ErrorValue!);
        }

        public async Task Match(Func<T, Task> onOk, Func<TError, Task> onError)
        {
            if (result.IsSuccess)
                await onOk(result.ResultValue!);
            else
                await onError(result.ErrorValue!);
        }

        public TTarget MatchToValue<TTarget>(Func<T, TTarget> onOk, Func<TError, TTarget> onError) =>
            result.IsSuccess ? onOk(result.ResultValue!) : onError(result.ErrorValue!);

        public Result<T, TError> AddLog(Action<ILogger> logAction) =>
            result with { TraceContext = result.TraceContext.AddLog(logAction) };

        public Result<T, TError> AddLogOnSuccess(Func<T, Action<ILogger>> logAction) =>
            result.IsSuccess
                ? result with { TraceContext = result.TraceContext.AddLog(logAction(result.ResultValue!)) }
                : result;

        public Result<T, TError> AddLogOnFailure(Func<TError, Action<ILogger>> logAction) =>
            result.IsSuccess
                ? result
                : result with { TraceContext = result.TraceContext.AddLog(logAction(result.ErrorValue!)) };

        public Result<T, TError> WithLogProperty(string key, object value) =>
            result with { TraceContext = result.TraceContext.WithProperty(key, value) };

        /// <summary>Adds a property from the success value; on failure the selector isn't invoked.</summary>
        public Result<T, TError> WithLogProperty(string key, Func<T, object> valueSelector) =>
            result.IsFailure
                ? result
                : result with { TraceContext = result.TraceContext.WithProperty(key, valueSelector(result.ResultValue!)) };

        public Result<T, TError> WithOperationName(string value) =>
            result.WithLogProperty(Telemetry.OperationNameTag, value);

        public Result<T, TError> WithLogProperties(IEnumerable<KeyValuePair<string, object>> props) =>
            result with { TraceContext = result.TraceContext.WithProperties(props) };

        /// <summary>Writes the trace context, plus the error's log action on failure, then clears it.</summary>
        public Result<T, TError> FlushLogs(ILogger logger)
        {
            var contextToWrite = result.IsSuccess
                ? result.TraceContext
                : result.TraceContext.AddLog(ErrorCases.AsDomainError(result.ErrorValue!).LogAction());

            contextToWrite.Write(logger);
            return result with { TraceContext = TraceContext.Empty };
        }

        public void Complete(ILogger logger) => result.FlushLogs(logger);
    }

    private static Result<TTarget, TError> Continue<TTarget, TError, TStepError>(
        TraceContext previous,
        Result<TTarget, TStepError> next) where TStepError : TError
    {
        var traceContext = previous.Merge(next.TraceContext);
        return next.IsSuccess
            ? Result<TTarget, TError>.Success(next.ResultValue!, traceContext)
            : Result<TTarget, TError>.Failure(next.ErrorValue!, traceContext);
    }
}
