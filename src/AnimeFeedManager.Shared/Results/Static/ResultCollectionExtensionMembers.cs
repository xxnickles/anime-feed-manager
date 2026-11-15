namespace AnimeFeedManager.Shared.Results.Static;

public static class ResultCollectionExtensionMembers
{
    extension<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        /// <summary>
        /// <see cref="BulkResult{T, TError}.Completed"/> when all succeed, <see cref="BulkResult{T, TError}.Partial"/> when some fail;
        /// when all fail, <paramref name="allFailed"/> builds the caller's error. Item trace contexts merge into the outcome.
        /// </summary>
        public Result<BulkResult<TTarget, TError>, TFailure> Collect<TTarget, TFailure>(
            Func<IEnumerable<T>, TTarget> flatten,
            Func<ImmutableArray<TError>, TFailure> allFailed)
        {
            var (successes, errors, traceContext) = Split(results);
            return Outcome(successes, errors, flatten, allFailed, traceContext);
        }

        /// <summary>The flattened values when every item succeeds; otherwise <paramref name="failed"/> builds the caller's error from the items that failed.</summary>
        public Result<TTarget, TFailure> CollectAll<TTarget, TFailure>(
            Func<IEnumerable<T>, TTarget> flatten,
            Func<ImmutableArray<TError>, TFailure> failed)
        {
            var (successes, errors, traceContext) = Split(results);
            return errors.Count == 0
                ? Result<TTarget, TFailure>.Success(flatten(successes), traceContext)
                : Result<TTarget, TFailure>.Failure(failed([..errors]), traceContext);
        }

        /// <summary>Never fails: flattens the successes and stages each failure's log on the merged trace context.</summary>
        public Result<TTarget, TError> CollectSuccesses<TTarget>(Func<IEnumerable<T>, TTarget> flattenFunc)
        {
            var successes = new List<T>();
            var traceContext = TraceContext.Empty;

            foreach (var result in results)
            {
                traceContext = traceContext.Merge(result.TraceContext);
                if (result.IsSuccess)
                    successes.Add(result.ResultValue!);
                else
                    traceContext = traceContext.AddLog(ErrorCases.AsDomainError(result.ErrorValue!).LogAction());
            }

            return Result<TTarget, TError>.Success(flattenFunc(successes), traceContext);
        }
    }

    extension<T, TError>(IEnumerable<Result<BulkResult<T, TError>, TError>> results)
    {
        /// <summary>Merges collected bulks into one: values combined, item errors concatenated, a failed bulk's error kept as one more item error; same outcomes as <c>Collect</c>.</summary>
        public Result<BulkResult<TTarget, TError>, TFailure> Merge<TTarget, TFailure>(
            Func<IEnumerable<T>, TTarget> combine,
            Func<ImmutableArray<TError>, TFailure> allFailed)
        {
            var values = new List<T>();
            var errors = new List<TError>();
            var traceContext = TraceContext.Empty;

            foreach (var result in results)
            {
                traceContext = traceContext.Merge(result.TraceContext);
                if (result.IsSuccess)
                {
                    values.Add(result.ResultValue!.Value);
                    errors.AddRange(result.ResultValue!.Errors);
                }
                else
                    errors.Add(result.ErrorValue!);
            }

            return Outcome(values, errors, combine, allFailed, traceContext);
        }
    }

    private static (List<T> Successes, List<TError> Errors, TraceContext TraceContext) Split<T, TError>(
        IEnumerable<Result<T, TError>> results)
    {
        var successes = new List<T>();
        var errors = new List<TError>();
        var traceContext = TraceContext.Empty;

        foreach (var result in results)
        {
            traceContext = traceContext.Merge(result.TraceContext);
            if (result.IsSuccess)
                successes.Add(result.ResultValue!);
            else
                errors.Add(result.ErrorValue!);
        }

        return (successes, errors, traceContext);
    }

    private static Result<BulkResult<TTarget, TError>, TFailure> Outcome<T, TTarget, TError, TFailure>(
        List<T> values,
        List<TError> errors,
        Func<IEnumerable<T>, TTarget> flatten,
        Func<ImmutableArray<TError>, TFailure> allFailed,
        TraceContext traceContext) =>
        (values.Count, errors.Count) switch
        {
            (_, 0) => Result<BulkResult<TTarget, TError>, TFailure>.Success(
                new BulkResult<TTarget, TError>.Completed(flatten(values)), traceContext),
            (0, _) => Result<BulkResult<TTarget, TError>, TFailure>.Failure(allFailed([..errors]), traceContext),
            _ => Result<BulkResult<TTarget, TError>, TFailure>.Success(
                new BulkResult<TTarget, TError>.Partial(flatten(values), [..errors]), traceContext)
        };
}
