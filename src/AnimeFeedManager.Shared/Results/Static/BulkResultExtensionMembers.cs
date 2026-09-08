namespace AnimeFeedManager.Shared.Results.Static;

public static class BulkResultExtensionMembers
{
    
    extension<T>(BulkResult<T> bulkResult)
    {
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

        /// <summary>
        /// Writes the errors a partial success carries, without touching the value. A partial
        /// success arrives on the success branch, so chain this (<c>AddLogOnSuccess(b => b.LogErrors)</c>)
        /// wherever the summary is logged separately — otherwise those errors go unreported.
        /// </summary>
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