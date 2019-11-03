namespace AnimeFeedManager.Core.Error
{
    public abstract class DomainError
    {
        public string CorrelationId { get; }
        public string Message { get; }

        protected DomainError(string correlationId, string message)
        {
            CorrelationId = correlationId;
            Message = message;
        }

    }
}
