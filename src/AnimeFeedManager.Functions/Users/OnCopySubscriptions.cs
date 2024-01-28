using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
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
        [QueueTrigger(Box.Available.SubscriptionsCopyBox, Connection = "AzureWebJobsStorage")]
        CopySubscriptionRequest notification)
    {
        _logger.LogInformation("Trying to copy subscriptions from {Source} to {Target}", notification.sourceId,
            notification.targetId);
        var result =
            await (UserIdValidator.Validate(notification.sourceId), UserIdValidator.Validate(notification.targetId))
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