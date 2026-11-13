namespace AnimeFeedManager.Shared.Results.Static;

public static class LogFactories
{
    public static Func<T, Action<ILogger>> Log<T>(Action<T, ILogger> logAction) => t => logger => logAction(t, logger);
}