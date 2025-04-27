namespace AnimeFeedManager.Common.Results;

public static class ResultExtensions
{
    public static Result<ImmutableList<T>> Flattern<T>(this IEnumerable<Result<T>> results)
    {
        var oks = results.Count(x => x.IsSuccess);
        if (oks == results.Count())
        {
            var finalResult = results.Select(r => r.MatchToValue(
                v => v,
                _ => throw new InvalidOperationException("List contains errors which should not be possible")
            ));
            
            return Result<ImmutableList<T>>.Success(finalResult.ToImmutableList());
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
        
        return Result<ImmutableList<T>>.Failure(new AggregatedError(errors.ToImmutableList(), errorType));
    }
    
    public static async ValueTask Match<T>(this ValueTask<Result<T>> resultTask, Action<T> onSuccess,
        Action<DomainError> onError)
    {
        var result = await resultTask;
        result.Match(onSuccess, onError);
    }

    public static async ValueTask<TTarget> MatchToValue<T, TTarget>(this ValueTask<Result<T>> resultTask,
        Func<T, TTarget> onSuccess, Func<DomainError, TTarget> onError)
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
        Func<DomainError, DomainError> mapper)
    {
        var result = await resultTask;
        return result.MapError(mapper);
    }

    public static async ValueTask<Result<U>> Bind<T, U>(this ValueTask<Result<T>> resultTask, 
        Func<T, ValueTask<Result<U>>> binder)
    {
        var result = await resultTask;
        return await result.Bind(binder);
    }

    
}