using System.Runtime.CompilerServices;

namespace AnimeFeedManager.Features.Common.Domain.Errors;

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
}