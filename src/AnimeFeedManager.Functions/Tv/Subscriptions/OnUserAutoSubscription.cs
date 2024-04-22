using AnimeFeedManager.Common.Utils;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public sealed class OnUserAutoSubscription(
    InterestedToSubscribe interestedToSubscribe,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnUserAutoSubscription> _logger = loggerFactory.CreateLogger<OnUserAutoSubscription>();

    [Function("OnUserAutoSubscription")]
    public async Task Run(
        [QueueTrigger(UserAutoSubscription.TargetQueue, Connection = Constants.AzureConnectionName)]
        UserAutoSubscription notification)
    {
        var result = await UserId.Validate(notification.UserId)
            .ValidationToEither()
            .BindAsync(user => interestedToSubscribe.ProcessInterested(user, default));

        result.Match(
            count => _logger.LogInformation("{Count} automated subscriptions will be processed for {User}", count,
                notification.UserId),
            error => error.LogError(_logger));
    }
}