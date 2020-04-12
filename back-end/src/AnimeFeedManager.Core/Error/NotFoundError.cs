namespace AnimeFeedManager.Core.Error
{
    public class NotFoundError : DomainError
    {
        public NotFoundError(string correlationId, string message) : base(correlationId, message)
        {
        }

        public static NotFoundError Create(string correlationId, string message) => new NotFoundError(correlationId, message);
    }
}