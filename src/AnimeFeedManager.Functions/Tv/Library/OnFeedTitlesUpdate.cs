using IdHelpers = AnimeFeedManager.Features.Common.IdHelpers;
using AnimeFeedManager.Features.Tv.Library;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.FeedTitlesUpdateProcess;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnFeedTitlesUpdate
{
    private static readonly ActivitySource Source = new(Telemetry.TvLibraryFeedTitlesSource);

    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnFeedTitlesUpdate> _logger;

    public OnFeedTitlesUpdate(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnFeedTitlesUpdate> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnFeedTitlesUpdate))]
    public async Task Run(
        [QueueTrigger(FeedTitlesUpdated.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        FeedTitlesUpdated message, FunctionContext functionContext, CancellationToken token)
    {
        using var processActivity = message.StartProcessActivity(functionContext);
        using var activity = Source.StartActivity(TvLibraryTelemetry.UpdateFeedTitlesSpan);

        if(message.FeedTitles is null or [])
            _logger.LogError("No titles where sent with the update for season {series.season}", IdHelpers.GenerateAnimePartitionKey(message.Season));

        await FeedTitlesUpdate.StoreTitles(new FeedTitleUpdateData(message.Season, message.FeedTitles ?? []),
                _tableClientFactory.TableStorageFeedTitlesUpdater, token)
            .SentEvents(_domainPostman.SendMessages, message.Season, token)
            .MarkActivityErroredOnError()
            .AddLogOnSuccess(_ => logger => logger.LogInformation("Feed titles have been updated"))
            .Complete(_logger);
    }
}