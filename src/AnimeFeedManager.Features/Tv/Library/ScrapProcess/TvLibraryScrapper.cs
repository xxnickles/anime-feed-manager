using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.UpdateProcess;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

public delegate Task<Result<ScrapTvLibraryData, DomainError>> TvScrapper(SeasonSelector season, CancellationToken token);

public static class TvLibraryScrapper
{
    public static TvScrapper Scrapper(
        IAnimeScheduleClient animeScheduleClient,
        LatestSeasonGetter latestSeasonGetter,
        StoredSeriesGetter storedSeriesGetter
    ) => (season, token) => {
        return ResolveSeason(season, latestSeasonGetter, token)
            .Bind(resolved => AnimeScheduleTvScrapper.ScrapSeries(animeScheduleClient, resolved, token))
            .AddDataFromStorage(storedSeriesGetter, token);
    };

    // The featured season is elected by hand, so the current season is whichever one storage holds
    // as latest. Scraping never elects it, so every season resolved here carries IsLatest false.
    internal static Task<Result<SeriesSeason, DomainError>> ResolveSeason(
        SeasonSelector selector,
        LatestSeasonGetter latestSeason,
        CancellationToken token) =>
        selector switch
        {
            Current => latestSeason(token).MapError(error => (DomainError)error).Bind(stored => stored switch
            {
                CurrentLatestSeason latest => AsSeriesSeason(latest.Season),
                FallbackLatestSeason newest => AsSeriesSeason(newest.Season),
                _ => new NoLatestSeason()
            }),
            BySeason bySeason => Task.FromResult<Result<SeriesSeason, DomainError>>(
                new SeriesSeason(bySeason.Season, bySeason.Year)),
            _ => throw new UnreachableException()
        };

    private static Result<SeriesSeason, DomainError> AsSeriesSeason(SeasonStorage season) =>
        (season.Season ?? string.Empty, season.Year, false).ParseAsSeriesSeason();
}