namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>Binds a step whose family the chain's union accepts through <see cref="IWidensFrom{TSelf, TFamily}"/>.</summary>
// Its own class: these lower to the same signatures as the covariant Bind / BindWhen (constraints aren't part of one),
// and the constraint ties the receiver's TError to the method's TFamily, which an extension block can't express.
public static class ResultWidening
{
    public static Result<TTarget, TError> Bind<T, TError, TTarget, TFamily>(this Result<T, TError> result,
        Func<T, Result<TTarget, TFamily>> binder) where TError : IWidensFrom<TError, TFamily> =>
        result.IsSuccess
            ? ResultExtensionMembers.Continue<TTarget, TError, TError>(result.TraceContext,
                Widen<TTarget, TError, TFamily>(binder(result.ResultValue!)))
            : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

    public static async Task<Result<TTarget, TError>> Bind<T, TError, TTarget, TFamily>(this Result<T, TError> result,
        Func<T, Task<Result<TTarget, TFamily>>> binder) where TError : IWidensFrom<TError, TFamily> =>
        result.IsSuccess
            ? ResultExtensionMembers.Continue<TTarget, TError, TError>(result.TraceContext,
                Widen<TTarget, TError, TFamily>(await binder(result.ResultValue!)))
            : Result<TTarget, TError>.Failure(result.ErrorValue!, result.TraceContext);

    public static Result<T, TError> BindWhen<T, TError, TFamily>(this Result<T, TError> result,
        Func<T, Result<T, TFamily>> binder, Func<T, bool> predicate) where TError : IWidensFrom<TError, TFamily> =>
        result.IsSuccess && predicate(result.ResultValue!) ? result.Bind(binder) : result;

    public static async Task<Result<T, TError>> BindWhen<T, TError, TFamily>(this Result<T, TError> result,
        Func<T, Task<Result<T, TFamily>>> binder, Func<T, bool> predicate) where TError : IWidensFrom<TError, TFamily> =>
        result.IsSuccess && predicate(result.ResultValue!) ? await result.Bind(binder) : result;

    public static async Task<Result<TTarget, TError>> Bind<T, TError, TTarget, TFamily>(
        this Task<Result<T, TError>> resultTask, Func<T, Result<TTarget, TFamily>> binder)
        where TError : IWidensFrom<TError, TFamily> =>
        (await resultTask).Bind(binder);

    public static async Task<Result<TTarget, TError>> Bind<T, TError, TTarget, TFamily>(
        this Task<Result<T, TError>> resultTask, Func<T, Task<Result<TTarget, TFamily>>> binder)
        where TError : IWidensFrom<TError, TFamily> =>
        await (await resultTask).Bind(binder);

    public static async Task<Result<T, TError>> BindWhen<T, TError, TFamily>(
        this Task<Result<T, TError>> resultTask, Func<T, Task<Result<T, TFamily>>> binder, Func<T, bool> predicate)
        where TError : IWidensFrom<TError, TFamily> =>
        await (await resultTask).BindWhen(binder, predicate);

    private static Result<TTarget, TError> Widen<TTarget, TError, TFamily>(Result<TTarget, TFamily> step)
        where TError : IWidensFrom<TError, TFamily> =>
        step.MapError(TError.Widen);
}
