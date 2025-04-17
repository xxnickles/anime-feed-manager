using System.Collections.Immutable;
using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Users;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Users;

public sealed class OnSubscriptionsCleanup
{
    private readonly CleanAllSubscriptions _subscriptionsCleaner;
    private readonly ILogger<OnSubscriptionsCleanup> _logger;

    public OnSubscriptionsCleanup(ILoggerFactory loggerFactory, CleanAllSubscriptions subscriptionsCleaner)
    {
        _subscriptionsCleaner = subscriptionsCleaner;
        _logger = loggerFactory.CreateLogger<OnSubscriptionsCleanup>();
    }


    [Function(nameof(OnSubscriptionsCleanup))]
    public async Task Run(
        [QueueTrigger(RemoveSubscriptionsRequest.TargetQueue, Connection = Constants.AzureConnectionName)]
        RemoveSubscriptionsRequest notification, CancellationToken token)
    {
        var result = await UserId.Validate(notification.UserId)
            .ValidationToEither()
            .BindAsync(userId => _subscriptionsCleaner.CleanAll(userId, token));

        result.Match(LogResults,
            error => error.LogError(_logger));
    }

    private void LogResults(ImmutableList<ProcessResult> results)
    {
        foreach (var result in results)
        {
            _logger.LogInformation("[{Type}]: {Count} entries have been removed", result.Scope.ToString(),
                result.Completed.ToString()
            );
        }
    }
}