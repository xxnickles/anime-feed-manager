using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Common.Domain.Errors;

public static class ErrorLogger
{
    
    public static async Task<Either<DomainError, T>> LogErrors<T>(this Task<Either<DomainError, T>> result, ILogger logger)
    {
        var unwrapped = await result;
        return unwrapped.MapLeft(error =>
        {
            error.LogError(logger);
            return error;
        });
    }
    
    public static Either<DomainError, T> LogErrors<T>(this Either<DomainError, T> result, ILogger logger)
    {
       return result.MapLeft(error =>
        {
            error.LogError(logger);
            return error;
        });
    }

}