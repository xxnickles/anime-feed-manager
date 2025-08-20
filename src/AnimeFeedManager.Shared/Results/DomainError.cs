using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Shared.Results;

public abstract record DomainError(
    string Message,
    string CallerMemberName,
    string CallerFilePath,
    int CallerLineNumber)
{
    public string Message { get; } = Message;
    public string CallerMemberName { get; } = CallerMemberName;
    public string CallerFilePath { get; } = CallerFilePath;
    public int CallerLineNumber { get; } = CallerLineNumber;


    public override string ToString()
    {
        return $"{Message} (Called from {CallerMemberName} at {CallerFilePath}:{CallerLineNumber})";
    }

    // Abstract method that implementations must provide, only accessible to derived classes
    /// <summary>
    /// Provides an implementation for logging messages
    /// </summary>
    /// <param name="logger">A logger <see cref="ILogger"/></param>
    protected abstract void LoggingBehavior(ILogger logger);


    // Template method that wraps the implementation-specific logging in a context scope
    public void LogError(ILogger logger)
    {
        // Create a dictionary of custom log properties with caller information
        var logProperties = new Dictionary<string, object>
        {
            ["CallerMemberName"] = CallerMemberName,
            ["CallerFilePath"] = CallerFilePath,
            ["CallerLineNumber"] = CallerLineNumber,
            ["Caller"] = $"{CallerFilePath}:{CallerLineNumber}",
            ["ErrorType"] = GetType().Name
        };

        // Add additional properties specific to this error
        AddLogProperties(logProperties);

        // Create a logging scope with all properties
        using (logger.BeginScope(logProperties))
        {
            // Call the implementation-specific logging method
            LoggingBehavior(logger);
        }
    }

    // Allow derived classes to add their own properties to the logging context
    protected virtual void AddLogProperties(Dictionary<string, object> properties)
    {
        // Default implementation does nothing
    }
}