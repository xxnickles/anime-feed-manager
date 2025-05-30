using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results;

public readonly record struct Unit;

public abstract record DomainError(string Message)
{
    public string Message { get; } = Message;

    public override string ToString()
    {
        return Message;
    }

    public abstract void LogError(ILogger logger);
}

// ReSharper disable NullableWarningSuppressionIsUsed
public sealed record Result<T>
{
    private readonly T? _resultValue;
    private readonly DomainError? _errorValueValue;


    private Result(T? resultValue, DomainError? errorValueValue)
    {
        _resultValue = resultValue;
        _errorValueValue = errorValueValue;
    }

    public bool IsSuccess => _errorValueValue is null;


    public static Result<Unit> Success() => new(new Unit(), null);
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(DomainError error) => new(default, error);


    public void Match(Action<T> onOk, Action<DomainError> onError)
    {
        if (IsSuccess)
            onOk(_resultValue!);
        else
            onError(_errorValueValue!);
    }

    public Result<TTarget> Map<TTarget>(Func<T, TTarget> mapper)
    {
        return IsSuccess
            ? new Result<TTarget>(mapper(_resultValue!), null)
            : new Result<TTarget>(default, _errorValueValue);
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
            ? new Result<T>(_resultValue, null)
            : new Result<T>(default, mapper(_errorValueValue!));
    }

    public Result<TTarget> Bind<TTarget>(Func<T, Result<TTarget>> binder)
    {
        return IsSuccess ? binder(_resultValue!) : new Result<TTarget>(default, _errorValueValue);
    }
    
    public async Task<Result<TTarget>> Bind<TTarget>(Func<T, Task<Result<TTarget>>> binder)
    {
        return IsSuccess ? await binder(_resultValue!) : new Result<TTarget>(default, _errorValueValue);
    }

    public Result<T> Apply(Action<T> func)
    {
        if (IsSuccess)
            func(_resultValue!);
        return this;
    }
}