using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public delegate Task<Result<ScrapTvLibraryData>> TvScrapper(SeasonSelector season, CancellationToken token);

public interface ITvLibraryScrapper
{
    Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default);
}

internal sealed class TvLibraryScrapper : ITvLibraryScrapper
{
    private readonly ISeasonFeedDataProvider _seasonFeedDataProvider;
    private readonly ITableClientFactory _tableClientFactory;
    private readonly IAnimeScheduleClient _animeScheduleClient;

    public TvLibraryScrapper(
        ISeasonFeedDataProvider seasonFeedDataProvider,
        ITableClientFactory tableClientFactory,
        IAnimeScheduleClient animeScheduleClient)
    {
        _seasonFeedDataProvider = seasonFeedDataProvider;
        _tableClientFactory = tableClientFactory;
        _animeScheduleClient = animeScheduleClient;
    }

    public Task<Result<ScrapTvLibraryData>> ScrapTvSeries(SeasonSelector season, CancellationToken token = default)
    {
        return ResolveSeason(season, _tableClientFactory.TableStorageLatestSeason, token)
            .Bind(resolved => _seasonFeedDataProvider.Get()
                .ScrapSeries(_animeScheduleClient, resolved, token))
            .AddDataFromStorage(_tableClientFactory.TableStorageExistentStoredSeries, token);
    }

    // The featured season is elected by hand, so the current season is whichever one storage holds
    // as latest. Scraping never elects it, so every season resolved here carries IsLatest false.
    internal static Task<Result<SeriesSeason>> ResolveSeason(
        SeasonSelector selector,
        LatestSeasonGetter latestSeason,
        CancellationToken token) =>
        selector switch
        {
            Current => latestSeason(token).Bind(stored => stored switch
            {
                CurrentLatestSeason latest => AsSeriesSeason(latest.Season),
                FallbackLatestSeason newest => AsSeriesSeason(newest.Season),
                _ => Error.Create("There is no latest season data in storage.")
            }),
            BySeason bySeason => Task.FromResult<Result<SeriesSeason>>(
                new SeriesSeason(bySeason.Season, bySeason.Year)),
            _ => throw new UnreachableException()
        };

    private static Result<SeriesSeason> AsSeriesSeason(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, false).ParseAsSeriesSeason();
}
