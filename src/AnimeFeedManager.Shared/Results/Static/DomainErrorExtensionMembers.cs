namespace AnimeFeedManager.Shared.Results.Static;

public static class DomainErrorExtensionMembers
{
    extension(DomainError error)
    {
        /// <summary>Logs immediately; inside a result chain prefer <c>FlushLogs</c>.</summary>
        public void WriteError(ILogger logger) => error.LogAction().Invoke(logger);
    }
}
