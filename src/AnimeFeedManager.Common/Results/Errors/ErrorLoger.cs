using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Results.Errors;

public static class ErrorLogger
{
    
    public static async Task<Result<T>> LogErrors<T>(this Task<Result<T>> result, ILogger logger)
    {
        var unwrapped = await result;
        return unwrapped.MapError(error =>
        {
            error.LogError(logger);
            return error;
        });
    }
    
    public static Result<T> LogErrors<T>(this Result<T> result, ILogger logger)
    {
       return result.MapError(error =>
        {
            error.LogError(logger);
            return error;
        });
    }

}
