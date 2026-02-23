using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Extension members for collections of <see cref="Result{T}"/>,
/// providing bulk aggregation operations.
/// </summary>
public static class ResultCollectionExtensionMembers
{
    extension<T>(IEnumerable<Result<T>> results)
    {
        /// <summary>
        /// Aggregates a collection of individual results into a single <see cref="BulkResult{T}"/> result
        /// using a single pass over the collection.
        /// </summary>
        /// <typeparam name="TTarget">The type produced by aggregating successful values.</typeparam>
        /// <param name="flattenFunc">
        /// Projection applied to successful values to produce the aggregated result
        /// (e.g., summing RU costs into <c>OperationMetadata</c>).
        /// </param>
        /// <returns>
        /// <list type="bullet">
        ///   <item><see cref="CompletedBulkResult{T}"/> — all operations succeeded.</item>
        ///   <item><see cref="PartialSuccessBulkResult{T}"/> — some succeeded, errors collected alongside.</item>
        ///   <item><see cref="AggregatedError"/> — all operations failed.</item>
        /// </list>
        /// </returns>
        public Result<BulkResult<TTarget>> Flatten<TTarget>(Func<IEnumerable<T>, TTarget> flattenFunc)
        {
            var successes = new List<T>();
            var errors = new List<DomainError>();

            foreach (var result in results)
            {
                if (result.IsSuccess)
                    successes.Add(result.ResultValue!);
                else
                    errors.Add(result.ErrorValue!);
            }

            return (successes.Count, errors.Count) switch
            {
                (_, 0) => new CompletedBulkResult<TTarget>(flattenFunc(successes)),
                (0, _) => new AggregatedError("All the operations in a bulk operation failed", [..errors]),
                _ => new PartialSuccessBulkResult<TTarget>(flattenFunc(successes), [..errors])
            };
        }
    }

    extension<T>(Task<IEnumerable<Result<T>>> results)
    {
        /// <summary>
        /// Async variant of <see cref="Flatten{TTarget}"/>. Awaits the task, then delegates
        /// to the synchronous overload.
        /// </summary>
        public async Task<Result<BulkResult<TTarget>>> Flatten<TTarget>(Func<IEnumerable<T>, TTarget> flattenFunc)
        {
            return (await results).Flatten(flattenFunc);
        }
    }
}