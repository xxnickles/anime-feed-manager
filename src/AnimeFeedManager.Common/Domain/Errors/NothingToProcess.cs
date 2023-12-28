using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Common.Domain.Errors;

public sealed class NotingToProcessError : DomainError
{
    private NotingToProcessError(string message) : base(message)
    {
    }

    public static NotingToProcessError Create(
        string additionalInput, 
        [CallerMemberName] string callerName = "",
        [CallerFilePath] string callerFilePath = "") =>
        new($"Method/function {callerName} ({callerFilePath}) didn't produce anything to process. {additionalInput}");

    public override void LogError(ILogger logger)
    {
        logger.LogWarning("{Error}", Message);
    }
}