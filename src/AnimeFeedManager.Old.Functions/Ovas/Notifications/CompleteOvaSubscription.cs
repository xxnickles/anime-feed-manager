using AnimeFeedManager.Old.Features.Ovas.Subscriptions.IO;
using AnimeFeedManager.Old.Features.Ovas.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Old.Functions.Ovas.Notifications;

public class CompleteOvaSubscription
{
    private readonly IOvasSubscriptionStore _ovasSubscriptionStore;
    private readonly ILogger<CompleteOvaSubscription> _logger;

    public CompleteOvaSubscription(
        IOvasSubscriptionStore ovasSubscriptionStore,
        ILogger<CompleteOvaSubscription> logger)
    {
        _ovasSubscriptionStore = ovasSubscriptionStore;
        _logger = logger;
    }

    [Function(nameof(CompleteOvaSubscription))]
    public async Task Run(
        [QueueTrigger(CompleteOvaSubscriptionEvent.TargetQueue, Connection = Constants.AzureConnectionName)]
        CompleteOvaSubscriptionEvent message, CancellationToken token)
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