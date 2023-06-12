namespace AnimeFeedManager.Features.Common.Error;

public sealed class BasicError : DomainError
{
    public BasicError(string correlationId, string message) : base(correlationId, message)
    {
    }

    public static BasicError Create(string correlationId, string message) => new(correlationId, message);
}