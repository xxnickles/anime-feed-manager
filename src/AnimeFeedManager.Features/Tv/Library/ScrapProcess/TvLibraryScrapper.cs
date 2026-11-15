using AnimeFeedManager.Features.Scrapping;
using AnimeFeedManager.Features.Scrapping.AnimeSchedule;
using AnimeFeedManager.Features.Scrapping.Types;
using AnimeFeedManager.Features.Seasons.Queries;
using AnimeFeedManager.Features.Tv.Library.ScrapProcess.Static;

namespace AnimeFeedManager.Features.Tv.Library.ScrapProcess;

[Widens<LatestSeasonError>]
public partial union TvScrapError(StorageError, NoLatestSeason, ScrapeError);

public delegate Task<Result<ScrapTvLibraryData, TvScrapError>> TvScrapper(SeasonSelector season, CancellationToken token);

public static class TvLibraryScrapper
{
    public static TvScrapper Scrapper(
        IAnimeScheduleClient animeScheduleClient,
        LatestSeasonGetter latestSeasonGetter,
        StoredSeriesGetter storedSeriesGetter
    ) => (season, token) => {
        return ResolveSeason(season, latestSeasonGetter, token)
            .MapError(error => (TvScrapError)error)
            .Bind(resolved => AnimeScheduleTvScrapper.ScrapSeries(animeScheduleClient, resolved, token))
            .AddDataFromStorage(storedSeriesGetter, token);
    };

    // The featured season is elected by hand, so the current season is whichever one storage holds
    // as latest. Scraping never elects it, so every season resolved here carries IsLatest false.
    internal static Task<Result<SeriesSeason, LatestSeasonError>> ResolveSeason(
        SeasonSelector selector,
        LatestSeasonGetter latestSeason,
        CancellationToken token) =>
        selector switch
        {
            Current => SeasonQueries.GetLatestSeason(latestSeason, token).Map(season => season with {IsLatest = false}),
            BySeason bySeason => Task.FromResult(
                Result<SeriesSeason, LatestSeasonError>.Success(new SeriesSeason(bySeason.Season, bySeason.Year))),
            _ => throw new UnreachableException()
        };
}