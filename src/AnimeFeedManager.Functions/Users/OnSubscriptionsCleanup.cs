using System.Collections.Immutable;
using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Users;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Users;

public sealed class OnSubscriptionsCleanup
{
    private readonly CleanAllSubscriptions _subscriptionsCleaner;
    private readonly ILogger<OnSubscriptionsCleanup> _logger;

    public OnSubscriptionsCleanup(ILoggerFactory loggerFactory, CleanAllSubscriptions subscriptionsCleaner)
    {
        _subscriptionsCleaner = subscriptionsCleaner;
        _logger = loggerFactory.CreateLogger<OnSubscriptionsCleanup>();
    }


    [Function("OnSubscriptionsCleanup")]
    public async Task Run(
        [QueueTrigger(Box.Available.SubscriptionsRemovalBox, Connection = "AzureWebJobsStorage")]
        RemoveSubscriptionsRequest notification)
    {
        var result = await UserId.Validate(notification.UserId)
            .ValidationToEither()
            .BindAsync(userId => _subscriptionsCleaner.CleanAll(userId, default));

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