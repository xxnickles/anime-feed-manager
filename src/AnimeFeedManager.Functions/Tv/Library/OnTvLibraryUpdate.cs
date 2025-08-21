using AnimeFeedManager.Features.Common;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Tv.Library.Events;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess;
using AnimeFeedManager.Features.Tv.Library.Storage;

namespace AnimeFeedManager.Functions.Tv.Library;

public class OnTvLibraryUpdate
{
    private readonly ITvLibraryScrapper _scrapper;
    private readonly IImageProvider _imageProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IDomainPostman _domainPostman;
    private readonly ILogger<OnTvLibraryUpdate> _logger;

    public OnTvLibraryUpdate(
        ITvLibraryScrapper scrapper,
        IImageProvider imageProvider,
        ITableClientFactory tableClientFactory,
        IDomainPostman domainPostman,
        ILogger<OnTvLibraryUpdate> logger)
    {
        _scrapper = scrapper;
        _imageProvider = imageProvider;
        _tableClientFactory = tableClientFactory;
        _domainPostman = domainPostman;
        _logger = logger;
    }

    [Function(nameof(OnTvLibraryUpdate))]
    public async Task Run(
        [QueueTrigger(UpdateTvSeriesEvent.TargetQueue, Connection = StorageRegistrationConstants.QueueConnection)]
        UpdateTvSeriesEvent message,
        CancellationToken token)
    {
        using var tracedActivity = message.StartTracedActivity(nameof(OnTvLibraryUpdate));
        await TryGetSeasonSelector(message.SeasonParameters)
            .ScrapTvSeries(_scrapper, token)
            .AddImagesLinks(_imageProvider, _logger, token)
            .UpdateTvLibrary(_tableClientFactory.GetTvLibraryUpdater(), token)
            .SendEvents(_domainPostman, message.SeasonParameters, token)
            .Match(
                results => _logger.LogInformation(
                    "Season {Year}-{Season} tv series has been updated. {UpdatedSeries} has been updated and {NewSeries} has been added",
                    results.Season.Year, results.Season.Season, results.UpdatedSeries, results.NewSeries),
                e => e.LogError(_logger)
            );
    }

    private static Result<SeasonSelector> TryGetSeasonSelector(SeasonParameters? season)
    {
        if (season is null)
            return Result<SeasonSelector>.Success(new Latest());

        return (season.Season, season.Year, false)
            .ParseAsSeriesSeason()
            .Map<SeasonSelector>(parsedSeason => new BySeason(parsedSeason.Season, parsedSeason.Year));
    }
}