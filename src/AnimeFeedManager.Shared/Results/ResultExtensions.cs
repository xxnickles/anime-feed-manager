using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

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

    public static ImmutableList<T> GetSuccessValues<T>(this IEnumerable<Result<T>> results)
    {
        return results.Where(r => r.IsSuccess).Select(r => r.MatchToValue(
            v => v,
            _ => throw new InvalidOperationException("List contains errors which should not be possible")
        )).ToImmutableList();
    }

    public static async Task Match<T>(this Task<Result<T>> resultTask, Action<T> onSuccess,
        Action<DomainError> onError)
    {
        (await resultTask).Match(onSuccess, onError);
    }

    public static async Task<TTarget> MatchToValue<T, TTarget>(this Task<Result<T>> resultTask,
        Func<T, TTarget> onSuccess, Func<DomainError, TTarget> onError)
    {
        return (await resultTask).MatchToValue(onSuccess, onError);
    }

    public static async Task<Result<TTarget>> Map<T, TTarget>(this Task<Result<T>> resultTask,
        Func<T, TTarget> mapper)
    {
        return (await resultTask).Map(mapper);
    }

    public static async Task<Result<T>> MapError<T>(this Task<Result<T>> resultTask,
        Func<DomainError, DomainError> mapper)
    {
        return (await resultTask).MapError(mapper);
    }

    public static async Task<Result<TTarget>> Bind<T, TTarget>(this Task<Result<T>> resultTask,
        Func<T, Result<TTarget>> binder)
    {
        return (await resultTask).Bind(binder);
    }
    
    public static async Task<Result<TTarget>> Bind<T, TTarget>(this Task<Result<T>> resultTask,
        Func<T, Task<Result<TTarget>>> binder)
    {
        return await (await resultTask).Bind(binder);
    }
}