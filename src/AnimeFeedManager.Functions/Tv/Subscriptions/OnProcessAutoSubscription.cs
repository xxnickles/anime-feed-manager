using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public sealed class OnProcessAutoSubscription(
    AutomatedSubscriptionProcessor subscriptionProcessor,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<OnProcessAutoSubscription> _logger = loggerFactory.CreateLogger<OnProcessAutoSubscription>();

    [Function("OnProcessAutoSubscription")]
    public async Task Run(
        [QueueTrigger(Box.Available.AutoSubscriptionsProcessBox, Connection = "AzureWebJobsStorage")]
        StateWrap<InterestedToSubscription> notification)
    {

        var result = await subscriptionProcessor.Process(notification, default);

        result.Match(
            _ => _logger.LogInformation("Processing {Title} Automated subscription for {User}",
                notification.Payload.InterestedTitle,
                notification.Payload.UserId),
            error => error.LogError(_logger));
    }
}