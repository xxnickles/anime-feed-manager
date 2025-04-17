using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results;

public abstract record Error(string Message)
{
    public abstract void LogError(ILogger logger);
}

public sealed record Result<T>
{
    private readonly T? _resultValue;
    private readonly Error? _errorValueValue;


    private Result(T? resultValue, Error? errorValueValue)
    {
        _resultValue = resultValue;
        _errorValueValue = errorValueValue;
    }

    private bool IsSuccess => _errorValueValue is null;


    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(Error error) => new(default, error);


    public void Match(Action<T> onOk, Action<Error> onError)
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

    public TTarget MatchToValue<TTarget>(Func<T, TTarget> onOk, Func<Error, TTarget> onError)
    {
        return IsSuccess
            ? onOk(_resultValue!)
            : onError(_errorValueValue!);
    }


    public Result<T> MapError(Func<Error, Error> mapper)
    {
        return IsSuccess
            ? new Result<T>(_resultValue, null)
            : new Result<T>(default, mapper(_errorValueValue!));
    }

    public async ValueTask<Result<TTarget>> Bind<TTarget>(Func<T, ValueTask<Result<TTarget>>> binder)
    {
        return IsSuccess ? await binder(_resultValue!) : new Result<TTarget>(default, _errorValueValue);
    }

    public async ValueTask<Result<T>> Apply(Func<T, ValueTask> func)
    {
        if (IsSuccess)
            await func(_resultValue!);

        return this;
    }
}

public static class ResultExtensions
{
    public static async ValueTask Match<T>(this ValueTask<Result<T>> resultTask, Action<T> onSuccess,
        Action<Error> onError)
    {
        var result = await resultTask;
        result.Match(onSuccess, onError);
    }

    public static async ValueTask<TTarget> MatchToValue<T, TTarget>(this ValueTask<Result<T>> resultTask,
        Func<T, TTarget> onSuccess, Func<Error, TTarget> onError)
    {
        var result = await resultTask;
        return result.MatchToValue(onSuccess, onError);
    }

    public static async ValueTask<Result<TTarget>> Map<T, TTarget>(this ValueTask<Result<T>> resultTask,
        Func<T, TTarget> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    public static async ValueTask<Result<T>> MapError<T>(this ValueTask<Result<T>> resultTask,
        Func<Error, Error> mapper)
    {
        var result = await resultTask;
        return result.MapError(mapper);
    }
}