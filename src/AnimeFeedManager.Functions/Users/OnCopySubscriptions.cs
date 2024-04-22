using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Users;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Users;

public sealed class OnCopySubscriptions
{
    private readonly CopyAllSubscriptions _subscriptionsCopier;
    private readonly ILogger<OnCopySubscriptions> _logger;

    public OnCopySubscriptions(ILoggerFactory loggerFactory, CopyAllSubscriptions subscriptionsCopier)
    {
        _subscriptionsCopier = subscriptionsCopier;
        _logger = loggerFactory.CreateLogger<OnCopySubscriptions>();
    }


    [Function("OnCopySubscriptions")]
    public async Task Run(
        [QueueTrigger(CopySubscriptionRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        CopySubscriptionRequest notification)
    {
        _logger.LogInformation("Trying to copy subscriptions from {Source} to {Target}", notification.SourceId,
            notification.TargetId);
        var result =
            await (UserId.Validate(notification.SourceId), UserId.Validate(notification.TargetId))
                .Apply((source, target) => (source, target))
                .ValidationToEither()
                .BindAsync(users => _subscriptionsCopier.CopyAll(users.source, users.target, default));

        result.Match(LogResults,
            error => error.LogError(_logger));
    }

    private void LogResults(ImmutableList<ProcessResult> results)
    {
        foreach (var result in results)
        {
            _logger.LogInformation("[{Type}]: {Count} entries have been copied", result.Scope.ToString(),
                result.Completed.ToString());
        }
    }
}