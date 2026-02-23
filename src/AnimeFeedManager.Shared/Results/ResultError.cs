using AnimeFeedManager.Shared.Results.Errors;

namespace AnimeFeedManager.Shared.Results;

public static class ResultError
{
    public static Result<T> HandledErrorResult<T>() => HandledError.Create();
}