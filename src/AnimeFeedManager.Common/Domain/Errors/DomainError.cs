namespace AnimeFeedManager.Common.Domain.Errors
{
    public abstract class DomainError
    {
        public string Message { get; }

        protected DomainError(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }

    }
}