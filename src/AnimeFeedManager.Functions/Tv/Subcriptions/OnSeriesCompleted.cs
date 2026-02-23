using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;
using AnimeFeedManager.Features.Tv.Subscriptions.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Subcriptions;

public class OnSeriesCompleted
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly ILogger<OnSeriesCompleted> _logger;

    public OnSeriesCompleted(
        ITableClientFactory tableClientFactory,
        ILogger<OnSeriesCompleted> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

    [Function(nameof(OnSeriesCompleted))]
    public async Task Run(
        [QueueTrigger(CompletedSeries.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)] CompletedSeries message,
        CancellationToken token)
    {
        await Subscription.ExpireSubscriptions(
                message.Id,
                _tableClientFactory.TableStorageTvSubscriptionsBySeries,
                _tableClientFactory.TableStorageTvSubscriptionUpdater,
                token)
            .AddLogOnSuccess(r => logger => logger.LogInformation("{Count} subscriptions has been expired for the {Series}", r.Changes, message.Id))
            .Complete(_logger);
    }
}