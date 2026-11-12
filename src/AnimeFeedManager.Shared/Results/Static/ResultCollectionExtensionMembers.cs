using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

public static class ResultCollectionExtensionMembers
{
    extension<T>(IEnumerable<Result<T, DomainError>> results)
    {
        /// <summary>
        /// <see cref="CompletedBulkResult{T}"/> when all succeed, <see cref="PartialSuccessBulkResult{T}"/> when some fail,
        /// <see cref="AggregatedError"/> when all fail; per-item trace contexts are merged into the outcome.
        /// </summary>
        public Result<BulkResult<TTarget>, DomainError> Flatten<TTarget>(Func<IEnumerable<T>, TTarget> flattenFunc)
        {
            var successes = new List<T>();
            var errors = new List<DomainError>();
            var traceContext = TraceContext.Empty;

            foreach (var result in results)
            {
                traceContext = traceContext.Merge(result.TraceContext);
                if (result.IsSuccess)
                    successes.Add(result.ResultValue!);
                else
                    errors.Add(result.ErrorValue!);
            }

            Result<BulkResult<TTarget>, DomainError> aggregated = (successes.Count, errors.Count) switch
            {
                (_, 0) => new CompletedBulkResult<TTarget>(flattenFunc(successes)),
                (0, _) => new AggregatedError("All the operations in a bulk operation failed", [..errors]),
                _ => new PartialSuccessBulkResult<TTarget>(flattenFunc(successes), [..errors])
            };

            return aggregated with {TraceContext = traceContext};
        }
    }

    extension<T>(Task<IEnumerable<Result<T, DomainError>>> results)
    {
        public async Task<Result<BulkResult<TTarget>, DomainError>> Flatten<TTarget>(Func<IEnumerable<T>, TTarget> flattenFunc)
        {
            return (await results).Flatten(flattenFunc);
        }
    }
}