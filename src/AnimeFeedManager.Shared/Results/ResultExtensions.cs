using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.Extensions.Logging;

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

            return finalResult.ToImmutableList();
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

        return new AggregatedError(errors.ToImmutableList(), errorType);
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

    public static async Task Match<T>(this Task<Result<T>> resultTask, Func<T, Task> onSuccess,
        Func<DomainError, Task> onError)
    {
        await (await resultTask).Match(onSuccess, onError);
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

    public static async Task<Result<T>> MapError<T>(this Task<Result<T>> resultTask,
        Func<DomainError, Task<DomainError>> mapper)
    {
        return await (await resultTask).MapError(mapper);
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
   

    public static Result<T> LogErrors<T>(this Result<T> result, ILogger logger)
    {
        return result.MapError(error =>
        {
            error.LogError(logger);
            return error;
        });
    }

    public static async Task<Result<T>> LogErrors<T>(this Task<Result<T>> result, ILogger logger) =>
        (await result).LogErrors(logger);
}