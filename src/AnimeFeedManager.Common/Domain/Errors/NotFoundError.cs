namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class NotFoundError : DomainError
{
    private NotFoundError(string message) : base(message)
    {
    }

    public static NotFoundError Create(string message) => new(message);
}