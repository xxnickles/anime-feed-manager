namespace AnimeFeedManager.Core.Error
{
    public class BasicError : DomainError
    {
        public BasicError(string correlationId, string error) : base(correlationId, error)
        {
        }

        public static BasicError Create(string correlationId, string error) => 
            new BasicError(correlationId, error);
    }
}
