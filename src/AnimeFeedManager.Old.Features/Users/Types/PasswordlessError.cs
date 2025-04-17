using AnimeFeedManager.Old.Common.Domain.Errors;
using Microsoft.Extensions.Logging;
using Passwordless;

namespace AnimeFeedManager.Old.Features.Users.Types;

public class PasswordlessError : DomainError
{
    public PasswordlessApiException Exception { get; }
    
    private PasswordlessError(PasswordlessApiException exception) : base(exception.Message)
    {
        Exception = exception;
    }

    internal static PasswordlessError FromException(PasswordlessApiException exception) => new(exception);

    public override void LogError(ILogger logger)
    {
        logger.LogError(Exception, "{Error}", Message);
    }
}