using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Common.Domain.Errors;

public abstract class DomainError(string message)
{
    public string Message { get; } = message;

    public override string ToString()
    {
        return Message;
    }
    
    public abstract void LogError(ILogger logger);

}