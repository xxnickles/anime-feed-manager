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
    }
}