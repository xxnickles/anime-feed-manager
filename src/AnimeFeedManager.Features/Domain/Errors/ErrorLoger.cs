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
        logger.LogError(eError.Exception, "{Error}", eError.Message);
        return unit;
    }
    
    private static Unit LogError(this DomainError error, ILogger logger)
    {
        logger.LogError("{Message}", error.Message);
        return unit;
    }
    
    
}