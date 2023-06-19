namespace AnimeFeedManager.Features.Domain.Errors
{
    public sealed class UnauthorizedError: DomainError
    {
        private UnauthorizedError(string correlationId, string message) : base(correlationId, message)
        {
        }

        public static UnauthorizedError Create(string endpoint) => new("api-unauthorized", $"Anonymous User are not allowed for {endpoint}");
    }
}