using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.FeedTitlesUpdateProcess;
using AnimeFeedManager.Features.Tv.Library.Storage.Stores;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnFeedTitlesUpdate
{
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
        FeedTitlesUpdated message, CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnFeedTitlesUpdate));

        if(message.FeedTitles is null or [])
            _logger.LogError("No titles where sent with the update for season {Year}-{Season}", message.Season.Year, message.Season.Season);
         
        await FeedTitlesUpdate.StoreTitles(new FeedTitleUpdateData(message.Season, message.FeedTitles ?? []),
                _tableClientFactory.TableStorageFeedTitlesUpdater, token)
            .SentEvents(_domainPostman.SendMessages, message.Season, token)
            .AddLogOnSuccess(_ => logger => logger.LogInformation("Feed titles have been updated"))
            .WriteLogs(_logger)
            .Done();
    }
}