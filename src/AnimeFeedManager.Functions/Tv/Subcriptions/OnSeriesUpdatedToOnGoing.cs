using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;

namespace AnimeFeedManager.Functions.Tv.Subcriptions;

public class OnSeriesUpdatedToOnGoing
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnSeriesUpdatedToOnGoing> _logger;

    public OnSeriesUpdatedToOnGoing(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnSeriesUpdatedToOnGoing> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnSeriesUpdatedToOnGoing))]
    public async Task Run(
        [QueueTrigger(UpdatedToOngoing.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        UpdatedToOngoing message, CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnSeriesUpdatedToOnGoing));
        await AutoSubscription.TryToSubscribe(message.Series, message.Feed, _tableClientFactory, _domainPostman, token)
            .Match(
                summary => _logger.LogInformation("{Count} Automatic Subscriptions for {Series} has been created",
                    summary.Changes, message.Series),
                e => e.LogError(_logger));
    }
}