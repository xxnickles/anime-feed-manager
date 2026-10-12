using AnimeFeedManager.Features.Tv.Subscriptions;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Subscriptions.Management;

namespace AnimeFeedManager.Functions.Tv.Subcriptions;

public class OnSeriesUpdatedToOnGoing
{
    private static readonly ActivitySource Source = new(Telemetry.TvSubscriptionsManagementSource);

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
        UpdatedToOngoing message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvSubscriptionsTelemetry.AutoSubscribeSpan);
        await AutoSubscription.TryToSubscribe(message.Series, message.Feed, _tableClientFactory, _domainPostman.SendMessages, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(summary => logger => logger.LogInformation("{subscription.count} Automatic Subscriptions for {series.id} has been created", summary.Changes, message.Series))
            .Complete(_logger);
    }
}