namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>Task mirrors of <see cref="ResultExtensionMembers"/>, so chains compose without explicit awaits.</summary>
public static class ResultTaskExtensionMembers
{
    extension<T, TError>(Task<Result<T, TError>> resultTask)
    {
        public async Task<Result<TTarget, TError>> Map<TTarget>(Func<T, TTarget> mapper) =>
            (await resultTask).Map(mapper);

        public async Task<Result<T, TNew>> MapError<TNew>(Func<TError, TNew> mapper) =>
            (await resultTask).MapError(mapper);

        public async Task<Result<T, TNew>> MapError<TNew>(Func<TError, Task<TNew>> mapper) =>
            await (await resultTask).MapError(mapper);

        public async Task<Result<TTarget, TError>> Bind<TTarget, TStepError>(Func<T, Result<TTarget, TStepError>> binder)
            where TStepError : TError =>
            (await resultTask).Bind(binder);

        public async Task<Result<TTarget, TError>> Bind<TTarget, TStepError>(Func<T, Task<Result<TTarget, TStepError>>> binder)
            where TStepError : TError =>
            await (await resultTask).Bind(binder);

        public Task<Result<T, TError>> BindWhen(Func<T, Task<Result<T, TError>>> binder, Func<T, bool> predicate) =>
            resultTask.Bind(v => predicate(v) ? binder(v) : resultTask);

        /// <summary>Maps the value either way; binds first when <paramref name="predicate"/> holds.</summary>
        public Task<Result<TTarget, TError>> BindWhen<TTarget>(
            Func<T, Task<Result<T, TError>>> binder,
            Func<T, TTarget> mapper,
            Func<T, bool> predicate) =>
            resultTask.Bind(v => predicate(v) ? binder(v).Map(mapper) : resultTask.Map(mapper));

        public async Task<Result<T, TError>> BindOnError(Func<TError, Result<T, TError>> binder) =>
            (await resultTask).BindOnError(binder);

        public async Task<Result<T, TError>> BindOnError(Func<TError, Task<Result<T, TError>>> binder) =>
            await (await resultTask).BindOnError(binder);

        public async Task<Result<T, TError>> BindOnErrorWhen(
            Func<TError, Result<T, TError>> binder,
            Func<TError, bool> predicate) =>
            (await resultTask).BindOnErrorWhen(binder, predicate);

        public async Task<Result<T, TError>> BindOnErrorWhen(
            Func<TError, Task<Result<T, TError>>> binder,
            Func<TError, bool> predicate) =>
            await (await resultTask).BindOnErrorWhen(binder, predicate);

        public async Task<Result<T, TError>> Tap(Action<T> action) =>
            (await resultTask).Tap(action);

        public async Task<Result<T, TError>> Tap(Func<T, Task> action) =>
            await (await resultTask).Tap(action);

        public async Task<Result<T, TError>> TapError(Action<TError> action) =>
            (await resultTask).TapError(action);

        public async Task<Result<T, TError>> TapError(Func<TError, Task> action) =>
            await (await resultTask).TapError(action);

        public async Task Match(Action<T> onSuccess, Action<TError> onError) =>
            (await resultTask).Match(onSuccess, onError);

        public async Task Match(Func<T, Task> onSuccess, Func<TError, Task> onError) =>
            await (await resultTask).Match(onSuccess, onError);

        public async Task<TTarget> MatchToValue<TTarget>(Func<T, TTarget> onSuccess, Func<TError, TTarget> onError) =>
            (await resultTask).MatchToValue(onSuccess, onError);

        public async Task<Result<T, TError>> AddLogOnSuccess(Func<T, Action<ILogger>> logAction) =>
            (await resultTask).AddLogOnSuccess(logAction);

        public async Task<Result<T, TError>> AddLogOnFailure(Func<TError, Action<ILogger>> logAction) =>
            (await resultTask).AddLogOnFailure(logAction);

        public async Task<Result<T, TError>> WithOperationName(string value) =>
            (await resultTask).WithOperationName(value);

        public async Task<Result<T, TError>> WithLogProperty(string key, object value) =>
            (await resultTask).WithLogProperty(key, value);

        public async Task<Result<T, TError>> WithLogProperty(string key, Func<T, object> valueSelector) =>
            (await resultTask).WithLogProperty(key, valueSelector);

        public async Task<Result<T, TError>> WithLogProperties(IEnumerable<KeyValuePair<string, object>> props) =>
            (await resultTask).WithLogProperties(props);

        public async Task<Result<T, TError>> FlushLogs(ILogger logger) =>
            (await resultTask).FlushLogs(logger);

        /// <summary>Unlike the sync form, returns the flushed result so it can still be aggregated.</summary>
        public async Task<Result<T, TError>> Complete(ILogger logger) =>
            (await resultTask).FlushLogs(logger);
    }
}
