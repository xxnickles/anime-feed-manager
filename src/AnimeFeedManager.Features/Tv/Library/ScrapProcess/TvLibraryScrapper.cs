using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.SubsPlease;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Storage;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public delegate Task<Result<ScrapTvLibraryData>> TvScrapper(SeasonSelector season, CancellationToken token);

public static class TvLibraryScrapper
{
    public static TvScrapper Scrapper(
        IAnimeScheduleClient animeScheduleClient,
        ISeasonFeedDataProvider seasonFeedDataProvider,
        LatestSeasonGetter latestSeasonGetter,
        StoredSeriesGetter storedSeriesGetter
    ) => (season, token) => {
        return ResolveSeason(season, latestSeasonGetter, token)
            .Bind(resolved => seasonFeedDataProvider.Get()
                .ScrapSeries(animeScheduleClient, resolved, token))
            .AddDataFromStorage(storedSeriesGetter, token);
    };

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