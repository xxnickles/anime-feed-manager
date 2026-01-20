using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

/// <summary>
/// Provides extension members for collections of <see cref="Result{T}"/>,
/// enabling batch operations like flattening multiple results into a single result.
/// </summary>
public static class ResultCollectionExtensionMembers
{
    // ──────────────────────────────────────────────────────────────────
    // Synchronous Collections - IEnumerable<Result<T>>
    // ──────────────────────────────────────────────────────────────────

    extension<T>(IEnumerable<Result<T>> results)
    {
        /// <summary>
        /// Flattens a collection of <see cref="Result{T}"/> into a single result containing all success values.
        /// Returns success only if all results are successful, otherwise returns an <see cref="AggregatedError"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing an <see cref="ImmutableList{T}"/> of all success values,
        /// or an <see cref="AggregatedError"/> with all failures.
        /// </returns>
        public Result<ImmutableList<T>> Flatten()
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

        /// <summary>
        /// Extracts only the successful values from the collection, discarding all failures.
        /// Useful when partial success is acceptable and you want to proceed with available data.
        /// </summary>
        /// <returns>An <see cref="ImmutableList{T}"/> containing only the successful values.</returns>
        public ImmutableList<T> GetSuccessValues()
        {
            return results.Where(r => r.IsSuccess).Select(r => r.MatchToValue(
                v => v,
                _ => throw new InvalidOperationException("List contains errors which should not be possible")
            )).ToImmutableList();
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // Async Collections - IEnumerable<Task<Result<T>>>
    // ──────────────────────────────────────────────────────────────────

    extension<T>(IEnumerable<Task<Result<T>>> results)
    {
        /// <summary>
        /// Awaits all tasks and flattens the resulting collection into a single result.
        /// Uses <see cref="Task.WhenAll"/> for efficient parallel execution.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with an <see cref="ImmutableList{T}"/>
        /// of all success values, or an <see cref="AggregatedError"/> with all failures.
        /// </returns>
        public async Task<Result<ImmutableList<T>>> Flatten()
            => (await Task.WhenAll(results.ToArray())).Flatten();

        /// <summary>
        /// Awaits tasks in batches and flattens the resulting collection into a single result.
        /// Uses <see cref="Enumerable.Chunk{TSource}"/> to split tasks into batches,
        /// processing each batch with <see cref="Task.WhenAll"/> before moving to the next.
        /// This limits concurrent execution, useful for rate-limited APIs or resource-constrained operations.
        /// </summary>
        /// <param name="batchSize">The maximum number of tasks to execute concurrently in each batch.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with an <see cref="ImmutableList{T}"/>
        /// of all success values, or an <see cref="AggregatedError"/> with all failures.
        /// </returns>
        public async Task<Result<ImmutableList<T>>> FlattenBatched(int batchSize)
        {
            var all = new List<Result<T>>();
            foreach (var batch in results.Chunk(batchSize))
            {
                var batchResults = await Task.WhenAll(batch);
                all.AddRange(batchResults);
            }

            return all.Flatten();
        }  
    }

    // ──────────────────────────────────────────────────────────────────
    // Async Enumerable - Task<IEnumerable<Result<T>>>
    // ──────────────────────────────────────────────────────────────────

    extension<T>(Task<IEnumerable<Result<T>>> results)
    {
        /// <summary>
        /// Awaits the enumerable task and flattens the resulting collection into a single result.
        /// </summary>
        /// <returns>
        /// A <see cref="Task{TResult}"/> containing a <see cref="Result{T}"/> with an <see cref="ImmutableList{T}"/>
        /// of all success values, or an <see cref="AggregatedError"/> with all failures.
        /// </returns>
        public async Task<Result<ImmutableList<T>>> Flatten()
            => (await results).Flatten();
    }
}
