using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Tv.Subscriptions;

public sealed class OnProcessAutoSubscription(
    AutomatedSubscriptionProcessor subscriptionProcessor,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnProcessAutoSubscription> _logger = loggerFactory.CreateLogger<OnProcessAutoSubscription>();

    [Function(nameof(OnProcessAutoSubscription))]
    public async Task Run(
        [QueueTrigger(InterestedToSubscription.TargetQueue, Connection = Constants.AzureConnectionName)]
        StateWrap<InterestedToSubscription> notification, CancellationToken token)
    {

        var result = await subscriptionProcessor.Process(notification, token);

        result.Match(
            _ => _logger.LogInformation("Processing {Title} Automated subscription for {User}",
                notification.Payload.InterestedTitle,
                notification.Payload.UserId),
            error => error.LogError(_logger));
    }
}