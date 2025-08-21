using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.Storage;
using AnimeFeedManager.Features.Tv.Library.TitlesScrapProcess;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnLatestFeedTitlesUpdate
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly ISeasonFeedTitlesProvider _seasonFeedTitlesProvider;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnLatestFeedTitlesUpdate> _logger;

    public OnLatestFeedTitlesUpdate(
        ITableClientFactory tableClientFactory,
        ISeasonFeedTitlesProvider seasonFeedTitlesProvider,
        IDomainPostman domainPostman,
        ILogger<OnLatestFeedTitlesUpdate> logger)
    {
        _tableClientFactory = tableClientFactory;
        _seasonFeedTitlesProvider = seasonFeedTitlesProvider;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnLatestFeedTitlesUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateLatestFeedTitlesEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        UpdateLatestFeedTitlesEvent message, CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnLatestFeedTitlesUpdate));
        await FeedTitlesScrap.StartFeedUpdateProcess(_tableClientFactory.LatestSeasonGetter(), token)
            .GetFeedTitles(_seasonFeedTitlesProvider)
            .UpdateSeries(_tableClientFactory.GetRawExistentStoredSeriesGetter(),
                _tableClientFactory.GetTvLibraryUpdater(), token)
            .SendEvents(_domainPostman, token)
            .Match(
                results => _logger.LogInformation(
                    "Season {Year}-{Season} tv series titles has been updated. {UpdatedSeries} has been updated",
                    results.Season.Year, results.Season.Season, results.UpdatedSeries),
                e => e.LogError(_logger)
            );;
    }
}