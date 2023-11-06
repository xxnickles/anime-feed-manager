namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class BasicError(string message) : DomainError(message)
{
    public static BasicError Create(string message) => new(message);
}