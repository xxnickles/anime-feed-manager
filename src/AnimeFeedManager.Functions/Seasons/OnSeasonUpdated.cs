using AnimeFeedManager.Features.Seasons.Events;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;

namespace AnimeFeedManager.Functions.Seasons;

public class OnSeasonUpdated
{
    private readonly ITableClientFactory _tableClientFactory;
    private readonly ILogger<OnSeasonUpdated> _logger;

    public OnSeasonUpdated(
        ITableClientFactory tableClientFactory,
        ILogger<OnSeasonUpdated> logger)
    {
        _tableClientFactory = tableClientFactory;
        _logger = logger;
    }

    [Function(nameof(OnSeasonUpdated))]
    public async Task Run(
        [QueueTrigger(SeasonUpdated.TargetQueue, Connection = Constants.AzureConnectionName)]
        SeasonUpdated message,
        CancellationToken token)
    {
        await SeasonUpdate.CheckSeasonExist(_tableClientFactory.SeasonGetter(), message.Season, token)
            .CreateNewSeason()
            .AddLatestSeasonData(_tableClientFactory.LatestSeasonGetter(), token)
            .StoreUpdatedSeason(_tableClientFactory.SeasonUpdater(), token)
            .DemoteCurrentLatest(_tableClientFactory.SeasonUpdater(), token)
            .UpdateLast4Season(
                _tableClientFactory.AllSeasonsGetter(),
                _tableClientFactory.LastestSeasonsUpdater(),
                data => data.SeasonData is not NoUpdateRequired,
                token)
            .Match(r => LogSuccess(r, _logger), 
                e => e.LogError(_logger));
    }

    private static void LogSuccess(SeasonUpdateData data, ILogger logger)
    {
        switch (data.SeasonData)
        {
            case NoUpdateRequired:
                logger.LogInformation("{Year}-{Season} already exist in the system. No actions have been taken",
                    data.SeasonToUpdate.Year, data.SeasonToUpdate.Season);
                break;
            case LatestSeason:
                logger.LogInformation("{Year}-{Season} has been added to the system as latest season",
                    data.SeasonToUpdate.Year, data.SeasonToUpdate.Season);
                break;
            case NewSeason:
                logger.LogInformation("{Year}-{Season} has been added to the system as new season",
                    data.SeasonToUpdate.Year, data.SeasonToUpdate.Season);
                break;
            default:
                logger.LogInformation("{Year}-{Season} has been updated",
                    data.SeasonToUpdate.Year, data.SeasonToUpdate.Season);
                break;
        }
    }
}