using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results.Static;

public static class BulkResultExtensionMembers
{
    
    extension<T>(BulkResult<T> bulkResult)
    {
        /// <summary>Fails a partial success with an <see cref="AggregatedError"/> of its errors; for callers that can't act on a subset.</summary>
        public Result<T, DomainError> RequireAll(string message) =>
            bulkResult is PartialSuccessBulkResult<T> partial
                ? new AggregatedError(message, partial.Errors)
                : bulkResult.Value;

        public void LogResults(
            ILogger logger,
            Action<T, ILogger> onCompleted)
        {
            switch (bulkResult)
            {
                case CompletedBulkResult<T> completed:
                    onCompleted(completed.Value, logger);
                    break;
                case PartialSuccessBulkResult<T> partialSuccess:
                    onCompleted(partialSuccess.Value, logger);
                    foreach (var error in partialSuccess.Errors)
                        error.WriteError(logger);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bulkResult), bulkResult, null);
            }
        }

        /// <summary>Writes a partial success's errors only; chain as <c>AddLogOnSuccess(b => b.LogErrors)</c> when the summary is logged separately.</summary>
        public void LogErrors(ILogger logger)
        {
            switch (bulkResult)
            {
                case CompletedBulkResult<T>:
                    break;
                case PartialSuccessBulkResult<T> partialSuccess:
                    foreach (var error in partialSuccess.Errors)
                        error.WriteError(logger);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bulkResult), bulkResult, null);
            }
        }
    }
}