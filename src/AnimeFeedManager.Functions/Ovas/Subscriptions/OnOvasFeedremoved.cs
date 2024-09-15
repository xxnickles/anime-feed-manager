using AnimeFeedManager.Features.Ovas.Subscriptions;
using AnimeFeedManager.Features.Ovas.Subscriptions.Types;
using Microsoft.Extensions.Logging;

namespace AnimeFeedManager.Functions.Ovas.Subscriptions;

public class OnOvasFeedremoved
{
    private readonly OvasSubscriptionStatusResetter _ovasSubscriptionStatusResetter;
    private readonly ILogger<OnOvasFeedremoved> _logger;

    public OnOvasFeedremoved(
        OvasSubscriptionStatusResetter ovasSubscriptionStatusResetter,
        ILogger<OnOvasFeedremoved> logger)
    {
        _ovasSubscriptionStatusResetter = ovasSubscriptionStatusResetter;
        _logger = logger;
    }

    [Function(nameof(OnOvasFeedremoved))]
    public async Task Run(
        [QueueTrigger(OvaFeedRemovedEvent.TargetQueue, Connection = Constants.AzureConnectionName)]
        OvaFeedRemovedEvent notification, CancellationToken token)
    {
        var result = await RowKey.Parse(notification.Tile)
            .BindAsync(title => _ovasSubscriptionStatusResetter.ResetStatus(title, token));

        result.Match(
            _ => { _logger.LogInformation("Subscriptions for Ova {Ova} have been reset", notification.Tile); },
            e => { e.LogError(_logger); }
        );
    }
}