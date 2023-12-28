using AnimeFeedManager.Common.Domain.Errors;
using Microsoft.Extensions.Logging;
using Passwordless.Net;

namespace AnimeFeedManager.Features.Users.Types;

public class PasswordlessError : DomainError
{
    public PasswordlessApiException Exception { get; set; }
    
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