namespace AnimeFeedManager.Features.Common.Error;

public sealed class ForbiddenError: DomainError
{
    private ForbiddenError(string correlationId, string message) : base(correlationId, message)
    {
    }

    public static ForbiddenError Create(string endpoint) => new("api-forbidden", $"Anonymous User are not allowed for {endpoint}");
}