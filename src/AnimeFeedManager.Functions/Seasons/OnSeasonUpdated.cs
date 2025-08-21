using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Functions.Seasons;

public class OnSeasonUpdated
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnSeasonUpdated> _logger;

    public OnSeasonUpdated(
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnSeasonUpdated> logger)
    {
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnSeasonUpdated))]
    public async Task Run(
        [QueueTrigger(SeasonUpdated.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        SeasonUpdated message,
        CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnSeasonUpdated));
        await SeasonUpdate.CheckSeasonExist(_tableClientFactory.SeasonGetter(), message.Season, token)
            .CreateNewSeason()
            .AddLatestSeasonData(_tableClientFactory.LatestSeasonGetter(), token)
            .StoreUpdatedSeason(_tableClientFactory.SeasonUpdater(), token)
            .DemoteCurrentLatest(_tableClientFactory.SeasonUpdater(), token)
            .UpdateLast4Seasons(_tableClientFactory.AllSeasonsGetter(), _tableClientFactory.LastestSeasonsUpdater(),
                token)
            .SentEvents(_domainPostman, message.Season, token)
            .Match(r => LogSuccess(r, _logger),
                e => e.LogError(_logger));
    }

    private static void LogSuccess(SeasonUpdateResult data, ILogger logger)
    {
        switch (data.SeasonUpdateStatus, data.Season.IsLatest)
        {
            case (SeasonUpdateStatus.NoChanges, _):
                logger.LogInformation("{Year}-{Season} already exist in the system. No actions have been taken",
                    data.Season.Year, data.Season.Season);
                break;
            case (SeasonUpdateStatus.New, true):
                logger.LogInformation("{Year}-{Season} has been added to the system as latest season",
                    data.Season.Year, data.Season.Season);
                break;
            case  (SeasonUpdateStatus.New, false):
                logger.LogInformation("{Year}-{Season} has been added to the system as new season",
                    data.Season.Year, data.Season.Season);
                break;
            default:
                logger.LogInformation("{Year}-{Season} has been updated",
                    data.Season.Year, data.Season.Season);
                break;
        }
    }
}