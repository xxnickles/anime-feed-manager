namespace AnimeFeedManager.Features.Domain.Errors
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

        public override string ToString()
        {
            return $"[{CorrelationId}] - {Message}";
        }

    }
}