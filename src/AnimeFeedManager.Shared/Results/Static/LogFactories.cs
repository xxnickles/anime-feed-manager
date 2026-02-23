namespace AnimeFeedManager.Shared.Results.Static;

public static class LogFactories
{
    public static Func<BulkResult<T>, Action<ILogger>> LogBulkResult<T>(Action<T, ILogger> onCompleted) =>
        bulkResult => logger => bulkResult.LogResults(logger, onCompleted);
    
    public static Func<T, Action<ILogger>> Log<T>(Action<T, ILogger> logAction) => t => logger => logAction(t, logger);
}