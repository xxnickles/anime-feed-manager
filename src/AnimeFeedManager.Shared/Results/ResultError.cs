using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

public static class ResultError
{
    public static Result<T> HandledErrorResult<T>() => new HandledError();
    
    public static Result<T> NotFoundResult<T>(string message) => NotFoundError.Create(message);
        
}