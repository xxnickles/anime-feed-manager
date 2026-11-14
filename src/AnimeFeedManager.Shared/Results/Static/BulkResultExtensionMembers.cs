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
}
