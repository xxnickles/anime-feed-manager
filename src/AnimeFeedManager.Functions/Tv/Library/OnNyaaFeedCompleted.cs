using AnimeFeedManager.Features.Tv.FeedManager.Events;
using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Tv.Library.SeriesCompletion;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnNyaaFeedCompleted
{
    private static readonly ActivitySource Source = new(Telemetry.TvLibraryCompletionSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly ILogger<OnNyaaFeedCompleted> _logger;

    public OnNyaaFeedCompleted(
        ITableClientFactory tableClientFactory,
        ILogger<OnNyaaFeedCompleted> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

    [Function(nameof(OnNyaaFeedCompleted))]
    public async Task Run(
        [QueueTrigger(NyaaFeedCompleted.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        NyaaFeedCompleted message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvLibraryTelemetry.CompleteFeedSeriesSpan);
        await CompleteFeedSeries.Complete(
                message.SeriesId,
                _tableClientFactory.TableStorageTvSeriesGetter,
                _tableClientFactory.TableStorageTvSeriesUpdater,
                token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(_ => logger => logger.LogInformation("TV series {series.id} completed by its nyaa feed", message.SeriesId))
            .Complete(_logger);
    }
}
