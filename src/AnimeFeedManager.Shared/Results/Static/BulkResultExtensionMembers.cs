using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

public static class BulkResultExtensionMembers
{
    extension<T, TError>(BulkResult<T, TError> bulk)
    {
        /// <summary>The items that failed; empty when all succeeded.</summary>
        public ImmutableArray<TError> Errors => bulk switch
        {
            BulkResult<T, TError>.Completed => [],
            BulkResult<T, TError>.Partial partial => partial.Errors
        };

        /// <summary>Writes the failed items' errors; chain as <c>AddLogOnSuccess(b => b.LogErrors)</c>.</summary>
        public void LogErrors(ILogger logger)
        {
            foreach (var error in bulk.Errors)
                ErrorCases.AsDomainError(error).WriteError(logger);
        }
    }

    extension<T, TError>(Result<BulkResult<T, TError>, BulkError<TError>.AllFailed> collected)
    {
        /// <summary>Fails a partial success with <see cref="BulkError{TError}.Incomplete"/>; for callers that can't act on a subset.</summary>
        public Result<T, BulkError<TError>> RequireAll(string message) =>
            collected
                .MapError(BulkError<TError> (error) => error)
                .Bind(bulk => bulk switch
                {
                    BulkResult<T, TError>.Completed completed => Result<T, BulkError<TError>>.Success(completed.Value),
                    BulkResult<T, TError>.Partial partial => new BulkError<TError>.Incomplete(message, partial.Errors)
                });
    }
}
