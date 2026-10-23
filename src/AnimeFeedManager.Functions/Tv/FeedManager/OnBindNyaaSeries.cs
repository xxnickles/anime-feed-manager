using AnimeFeedManager.Features.Tv.FeedManager;
using AnimeFeedManager.Features.Tv.FeedManager.Binding;
using AnimeFeedManager.Features.Tv.FeedManager.Events;

namespace AnimeFeedManager.Functions.Tv.FeedManager;

public class OnBindNyaaSeries
{
    private static readonly ActivitySource Source = new(Telemetry.TvFeedManagerSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnBindNyaaSeries> _logger;

    public OnBindNyaaSeries(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnBindNyaaSeries> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnBindNyaaSeries))]
    public async Task Run(
        [QueueTrigger(BindNyaaSeries.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        BindNyaaSeries message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(FeedManagerTelemetry.BindSeriesSpan);
        await BindSeries.Bind(message, _tableClientFactory, _domainPostman.SendMessages, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(bound => logger => logger.LogInformation(
                "{series.title} bound to nyaa [{nyaa.group}] {nyaa.release_title}", bound.Title, message.Group, message.ReleaseTitle))
            .Complete(_logger);
    }
}
