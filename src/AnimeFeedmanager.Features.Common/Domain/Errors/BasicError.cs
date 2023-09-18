namespace AnimeFeedManager.Features.Common.Domain.Errors;

public sealed class BasicError : DomainError
{
    public BasicError( string message) : base(message)
    {
    }

    public static BasicError Create(string message) => new(message);
}