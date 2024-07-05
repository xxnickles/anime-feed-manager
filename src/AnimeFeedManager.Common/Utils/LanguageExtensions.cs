using AnimeFeedManager.Common.Domain.Errors;

namespace AnimeFeedManager.Common.Utils;

public static class LanguageExtensions
{
    public static Either<DomainError, TR> ValidationToEither<TR>(this Validation<ValidationError, TR> validation) =>
        validation.ToEither().MapLeft(errors => (DomainError)ValidationErrors.Create(errors));


    public static Either<DomainError, ImmutableList<T>> FlattenResults<T>(this Either<DomainError, T>[] results)
    {
        var oks = results.Rights().ToImmutableList();
        if (oks.Count == results.Length)
            return results.Rights().ToImmutableList();

        var errorType = !oks.IsEmpty switch
        {
            true => AggregatedError.FailureType.Partial,
            false => AggregatedError.FailureType.Total
        };

        return new AggregatedError(results.Lefts().ToImmutableList(), errorType);
    }

    public static async Task<Either<DomainError, ImmutableList<T>>> FlattenResults<T>(
        this Task<Either<DomainError, T>[]> results) => (await results).FlattenResults();


    public static ImmutableList<T> Flatten<T>(this ImmutableList<Option<T>> options)
    {
        var results = new List<T>();
        foreach (var option in options)
        {
            option.Match(
                value => results.Add(value),
                () => {}
            );
        }

        return results.ToImmutableList();
    }
}