namespace AnimeFeedManager.Features.Common.Utils;

public static class LanguageExtensions
{
    public static Either<DomainError, TR> ValidationToEither<TR>(this Validation<ValidationError, TR> validation) =>
        validation.ToEither().MapLeft(errors => (DomainError)ValidationErrors.Create(errors));


    public static Either<DomainError, ImmutableList<T>> Flatten<T>(this Either<DomainError, T>[] results)
    {
        var oks = results.Rights().ToImmutableList();
        if (oks.Count == results.Length)
            return results.Rights().ToImmutableList();

        var errorType = oks.Any() switch
        {
            true => AggregatedError.FailureType.Partial,
            false => AggregatedError.FailureType.Total
        };
        
        return new AggregatedError(results.Lefts().ToImmutableList(), errorType);
    }

    public static async Task<Either<DomainError, ImmutableList<T>>> Flatten<T>(
        this Task<Either<DomainError, T>[]> results) => (await results).Flatten();
}