using AnimeFeedManager.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Notifications;

public class CompleteSubscription
{
    private readonly IOvasSubscriptionStore _ovasSubscriptionStore;
    private readonly ILogger<CompleteSubscription> _logger;

    public CompleteSubscription(
        IOvasSubscriptionStore ovasSubscriptionStore,
        ILogger<CompleteSubscription> logger)
    {
        _ovasSubscriptionStore = ovasSubscriptionStore;
        _logger = logger;
    }

    [Function(nameof(CompleteSubscription))]
    public async Task Run(
        [QueueTrigger(CompleteOvaSubscription.TargetQueue, Connection = Constants.AzureConnectionName)]
        CompleteOvaSubscription message, CancellationToken token)
    {
        var entity = message.OvaInformation;
        entity.Processed = true;
        var result = await _ovasSubscriptionStore.Upsert(entity,token);
        result.Match(
            _ => _logger.LogInformation("Notification for Ova subscription {Ova} for {Subscriber} has been completed", entity.RowKey, entity.PartitionKey),
            error => error.LogError(_logger)
        );
    }
}