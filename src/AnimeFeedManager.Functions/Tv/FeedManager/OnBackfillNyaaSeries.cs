using AnimeFeedManager.Features.Scrapping.Nyaa;
using AnimeFeedManager.Features.Tv.FeedManager;
using AnimeFeedManager.Features.Tv.FeedManager.Backfill;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Functions.Tv.FeedManager;

public class OnBackfillNyaaSeries
{
    private static readonly ActivitySource Source = new(Telemetry.TvFeedManagerSource);

    private readonly INyaaClient _nyaaClient;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnBackfillNyaaSeries> _logger;

    public OnBackfillNyaaSeries(
        INyaaClient nyaaClient,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnBackfillNyaaSeries> logger)
    {
        _nyaaClient = nyaaClient;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnBackfillNyaaSeries))]
    public async Task Run(
        [QueueTrigger(BackfillNyaaSeries.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        BackfillNyaaSeries message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(FeedManagerTelemetry.BackfillSeriesSpan);
        await BackfillSeries.Backfill(message, _nyaaClient, _tableClientFactory, _domainPostman.SendMessages, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(backfilled => logger => logger.LogInformation(
                "{series.title} backfilled from nyaa: {nyaa.catalogued_count} releases catalogued",
                backfilled.Title, backfilled.Catalogued))
            .Complete(_logger);
    }
}
