using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.Storage.Stores;
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
        await SeasonUpdate.CheckSeasonExist(_tableClientFactory.TableStorageSeason, message.Season, token)
            .CreateNewSeason()
            .AddLatestSeasonData(_tableClientFactory.TableStorageLatestSeason, token)
            .StoreUpdatedSeason(_tableClientFactory.TableStorageSeasonUpdater, token)
            .DemoteCurrentLatest(_tableClientFactory.TableStorageSeasonUpdater, token)
            .UpdateLast4Seasons(_tableClientFactory.TableStorageAllSeasonsGetter,
                _tableClientFactory.TableStorageLastestSeasonsUpdater,
                token)
            .SentEvents(_domainPostman.SendMessages, message.Season, token)
            .AddLogOnSuccess(LogSuccess)
            .Complete(_logger);
    }

    private static Action<ILogger> LogSuccess(SeasonUpdateResult data) => logger =>
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
            case (SeasonUpdateStatus.New, false):
                logger.LogInformation("{Year}-{Season} has been added to the system as new season",
                    data.Season.Year, data.Season.Season);
                break;
            default:
                logger.LogInformation("{Year}-{Season} has been updated",
                    data.Season.Year, data.Season.Season);
                break;
        }
    };
}