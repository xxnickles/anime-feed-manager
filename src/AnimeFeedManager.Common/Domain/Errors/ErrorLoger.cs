using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public static class ErrorLogger
{
    
    public static async Task<Either<DomainError, T>> LogErrors<T>(this Task<Either<DomainError, T>> result, ILogger logger)
    {
        var unwrapped = await result;
        return unwrapped.MapLeft(error =>
        {
            error.LogDomainError(logger);
            return error;
        });
    }
    
    public static Either<DomainError, T> LogErrors<T>(this Either<DomainError, T> result, ILogger logger)
    {
       return result.MapLeft(error =>
        {
            error.LogDomainError(logger);
            return error;
        });
    }
    
    public static void LogDomainError(this DomainError error, ILogger logger)
    {
        _ = error switch
        {
            ExceptionError eError => eError.LogException(logger),
            NoContentError noContentError => noContentError.LogNoContent(logger),
            ValidationErrors validationErrors => validationErrors.LogValidationErrors(logger),
            AggregatedError aError => aError.LogAggregatedError(logger),
            NotingToProcessError notingToProcessError => notingToProcessError.LogNotingToProcess(logger),
            _ => error.LogError(logger)
        };
    }

    private static Unit LogValidationErrors(this ValidationErrors validationErrors, ILogger logger)
    {
        logger.LogWarning("{Error}", validationErrors.Message);
        foreach (var validationError in validationErrors.Errors)
            logger.LogWarning("Field: {Field} Messages: {Messages}", validationError.Key,
                string.Join(". ", validationError.Value));

        return unit;
    }

    private static Unit LogAggregatedError(this AggregatedError aggregatedError, ILogger logger)
    {
        logger.LogWarning("{Message}. {TypeMessage}", aggregatedError.Message, TypeMessage(aggregatedError.Type));
        foreach (var error in aggregatedError.Errors)
            error.LogError(logger);
        return unit;

        string TypeMessage(AggregatedError.FailureType type) => type switch
        {
            AggregatedError.FailureType.Partial => "Some of operations were completed Successfully.",
            AggregatedError.FailureType.Total => "All operations failed.",
            _ => throw new UnreachableException()
        };
    }

    private static Unit LogNoContent(this NoContentError nError, ILogger logger)
    {
        logger.LogWarning("{Error}", nError.Message);
        return unit;
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

    private static Unit LogNotingToProcess(this NotingToProcessError nError, ILogger logger)
    {
        logger.LogWarning("{Error}", nError.Message);
        return unit;
    }
}