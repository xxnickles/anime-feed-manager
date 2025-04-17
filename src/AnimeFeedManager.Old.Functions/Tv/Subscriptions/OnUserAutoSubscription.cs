using AnimeFeedManager.Old.Common.Types;
using AnimeFeedManager.Old.Common.Utils;
using AnimeFeedManager.Old.Features.Tv.Subscriptions;
using AnimeFeedManager.Old.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Tv.Subscriptions;

public sealed class OnUserAutoSubscription(
    InterestedToSubscribe interestedToSubscribe,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnUserAutoSubscription> _logger = loggerFactory.CreateLogger<OnUserAutoSubscription>();

    [Function(nameof(OnUserAutoSubscription))]
    public async Task Run(
        [QueueTrigger(UserAutoSubscription.TargetQueue, Connection = Constants.AzureConnectionName)]
        UserAutoSubscription notification, CancellationToken token)
    {
        var result = await UserId.Validate(notification.UserId)
            .ValidationToEither()
            .BindAsync(user => interestedToSubscribe.ProcessInterested(user, token));

        result.Match(
            count => _logger.LogInformation("{Count} automated subscriptions will be processed for {User}", count,
                notification.UserId),
            error => error.LogError(_logger));
    }
}