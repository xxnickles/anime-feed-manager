namespace AnimeFeedManager.Features.Common.Domain.Errors;

public sealed class UnauthorizedError: DomainError
{
    private UnauthorizedError(string message) : base(message)
    {
    }

    public static UnauthorizedError Create(string endpoint) => new($"Anonymous User are not allowed for {endpoint}");
}