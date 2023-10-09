namespace AnimeFeedManager.Common.Domain.Errors
{
    public sealed class NoContentError : DomainError
    {
        private NoContentError(string message) : base(message)
        {
        }

        public static NoContentError Create(string message) => new(message);
    }
}