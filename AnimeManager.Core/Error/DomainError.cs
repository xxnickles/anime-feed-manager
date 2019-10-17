namespace AnimeFeedManager.Core.Error
{
    public abstract class DomainError
    {
        public string CorrelationId { get; }
        public string Error { get; }

        protected DomainError(string correlationId, string error)
        {
            CorrelationId = correlationId;
            Error = error;
        }

    }
}
