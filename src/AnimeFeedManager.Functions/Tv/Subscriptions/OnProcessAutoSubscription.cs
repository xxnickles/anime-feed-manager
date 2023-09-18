using AnimeFeedManager.Features.Common.Domain.Errors;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.State.Types;
using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public sealed class OnProcessAutoSubscription
{
    private readonly AutomatedSubscriptionProcessor _subscriptionProcessor;
    private readonly ILogger<OnProcessAutoSubscription> _logger;

    public OnProcessAutoSubscription(
        AutomatedSubscriptionProcessor subscriptionProcessor,
        ILoggerFactory loggerFactory)
    {
        _subscriptionProcessor = subscriptionProcessor;
        _logger = loggerFactory.CreateLogger<OnProcessAutoSubscription>();
    }

    [Function("OnProcessAutoSubscription")]
    public async Task Run(
        [QueueTrigger(Box.Available.AutoSubscriptionsProcessBox, Connection = "AzureWebJobsStorage")]
        StateWrap<InterestedToSubscription> notification)
    {

        var result = await _subscriptionProcessor.Process(notification, default);

        result.Match(
            count => _logger.LogInformation("Automated subscriptions for {User}",
                notification.Payload.UserId),
            error => error.LogDomainError(_logger));
    }
}