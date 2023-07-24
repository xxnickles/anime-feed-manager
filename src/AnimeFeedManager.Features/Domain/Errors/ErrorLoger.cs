using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Features.Domain.Errors;

public static class ErrorLogger
{
    public static void LogDomainError(this DomainError error, ILogger logger)
    {
        _ = error switch
        {
            ExceptionError eError => eError.LogException(logger),
            _ => error.LogError(logger)
        };
    }

    private static Unit LogException(this ExceptionError eError, ILogger logger)
    {
        logger.LogError(eError.Exception, "[{CorrelationId}] An Exception has occurred", eError.CorrelationId);
        return unit;
    }
    
    private static Unit LogError(this DomainError error, ILogger logger)
    {
        logger.LogError("[{CorrelationId}] {Message}", error.CorrelationId, error.Message);
        return unit;
    }
    
    
}