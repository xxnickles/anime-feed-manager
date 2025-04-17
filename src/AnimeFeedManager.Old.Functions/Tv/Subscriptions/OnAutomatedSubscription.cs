using AnimeFeedManager.Old.Common.Domain.Events;
using AnimeFeedManager.Old.Features.Tv.Subscriptions;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Tv.Subscriptions;

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

    [Function(nameof(OnAutomatedSubscription))]
    public async Task Run(
        [QueueTrigger(AutomatedSubscription.TargetQueue, Connection = Constants.AzureConnectionName)]
        AutomatedSubscription notification, CancellationToken token)
    {
        _logger.LogInformation("Starting automated subscription");
        await _automatedSubscriptionHandler.Handle(notification, token);
    }
}