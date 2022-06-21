namespace AnimeFeedManager.Core.Error;

public class BasicError : DomainError
{
    public BasicError(string correlationId, string message) : base(correlationId, message)
    {
    }

    public static BasicError Create(string correlationId, string message) => 
        new BasicError(correlationId, message);
}