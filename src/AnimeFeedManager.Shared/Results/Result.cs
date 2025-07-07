namespace AnimeFeedManager.Shared.Results;

public readonly record struct Unit;

// ReSharper disable NullableWarningSuppressionIsUsed
public sealed record Result<T>
{
    private readonly T? _resultValue;
    private readonly DomainError? _errorValueValue;
    private Result(bool isSuccess, T? resultValue, DomainError? errorValueValue) =>
        (IsSuccess, _resultValue, _errorValueValue) = (isSuccess, resultValue, errorValueValue);


    public bool IsSuccess { get; }

    public static Result<Unit> Success() => new(true, new Unit(), null);
    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(DomainError error) => new(false, default, error);


    public void Match(Action<T> onOk, Action<DomainError> onError)
    {
        if (IsSuccess)
            onOk(_resultValue!);
        else
            onError(_errorValueValue!);
    }

    public async Task Match(Func<T, Task> onOk, Func<DomainError, Task> onError)
    {
        if (IsSuccess)
            await onOk(_resultValue!);
        else
            await onError(_errorValueValue!);
    }


    public Result<TTarget> Map<TTarget>(Func<T, TTarget> mapper)
    {
        return IsSuccess
            ? new Result<TTarget>(true, mapper(_resultValue!), null)
            : new Result<TTarget>(false, default, _errorValueValue);
    }

    public async Task<Result<TTarget>> Map<TTarget>(Func<T, Task<TTarget>> mapper)
    {
        return IsSuccess
            ? new Result<TTarget>(true, await mapper(_resultValue!), null)
            : new Result<TTarget>(false, default, _errorValueValue);
    }

    public TTarget MatchToValue<TTarget>(Func<T, TTarget> onOk, Func<DomainError, TTarget> onError)
    {
        return IsSuccess
            ? onOk(_resultValue!)
            : onError(_errorValueValue!);
    }

    public Result<T> MapError(Func<DomainError, DomainError> mapper)
    {
        return IsSuccess
            ? new Result<T>(true, _resultValue, null)
            : new Result<T>(false, default, mapper(_errorValueValue!));
    }

    public async Task<Result<T>> MapError(Func<DomainError, Task<DomainError>> mapper)
    {
        return IsSuccess
            ? new Result<T>(true, _resultValue, null)
            : new Result<T>(false, default, await mapper(_errorValueValue!));
    }


    public Result<TTarget> Bind<TTarget>(Func<T, Result<TTarget>> binder)
    {
        return IsSuccess ? binder(_resultValue!) : new Result<TTarget>(false, default, _errorValueValue);
    }

    public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder)
    {
        return IsSuccess ? await binder(_resultValue!) : new Result<TTarget>(false, default, _errorValueValue);
    }

    public Result<T> Apply(Action<T> func)
    {
        if (IsSuccess)
            func(_resultValue!);
        return this;
    }
}