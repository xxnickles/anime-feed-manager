using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

public static class ResultCollectionExtensionMembers
{
    extension<T, TError>(IEnumerable<Result<T, TError>> results)
    {
        /// <summary>
        /// <see cref="BulkResult{T, TError}.Completed"/> when all succeed, <see cref="BulkResult{T, TError}.Partial"/> when some fail,
        /// <see cref="BulkError{TError}.AllFailed"/> when all fail; per-item trace contexts are merged into the outcome.
        /// </summary>
        public Result<BulkResult<TTarget, TError>, BulkError<TError>.AllFailed> Collect<TTarget>(
            Func<IEnumerable<T>, TTarget> flatten)
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

            return Outcome(successes, errors, flatten, traceContext);
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

    extension<T, TError>(IEnumerable<Result<BulkResult<T, TError>, BulkError<TError>.AllFailed>> results)
    {
        /// <summary>Merges collected bulks into one: values combined, every item's errors concatenated; same outcomes as <c>Collect</c>.</summary>
        public Result<BulkResult<TTarget, TError>, BulkError<TError>.AllFailed> Merge<TTarget>(
            Func<IEnumerable<T>, TTarget> combine)
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
                    errors.AddRange(result.ErrorValue!.Errors);
            }

            return Outcome(values, errors, combine, traceContext);
        }
    }

    private static Result<BulkResult<TTarget, TError>, BulkError<TError>.AllFailed> Outcome<T, TTarget, TError>(
        List<T> values,
        List<TError> errors,
        Func<IEnumerable<T>, TTarget> flatten,
        TraceContext traceContext)
    {
        Result<BulkResult<TTarget, TError>, BulkError<TError>.AllFailed> outcome = (values.Count, errors.Count) switch
        {
            (_, 0) => new BulkResult<TTarget, TError>.Completed(flatten(values)),
            (0, _) => new BulkError<TError>.AllFailed([..errors]),
            _ => new BulkResult<TTarget, TError>.Partial(flatten(values), [..errors])
        };

        return outcome with {TraceContext = traceContext};
    }
}
