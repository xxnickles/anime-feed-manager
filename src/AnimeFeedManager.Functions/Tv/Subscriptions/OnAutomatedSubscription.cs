using AnimeFeedManager.Common.Domain.Events;
using AnimeFeedManager.Features.Infrastructure.Messaging;
using AnimeFeedManager.Features.Tv.Subscriptions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Tv.Subscriptions;

public sealed class OnAutomatedSubscription
{
    private readonly AutomatedSubscriptionHandler _automatedSubscriptionHandler;

    public OnAutomatedSubscription(AutomatedSubscriptionHandler automatedSubscriptionHandler,
        ILoggerFactory loggerFactory)
    {
        _automatedSubscriptionHandler = automatedSubscriptionHandler;
        _logger = loggerFactory.CreateLogger<OnAutomatedSubscription>();
    }

    private readonly ILogger<OnAutomatedSubscription> _logger;

    [Function("OnAutomatedSubscription")]
    public async Task Run(
        [QueueTrigger(Box.Available.AutomatedSubscriptionBox, Connection = "AzureWebJobsStorage")] AutomatedSubscription notification)
    {
        _logger.LogInformation("Starting automated subscription");
        await _automatedSubscriptionHandler.Handle(notification, default);
    }
}